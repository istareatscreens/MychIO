using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
    // Important class cannot have more than 1 constructor (see Device Factory)
    public abstract partial class Device<T1, T2, T3> : IDevice<T1, T2> 
        where T1 : Enum 
        where T3 : IConnectionProperties 
        where T2 : Enum
    {
        // Debounce Properties
        protected readonly Dictionary<T1, long> _lastInputTriggerTimes;
        protected TimeSpan _debounceTime;
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

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
        protected IDictionary<T1, Action<T1, T2>> _inputSubscriptions;
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

            if (0 == defaultProperties.GetProperties().Count)
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
            foreach (var error in _connectionProperties.GetErrors())
            {
                manager.handleEvent(Event.IOEventType.InvalidDevicePropertyError, _classification, error);
            }

            // Setup Debounce
            _debounceTime = _connectionProperties.GetDebounceTime();
            _lastInputTriggerTimes = new Dictionary<T1, long>();
            foreach (T1 zone in Enum.GetValues(typeof(T1)))
            {
                _lastInputTriggerTimes[zone] = 0; // Initialize with 0 milliseconds
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
                _connection.Disconnect();
            });
        }

        public IConnectionProperties GetConnectionProperties() => _connectionProperties;

        public void SetInputCallbacks(IDictionary<T1, Action<T1, T2>> inputSubscriptions)
        {
            _inputSubscriptions = inputSubscriptions;
        }

        public void AddInputCallback(T1 interactionZone, Action<T1, T2> callback)
        {
            _inputSubscriptions[interactionZone] = callback;
        }

        public async Task<IDevice> Connect()
        {
            await _connection.Connect();
            return (IDevice)this;
        }
        public async Task Disconnect()
        {
            await _connection.Disconnect();
        }

        public bool IsConnected()
        {
            throw new NotImplementedException("Should be implemented by base class");
        }

        public IConnection GetConnection()
        {
            return _connection;
        }

        public DeviceClassification GetClassification()
        {
            return _classification;
        }

        public bool CanConnect(IDevice device)
        {
            return _connection.CanConnect(device.GetConnection());
        }
        public abstract void ResetState();

        public abstract Task OnStartWrite();

        public abstract Task OnDisconnectWrite();

        Task IDevice<T1, T2>.SetInputCallbacks(IDictionary<T1, Action<T1, T2>> inputSubscriptions)
        {
            // To prevent side effects due to threading reading will be halted temporarily to load new callbacks
            StopReading();
            _inputSubscriptions = inputSubscriptions;
            StartReading();
            return Task.CompletedTask;
        }

        public bool IsReading()
        {
            return _connection.IsReading();
        }

        public void StopReading()
        {
            if (IsReading())
            {
                _connection.StopReading();
            }
        }

        public void StartReading()
        {
            if (!IsReading())
            {
                _connection.Read();
            }
        }

        // Making these methods virtual introduces overhead so
        // just implement them in all devices objects
        public abstract void ReadData(byte[] data);
        public abstract void ReadData(IntPtr data);
        public abstract void ReadDataDebounce(byte[] data);
        public abstract void ReadDataDebounce(IntPtr intPtr);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DebouncedHandleInputChange<T4>(T1 zone, Func<T4, bool> callback, T4 input)
        {
            long now = _stopwatch.ElapsedMilliseconds;

            var diff = now - _lastInputTriggerTimes[zone];
            if (diff < _debounceTime.TotalMilliseconds)
            {
#if DEBUG
                _manager.handleEvent(Event.IOEventType.Debug, 
                                     _classification, 
                                     $"[Debounce] Received device response\nInterval: {diff}ms");
#endif
                return;
            }

            // handle input and check if there has been a change
            if (callback(input))
            {
                _lastInputTriggerTimes[zone] = now;
#if DEBUG
                _manager.handleEvent(Event.IOEventType.Debug,
                                     _classification,
                                     $"[Update] Received device response");
#endif
            }
        }

        public abstract Task Write<T>(params T[] interactions) where T:Enum;

        private static IDictionary<T1, Action<T1, T2>> CreateTypedDictionary(IDictionary<Enum, Action<Enum, Enum>> original)
        {
            var typedDictionary = new Dictionary<T1, Action<T1, T2>>();
            foreach (var kvp in original)
            {
                T1 key = (T1)kvp.Key;
                Action<T1, T2> value = (a1, a2) =>
                {
                    kvp.Value((T1)(object)a1, (T2)(object)a2);
                };
                typedDictionary[key] = value;
            }

            return typedDictionary;
        }
    }
}