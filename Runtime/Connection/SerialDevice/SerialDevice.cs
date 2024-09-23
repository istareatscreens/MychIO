using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;

namespace MychIO.Connection.SerialDevice
{
    public class SerialDeviceConnection : Connection
    {

        private SerialPort _serialPort;
        private int _pollTimeoutMs;
        private int _bufferByteLength;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public SerialDeviceConnection(IDevice device, IConnectionProperties connectionProperties) :
         base(device, connectionProperties)
        { }

        public new static ConnectionType GetConnectionType() => ConnectionType.SerialDevice;

        public override Task Connect()
        {
            if (!(_connectionProperties is SerialDeviceProperties))
            {
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

        public override bool CanConnect(IConnection connectionProperties)
        {
            return !(connectionProperties is SerialDeviceProperties) ||
             ((SerialDeviceProperties)connectionProperties).ComPortNumber !=
              ((SerialDeviceProperties)_connectionProperties).ComPortNumber;
        }

    }

}