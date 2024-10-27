using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;

namespace MychIO.Connection
{

    // Important class cannot have more than 1 constructor (see Connection Factory)
    // TODO: Improve the interface to include IsReading, and StopReading methods
    public abstract partial class Connection : IConnection
    {
        protected IDevice _device;
        protected IConnectionProperties _connectionProperties;
        protected IOManager _manager;

        public Connection(
            IDevice device,
            IConnectionProperties connectionProperties,
            IOManager manager
         )
        {
            _device = device;
            _connectionProperties = connectionProperties;
            _manager = manager;
        }

        public abstract Task Connect();

        public abstract Task Disconnect();

        public abstract bool IsConnected();

        public abstract Task Write(byte[] bytes);

        // This is used to prevent the same physical device from being connected
        // to twice e.g. COM3 then you need to override this and check for that
        // all devices connected are passed to this method so you must check instance type!
        public abstract bool CanConnect(IConnection connectionProperties);

        public abstract bool IsReading();

        public abstract void Read();

        public abstract void StopReading();

        protected void ValidateConnectionProperties<T>() where T : ConnectionProperties
        {
            if (_connectionProperties is not T)
            {
                _manager.handleEvent(
                    IOEventType.ConnectionError,
                    _device.GetClassification(),
                    $"{_device.GetType().Name} Invalid properties object passed should be {typeof(T).Name} got {_connectionProperties.GetType().Name}"
                );
            }
        }

    }

}