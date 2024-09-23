using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
    // Important class cannot have more than 1 constructor (see Device Factory)
    public abstract partial class Device<T1, T2> : IDevice<T1> where T1 : Enum where T2 : IConnectionProperties
    {
        // used if you want to use duplicate devices
        public string Id
        {
            get => Id;
            set => Id = value;
        }

        protected readonly IConnectionProperties _connectionProperties;
        protected IDictionary<T1, Action<T1, Enum>> _inputSubscriptions;
        protected IConnection _connection;

        protected Device(
            IDictionary<Enum, InputEvent> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null
        )
        {
            _inputSubscriptions = CreateTypedDictionary(inputSubscriptions);
            _connectionProperties = (null != connectionProperties) ?
                GetDefaultConnectionProperties().UpdateProperties(connectionProperties) :
                GetDefaultConnectionProperties();
            _connection = ConnectionFactory.GetConnection(this, _connectionProperties);
            Id = _connectionProperties.Id;
        }

        public IConnectionProperties GetConnectionProperties() => _connectionProperties;

        public void SetInputCallbacks(IDictionary<T1, Action<T1, Enum>> inputSubscriptions)
        {
            _inputSubscriptions = inputSubscriptions;
        }

        public void AddInputCallback(T1 interactionZone, Action<T1, Enum> callback)
        {
            _inputSubscriptions[interactionZone] = callback;
        }

        public async Task<IDevice> Connect()
        {
            await _connection.Connect();
            return (IDevice)this;
        }
        public void Disconnect()
        {
            _connection.Disconnect();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException();
        }

        public IConnection GetConnection()
        {
            return _connection;
        }

        public DeviceClassification GetClassification()
        {
            return GetDeviceClassification();
        }

        public bool CanConnect(IDevice device)
        {
            return _connection.CanConnect(device.GetConnection());
        }
        public abstract void ResetState();

        public abstract Task OnStartWrite();

        public abstract void ReadData(byte[] data);

        public abstract Task Write(params Enum[] interactions);

        private static IDictionary<T1, Action<T1, Enum>> CreateTypedDictionary(IDictionary<Enum, InputEvent> original)
        {
            var typedDictionary = new Dictionary<T1, Action<T1, Enum>>();
            foreach (var kvp in original)
            {
                T1 key = (T1)kvp.Key;
                Action<T1, Enum> value = (a1, a2) =>
                {
                    kvp.Value((Enum)(object)a1, (Enum)(object)a2);
                };
                typedDictionary[key] = value;
            }

            return typedDictionary;
        }
    }
}