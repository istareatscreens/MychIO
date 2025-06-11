using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;

namespace MychIO.Connection
{

    // Important class cannot have more than 1 constructor (see Connection Factory)
    // TODO: Improve the interface to include IsReading, and StopReading methods
    public abstract partial class Connection : IConnection
    {
        public abstract bool IsConnected { get; }
        public abstract bool IsReading { get; }

        protected IList<IDevice> _devices;
        protected IConnectionProperties _connectionProperties;
        protected IOManager _manager;
        protected DeviceClassification _classification;
        protected string _types;

        public Connection(
            IList<IDevice> devices,
            IConnectionProperties connectionProperties,
            IOManager manager
         )
        {
            _devices = devices;
            _connectionProperties = connectionProperties;
            _manager = manager;
            _classification = devices.Count == 1 ? devices.First().Classification : DeviceClassification.MultipleDevice;
            _types = string.Join(", ", devices.Select(d => d.GetType().ToString()));
        }

        public abstract void Connect();
        public abstract Task ConnectAsync();
        public abstract void Disconnect();
        public abstract Task DisconnectAsync();
        public abstract void Write(ReadOnlySpan<byte> data);
        public abstract Task WriteAsync(byte[] bytes);
        public abstract Task WriteAsync(ReadOnlyMemory<byte> data);

        // This is used to prevent the same physical device from being connected
        // to twice e.g. COM3 then you need to override this and check for that
        // all devices connected are passed to this method so you must check instance type!
        public abstract bool CanConnect(IConnection connectionProperties);

        public abstract void Read();

        public abstract void StopReading();
        public abstract void Dispose();

        protected void OnDeviceDisconnected()
        {
            foreach (var device in _devices)
            {
                device?.OnDisconnected();
            }
        }

        protected async Task OnDeviceDisconnectedAsync()
        {
            foreach (var device in _devices)
            {
                await device.OnDisconnectedAsync();
            }
        }

        protected void OnDeviceResetState()
        {
            foreach (var device in _devices)
            {
                device.ResetState();
            }
        }


        protected void OnDeviceConnected()
        {
            foreach (var device in _devices)
            {
                device.OnConnected();
            }
        }

        protected void ValidateConnectionProperties<T>() where T : ConnectionProperties
        {
            if (_connectionProperties is not T)
            {
                foreach (var device in _devices)
                {
                    _manager.handleEvent(
                        IOEventType.ConnectionError,
                        device.Classification,
                        $"{device.GetType().Name} Invalid properties object passed should be {typeof(T).Name} got {_connectionProperties.GetType().Name}"
                    );
                }
            }
        }
    }
}