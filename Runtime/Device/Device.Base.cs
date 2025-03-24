using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Helper;

namespace MychIO.Device
{
    // Important class cannot have more than 1 constructor (see Device Factory)
    public abstract partial class Device<TZone, TState, TConnProps> : IDevice<TZone, TState> 
        where TZone : Enum
        where TState : Enum
        where TConnProps : IConnectionProperties 
    {
        public abstract string Name { get; }
        public abstract bool CanRead { get; }
        public abstract bool CanWrite { get; }
        public bool IsReading
        {
            get => _connection.IsReading;
        }
        
        public virtual bool IsConnected
        {
            get => ThrowHelper.NotImplemented<bool>("Should be implemented by base class");
        }
        public IConnection Connection
        {
            get => _connection;
        }
        public DeviceClassification Classification
        {
            get => _classification;
        }
        public IConnectionProperties ConnectionProperties
        {
            get => _connectionProperties;
        }

        protected const byte MOST_SIGNIFICANT_BIT = 0b10000000;
        protected const byte LEAST_SIGNIFICANT_BIT = 0b00000001;
        private string _id;
        public string Id
        {
            get => _id;
            set => _id = value;
        }

        protected readonly IOManager _manager;
        protected readonly IConnectionProperties _connectionProperties;
        protected IDictionary<TZone, Action<TZone, TState>> _inputSubscriptions;
        protected IConnection _connection;
        protected DeviceClassification _classification;

        protected Device(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        )
        {
            // pull base class static methods
            var defaultProperties = (IConnectionProperties)GetBaseClassStaticMethod("GetDefaultConnectionProperties", GetType()).Invoke(null, null);
            _classification = (DeviceClassification)GetBaseClassStaticMethod("GetDeviceClassification", GetType()).Invoke(null, null);

            if (0 == defaultProperties.Properties.Count)
            {
                manager.handleEvent(
                    Event.IOEventType.InvalidDevicePropertyError,
                    _classification, "DefaultProperties are empty potential issue with GetDefaultConnectionProperties method"
                 );
            }

            // construct
            _inputSubscriptions = CreateTypedDictionary(inputSubscriptions);
            _connectionProperties = (null != connectionProperties) ?
                defaultProperties.UpdateProperties(connectionProperties) :
                defaultProperties;

            // send errors that occured when applying properties
            foreach (var error in _connectionProperties.Errors)
            {
                manager.handleEvent(Event.IOEventType.InvalidDevicePropertyError, _classification, error);
            }

            // Connect
            _connection = ConnectionFactory.GetConnection(this, _connectionProperties, manager);
            Id = _connectionProperties.Id;
            _manager = manager;
        }

        private void OnDestroy()
        {
            Task.Run(() =>
            {
                _connection.DisconnectAsync();
            });
        }
        public void AddInputCallback(TZone interactionZone, Action<TZone, TState> callback)
        {
            _inputSubscriptions[interactionZone] = callback;
        }
        public IDevice Connect()
        {
            var task = ConnectAsync();
            task.Wait();
            return task.Result;
        }
        public async Task<IDevice> ConnectAsync()
        {
            await _connection.ConnectAsync();
            return (IDevice)this;
        }
        public void Disconnect()
        {
            DisconnectAsync().Wait();
        }
        public async Task DisconnectAsync()
        {
            await _connection.DisconnectAsync();
        }
        public bool CanConnect(IDevice device)
        {
            return _connection.CanConnect(device.Connection);
        }
        public abstract void ResetState();

        public abstract void OnConnected();
        public abstract Task OnConnectedAsync();
        public abstract void OnDisconnected();
        public abstract Task OnDisconnectedAsync();

        public void SetInputCallbacks(IDictionary<TZone, Action<TZone, TState>> inputSubscriptions)
        {
            // To prevent side effects due to threading reading will be halted temporarily to load new callbacks
            StopReading();
            _inputSubscriptions = inputSubscriptions;
            StartReading();
        }
        public Task SetInputCallbacksAsync(IDictionary<TZone, Action<TZone, TState>> inputSubscriptions)
        {
            SetInputCallbacks(inputSubscriptions);
            return Task.CompletedTask;
        }




        public void StopReading()
        {
            if (IsReading)
            {
                _connection.StopReading();
            }
        }

        public void StartReading()
        {
            if (!IsReading)
            {
                _connection.Read();
            }
        }
        public virtual void ReadData(ReadOnlyMemory<byte> data)
        {
            ReadData(data.Span);
        }
        // Making these methods virtual introduces overhead so
        // just implement them in all devices objects
        public abstract void ReadData(ReadOnlySpan<byte> data);
        public abstract void ReadData(IntPtr data);
        public abstract void Write<T>(params T[] interactions) where T : Enum;
        public abstract Task WriteAsync<T>(params T[] interactions) where T : Enum;
        // source: https://stackoverflow.com/a/48599119
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool ByteArraysEqual<T>(ReadOnlySpan<T> a1, ReadOnlySpan<T> a2) where T : IEquatable<T>
        {
            return a1.SequenceEqual(a2);
        }
        private static IDictionary<TZone, Action<TZone, TState>> CreateTypedDictionary(IDictionary<Enum, Action<Enum, Enum>> original)
        {
            var typedDictionary = new Dictionary<TZone, Action<TZone, TState>>();
            foreach (var kvp in original)
            {
                TZone key = (TZone)kvp.Key;
                Action<TZone, TState> value = (a1, a2) =>
                {
                    kvp.Value((TZone)(object)a1, (TState)(object)a2);
                };
                typedDictionary[key] = value;
            }

            return typedDictionary;
        }
    }
}