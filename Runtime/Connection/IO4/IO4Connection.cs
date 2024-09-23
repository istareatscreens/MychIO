using System.Threading.Tasks;
using MychIO.Device;

namespace MychIO.Connection.IO4
{
    public class IO4Connection : Connection
    {

        private int _pollTimeoutMs;

        public IO4Connection(IDevice device, IConnectionProperties connectionProperties) :
         base(device, connectionProperties)
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