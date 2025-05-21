using System;
using System.IO;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;

namespace MychIO.Connection.SerialDevice
{
    public class SerialDeviceConnection : Connection
    {
        public override bool IsConnected
        {
            get => _serialPort?.IsOpen ?? false;
        }
        public override bool IsReading
        {
            get => !_readDataLoop.IsCompleted;
        }
        public new static ConnectionType GetConnectionType() => ConnectionType.SerialDevice;

        delegate void ReceiveDataHandler(ReadOnlySpan<byte> data);

        private SerialPort _serialPort;
        private int _pollTimeoutMs;
        private int _bufferByteLength;

        Task _readDataLoop = Task.CompletedTask;
        private CancellationTokenSource _cts = new CancellationTokenSource();

        ReceiveDataHandler _onReceiveData;
        public SerialDeviceConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        {
            _onReceiveData = _device.ReadData;
        }

        public override void Connect()
        {
            ConnectAsync().Wait();
        }
        public override Task ConnectAsync()
        {
            if (IsConnected)
            {
                // TODO: Set event here
                return Task.CompletedTask;
            }

            ValidateConnectionProperties<SerialDeviceProperties>();

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
                ReadTimeout = 0 == serialDeviceProperties.ReadTimeoutMS ? SerialPort.InfiniteTimeout :
                                                                          serialDeviceProperties.ReadTimeoutMS,
                Handshake = (System.IO.Ports.Handshake)serialDeviceProperties.Handshake,
                RtsEnable = serialDeviceProperties.Rts,
                DtrEnable = serialDeviceProperties.Dtr
            };

            // Functionality to detect attach and detach device is not present on 
            // .net SerialPort could potentially add using this method:
            // https://stackoverflow.com/questions/13408476/detecting-when-a-serialport-gets-disconnected
            _serialPort.Open();

            if (!IsConnected)
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.Classification, _device.GetType().ToString() + " Device lost COM port connection");
                return Task.CompletedTask;
            }

            StartReadDataLoop();

            if(IsReading)
            {
                _manager.handleEvent(IOEventType.Attach, _device.Classification, _device.GetType().ToString() + " Device connected");
            }

            return Task.CompletedTask;
        }
        public override void Disconnect()
        {
            DisconnectAsync().Wait();
        }
        public override async Task DisconnectAsync()
        {
            _device.ResetState();
            if (IsReading)
            {
                StopReadPollingAsync();
            }
            if (IsConnected)
            {
                await _device.OnDisconnectedAsync();
                _serialPort?.Close();
            }
            _serialPort = null;
            _manager.handleEvent(IOEventType.Detach, _device.Classification, _device.GetType().ToString() + "device disconnected");
        }
        
        public override void Write(ReadOnlySpan<byte> data)
        {
            EnsureSerialPortIsOpen(_serialPort);
            _serialPort.BaseStream.Write(data);
        }
        public async override Task WriteAsync(byte[] data)
        {
            await WriteAsync(data.AsMemory());
        }
        public override async Task WriteAsync(ReadOnlyMemory<byte> data)
        {
            EnsureSerialPortIsOpen(_serialPort);
            await _serialPort.BaseStream.WriteAsync(data);
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            return !(connectionProperties is SerialDeviceProperties) ||
             ((SerialDeviceProperties)connectionProperties).ComPortNumber !=
              ((SerialDeviceProperties)_connectionProperties).ComPortNumber;
        }

        public override void Read()
        {
            if (!IsReading)
            {
                if(_cts is not null)
                {
                    // Dispose the old one if it's not null
                    _cts.Cancel();
                }
                _cts = new CancellationTokenSource();
                StartReadDataLoop();
            }
        }

        public override void StopReading()
        {
            StopReadPollingAsync().Wait();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureSerialPortIsOpen(SerialPort serialSession)
        {
            if (!serialSession.IsOpen)
            {
                serialSession.Open();
                _device.OnConnected();
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        void ReadFromSerialPort(SerialPort serialPort,ReceiveDataHandler readDataCallback)
        {
            var bytes2Read = _serialPort.BytesToRead;
            if (0 == bytes2Read)
            {
                return;
            }
            Span<byte> buffer = stackalloc byte[bytes2Read];
            var read = serialPort.Read(buffer);
            if (read < _bufferByteLength)
            {
                // Handle case where not enough data to read
                return;
            }
            readDataCallback(buffer);
        }
        void StartReadDataLoop()
        {
            if (IsReading)
            {
                return;
            }
            var dt = _connectionProperties.GetDebounceThreshold();
            if(dt.TotalMilliseconds > 0)
            {
                _onReceiveData = _device.ReadDataWithDebounce;
            }
            else
            {
                _onReceiveData = _device.ReadData;
            }
            _readDataLoop = Task.Factory.StartNew(() =>
            {
                ReadDataLoop(_onReceiveData);
            }, TaskCreationOptions.LongRunning);
        }
        void ReadDataLoop(ReceiveDataHandler receiveDataHandler)
        {
            _device.OnConnected();
            try
            {
                var token = _cts.Token;
                while (true)
                {
                    EnsureSerialPortIsOpen(_serialPort);
                    ReadFromSerialPort(_serialPort, receiveDataHandler);
                    token.ThrowIfCancellationRequested();
                    Thread.Sleep(_pollTimeoutMs);
                }
            }
            catch (OperationCanceledException)
            {
                // Nothing to do here event was sent to detach
            }
            catch (Exception e)
            {
                // Throw event here potentially in the future for now just disconnect
                _manager.handleEvent(IOEventType.ConnectionError, _device.Classification, _device.GetType().ToString() + "device connection failed due to following exception: " + e);
                Disconnect();
            }
        }
        async Task StopReadPollingAsync()
        {
            _cts.Cancel();
            await _readDataLoop;
        }
        public override void Dispose()
        {
            _device?.OnDisconnected();
            _cts.Cancel();
        }
    }
    static class SerialPortExtensions
    {
        public static int Read(this SerialPort serial, Span<byte> buffer)
        {
            var byte2Read = serial.BytesToRead;
            var read = 0;
            for (; read < buffer.Length; read++)
            {
                if (read == byte2Read)
                {
                    break;
                }
                buffer[read] = (byte)serial.ReadByte();
            }
            return read;
        }
    }
}