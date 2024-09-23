using System.Threading.Tasks;
using MychIO.Device;

namespace MychIO.Connection
{

    // Important class cannot have more than 1 constructor (see Connection Factory)
    public abstract partial class Connection : IConnection
    {
        protected IDevice _device;
        protected IConnectionProperties _connectionProperties;

        public Connection(
            IDevice device,
            IConnectionProperties connectionProperties
         )
        {
            _device = device;
            _connectionProperties = connectionProperties;
        }

        public abstract Task Connect();

        public abstract void Disconnect();

        public abstract bool IsConnected();

        public abstract Task Write(byte[] bytes);

        // This is used to prevent the same physical device from being connected
        // to twice e.g. COM3 then you need to override this and check for that
        // all devices connected are passed to this method so you must check instance type!
        public abstract bool CanConnect(IConnection _connectionProperties);

    }

}