
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;

namespace MychIO.Connection.HID
{
    public class HIDConnection : Connection
    {

        private SerialPort _serialPort;
        private int _pollTimeoutMs;
        private int _bufferByteLength;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public HIDConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        { }


        public new static ConnectionType GetConnectionType() => ConnectionType.SerialDevice;

        public override Task Connect()
        {
            return Task.CompletedTask;
        }

        private void StopReadPolling() { }

        public override void Disconnect() { }

        public override bool IsConnected() { return true; }

        public override Task Write(byte[] data)
        {
            return Task.CompletedTask;
        }

        public override bool CanConnect(IConnection _connectionProperties)
        {
            return true;
        }
    }

}