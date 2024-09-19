
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;

namespace MychIO.Connection.SerialDevice
{
    public class SerialDevice : Connection
    {

        private SerialPort _serialPort;
        private int _pollTimeoutMs;
        private int _bufferByteLength;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public SerialDevice(IDevice device, IConnectionProperties connectionProperties) :
         base(device, connectionProperties)
        {
        }

        public override Task Connect()
        {
            if(!(_connectionProperties is SerialDeviceProperties)){
                throw new Exception("Invalid connection object passed to SerialDevice class");
            }
            var serialDeviceProperties = (SerialDeviceProperties)_connectionProperties;

            _pollTimeoutMs = serialDeviceProperties.PollTimeoutMs;
            _bufferByteLength = serialDeviceProperties.BufferByteLength;
            _serialPort = new SerialPort(serialDeviceProperties.ComPortNumber)
            {
                BaudRate = (int)serialDeviceProperties.BaudRate,
                Parity = (System.IO.Ports.Parity)serialDeviceProperties.ParityBit,
                StopBits = (System.IO.Ports.StopBits)serialDeviceProperties.StopBit,
                DataBits = (int)serialDeviceProperties.DataBits,
                WriteTimeout = 0 == serialDeviceProperties.WriteTimeoutMS ?
                    SerialPort.InfiniteTimeout :
                    serialDeviceProperties.WriteTimeoutMS,
                Handshake = (System.IO.Ports.Handshake)serialDeviceProperties.Handshake,
                RtsEnable = serialDeviceProperties.Rts,
                DtrEnable = serialDeviceProperties.Dtr
            };

            // Functionality to detect attach and detach device is not present on 
            // .net SerialPort could potentially add using this method:
            // https://stackoverflow.com/questions/13408476/detecting-when-a-serialport-gets-disconnected
            _serialPort.Open();

            Task.Run(async () =>
            {
                await _device.OnStartWrite();
                await RecieveData();
            });

            return Task.CompletedTask;
        }

        private async Task RecieveData()
        {
            try
            {
                byte[] buffer = new byte[_bufferByteLength];
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(_pollTimeoutMs, _cancellationTokenSource.Token);

                    int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, _bufferByteLength);
                    if (bytesRead == 0) { continue; } // Handle case where no data is read
                    _device.ReadData(buffer);
                }
            }
            catch (TaskCanceledException)
            {
                // Nothing to do here event was sent to detach
            }
            catch (Exception)
            {
                // Throw event here potentially in the future for now just disconnect
                Disconnect();
            }
        }

        private void StopReadPolling()
        {
            _cancellationTokenSource.Cancel();
        }

        public override void Disconnect()
        {
            _device.ResetState();
            StopReadPolling();
            if (IsConnected())
            {
                _serialPort.Close();
            }
            _serialPort = null;
        }

        public override bool IsConnected()
        {
            return _serialPort.IsOpen;
        }

        public async override Task Write(byte[] data)
        {
            await _serialPort.BaseStream.WriteAsync(data, 0, data.Length);
        }

    }

}

/*
        public SerialDevice(IDevice device){
            _device = device; 
        }

        public Task Connect()
        {
            _serialPort = new SerialPort(_deviceName)
            {
                BaudRate = _connectionProperties.GetBaudRate(),
                Parity = _connectionProperties.GetParityBit(),
                StopBits = _connectionProperties.GetStopBit(),
                DataBits = _connectionProperties.GetDataBits(),
                WriteTimeout = 0 == _connectionProperties.WriteTimeoutMS ?
                    SerialPort.InfiniteTimeout :
                    _connectionProperties.WriteTimeoutMS,
                Handshake = _connectionProperties.GetHandshake(),
                RtsEnable = _connectionProperties.GetRts(),
                DtrEnable = _connectionProperties.GetDtr()
            };


            // Functionality to detect attach and detach device is not present on 
            // .net SerialPort could potentially add using this method:
            // https://stackoverflow.com/questions/13408476/detecting-when-a-serialport-gets-disconnected
            _serialPort.Open();
            */

            // Attach callback
            // Not implemented https://issuetracker.unity3d.com/issues/serialport-bytestoread-returns-null-reference
            // _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(SerialErrorReceivedEventHandler);
            // _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

/*
            Task.Run(async () =>
            {
                await _decoder.OnStartWrite(this);
                await RecieveData();
            });

            _adxControllerObservable.handleEvent(
                ControllerEventType.Attach,
                _deviceName,
                _deviceType
            );
            throw new System.NotImplementedException();
        }

        public Task Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public ConnectionProperties GetConnectionProperties()
        {
            throw new System.NotImplementedException();
        }

        public string GetDeviceName()
        {
            throw new System.NotImplementedException();
        }

        public bool IsConnected()
        {
            throw new System.NotImplementedException();
        }

        public Task Write()
        {
            throw new System.NotImplementedException();
        }
    }
}
*/

/*
using AdxControllerConnector.Interaction;
using AdxControllerConnector.Manager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using AdxControllerConnector.Decoder;
using AdxControllerConnector.Event;
using System.Threading;
using Codice.Utils.Buffers;
using System.Text;
using System.IO.Ports;


namespace AdxControllerConnector.SerialDevice
{
    public class DesktopSerialDevice<T> : ISerialDevice where T : Enum
    {
        private const int POLL_TIMEOUT_MS = 100;

        private readonly string _deviceName;
        private readonly AdxControllerDevice _deviceType;
        private readonly ConnectionProperties _connectionProperties;

        private readonly AdxControllerDesktopObservable _adxControllerObservable;
        private readonly IDecoder<T> _decoder;
        private SerialPort _serialPort;

        // Serial Device Read Properties
        private readonly int _bufferSize;
        private readonly byte[] _buffer;
        private int _currentOffset = 0;

        private TaskCompletionSource<byte[]> _readTaskComplete = new TaskCompletionSource<byte[]>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public DesktopSerialDevice(
            // callbacks and device interaction
            AdxControllerDesktopObservable adxControllerConnector,
            // device properties
            string deviceName,
            AdxControllerDevice deviceType,
            // settings
            ConnectionProperties connectionProperties,
            IDecoder<T> decoder
            )
        {
            _adxControllerObservable = adxControllerConnector;
            _decoder = decoder;

            // Set buffer
            _bufferSize = connectionProperties.BufferBitLength;
            _buffer = new byte[_bufferSize * 2];

            _deviceType = deviceType;
            _deviceName = deviceName;
            _connectionProperties = connectionProperties;
            Attach();
        }

        public void Attach()
        {
            _serialPort = new SerialPort(_deviceName)
            {
                BaudRate = _connectionProperties.GetBaudRate(),
                Parity = _connectionProperties.GetParityBit(),
                StopBits = _connectionProperties.GetStopBit(),
                DataBits = _connectionProperties.GetDataBits(),
                WriteTimeout = 0 == _connectionProperties.WriteTimeoutMS ?
                    SerialPort.InfiniteTimeout :
                    _connectionProperties.WriteTimeoutMS,
                Handshake = _connectionProperties.GetHandshake(),
                RtsEnable = _connectionProperties.GetRts(),
                DtrEnable = _connectionProperties.GetDtr()
            };


            // Functionality to detect attach and detach device is not present on 
            // .net SerialPort could potentially add using this method:
            // https://stackoverflow.com/questions/13408476/detecting-when-a-serialport-gets-disconnected
            _serialPort.Open();

            // Attach callback
            // Not implemented https://issuetracker.unity3d.com/issues/serialport-bytestoread-returns-null-reference
            // _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(SerialErrorReceivedEventHandler);
            // _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            Task.Run(async () =>
            {
                await _decoder.OnStartWrite(this);
                await RecieveData();
            });

            _adxControllerObservable.handleEvent(
                ControllerEventType.Attach,
                _deviceName,
                _deviceType
            );
        }

        private async Task RecieveData()
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(POLL_TIMEOUT_MS, _cancellationTokenSource.Token);

                    int bytesRemaining = _buffer.Length - _currentOffset;
                    int bytesRead = await _serialPort.BaseStream.ReadAsync(_buffer, _currentOffset, bytesRemaining);
                    if (bytesRead == 0) continue; // Handle case where no data is read

                    _currentOffset += bytesRead;

                    if (_currentOffset >= _bufferSize || (0 == bytesRead && _currentOffset != 0))
                    {
                        var completeBuffer = new byte[_bufferSize];
                        Array.Copy(_buffer, completeBuffer, _bufferSize);

                        // Decode input
                        _adxControllerObservable.handleEvent(ControllerEventType.Debug, message: $"GOT SOME DATA: {Encoding.ASCII.GetString(completeBuffer)} END");
                        _decoder.Handle(completeBuffer);

                        int leftover = _currentOffset - _bufferSize;
                        if (leftover > 0)
                        {
                            Array.Copy(_buffer, _bufferSize, _buffer, 0, leftover);
                            _currentOffset = leftover;
                            continue;
                        }
                        _currentOffset = 0;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Nothing to do here event will be sent from the detach
            }
            catch (Exception ex)
            {
                _adxControllerObservable.handleEvent(
                    ControllerEventType.SerialDeviceReadError,
                    _deviceName,
                    _deviceType,
                    ex.Message
                );
            }
        }

        public void StopReadPolling()
        {
            _cancellationTokenSource.Cancel();
        }

        public Task<bool> Close()
        {
            return Task.Run(() =>
            {
                if (!IsConnected())
                {
                    return Task.FromResult(false);
                }
                _serialPort.Close();
                return Task.FromResult(true);
            });
        }

        public void Detach()
        {
            StopReadPolling();
            if (IsConnected())
            {
                Close().Wait();
            }
            _serialPort = null;
            _adxControllerObservable.handleEvent(
            ControllerEventType.Detach,
            _deviceName,
            _deviceType
        );
        }

        public bool IsConnected()
        {
            return _serialPort?.IsOpen ?? false;
        }

        public Task<bool> Write(string message)
        {
            return Task.Run(() =>
            {
                if (!IsConnected())
                {
                    return false;
                }
                _serialPort.Write(message);
                return true;
            });
        }
        public string GetDeviceName() => _deviceName;
        public AdxControllerDevice GetDeviceType() => _deviceType;
        public ConnectionProperties GetConnectionProperties() => _connectionProperties;

    }

}
*/
