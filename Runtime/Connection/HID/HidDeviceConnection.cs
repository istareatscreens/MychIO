using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;
using MychIO.Helper;

namespace MychIO.Connection.HidDevice
{
    public class HidDeviceConnection : Connection
    {
        public override bool IsReading
        {
            get => UnityHidApiPlugin.IsReading(_pluginHandle);
        }
        /// <summary>
        /// Is Connected means its reading currently
        /// </summary>
        public override bool IsConnected
        {
            get => UnityHidApiPlugin.IsConnected(_pluginHandle);
        }
        // Hold callbacks to prevent garbage collection
        private GCHandle _dataCallbackHandle;
        private GCHandle _eventCallbackHandle;

        // Holds C++ plugin object reference
        private IntPtr _pluginHandle;

        public HidDeviceConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        {

            ValidateConnectionProperties<HidDeviceProperties>();

            if (1 != UnityHidApiPlugin.PluginLoaded())
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                        _device.Classification,
                        "Error loading UnityHidApiPlugin plugin"
                );
            }

            // UnityHidApiPlugin.DisposeByClassification((int)device.GetClassification());
            HidDeviceProperties properties = (HidDeviceProperties)connectionProperties;
            _pluginHandle = UnityHidApiPlugin.Initialize(
                (int)device.Classification,
                properties.VendorId,
                properties.ProductId,
                properties.BufferSize,
                properties.LeftBytesToTruncate,
                properties.BytesToRead,
                properties.PollingRateMs
            );
            if (_pluginHandle == IntPtr.Zero)
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                    _device.Classification,
                    "Error Initializing HID Connection plugin, please recreate this device"
                );

                // This will destroy the initialized settings, HidDeviceConnection failed to initialize
                UnityHidApiPlugin.ReloadPlugin();
                throw new Exception();
            }
        }

        public override void Dispose()
        {

            if (_dataCallbackHandle.IsAllocated)
            {
                _dataCallbackHandle.Free();
            }
            if (_eventCallbackHandle.IsAllocated)
            {
                _eventCallbackHandle.Free();
            }
            if (_pluginHandle != IntPtr.Zero)
            {
                UnityHidApiPlugin.Dispose(_pluginHandle);
            }
            try
            {
                _device?.OnDisconnected();
            }
            catch { }
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            // It is possible to have multiple devices with the same vendorId and ProductId
            // if in the future multiplayer on the same machine is supported 
            // device path should be used instead requiring a rework of how the plugin is implemented 
            // e.g. add support for device path as a connectionProperty then overload the plugin constructor 
            return connectionProperties is not HidDeviceProperties ||
            (
             ((HidDeviceProperties)connectionProperties).VendorId !=
              ((HidDeviceProperties)_connectionProperties).VendorId &&
             ((HidDeviceProperties)connectionProperties).ProductId !=
              ((HidDeviceProperties)_connectionProperties).ProductId
            );
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

            var eventReceivedCallback = new UnityHidApiPlugin.EventCallbackDelegate(
                (string message) =>
                {
                    _manager.handleEvent(IOEventType.HidDeviceReadError, _device.Classification, _device.GetType().ToString() + " Error: " + message);
                }
            );

            if (!UnityHidApiPlugin.Connect(_pluginHandle, eventReceivedCallback))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.Classification, _device.GetType().ToString() + " Failed to Connect");
            }

            var dataReceivedCallback = GetRecieveDataFunction();

            // prevent garbage collection of callbacks
            _dataCallbackHandle = GCHandle.Alloc(dataReceivedCallback);
            _eventCallbackHandle = GCHandle.Alloc(eventReceivedCallback);
            Read();

            if(IsReading)
            {
                _manager.handleEvent(IOEventType.Attach, _device.Classification, _device.GetType().ToString() + " Device is running properly");
            }

            return Task.CompletedTask;

        }

        private UnityHidApiPlugin.DataCallbackDelegate GetRecieveDataFunction()
        {
            var dt = _connectionProperties.GetDebounceThreshold();
            if(dt.TotalMilliseconds > 0)
            {
                return new UnityHidApiPlugin.DataCallbackDelegate(_device.ReadDataWithDebounce);
            }
            else
            {
                return new UnityHidApiPlugin.DataCallbackDelegate(_device.ReadData);
            }
        }
        public override void Disconnect()
        {
            DisconnectAsync().Wait();
        }
        public override async Task DisconnectAsync()
        {
            if (IsConnected)
            {
                await _device.OnDisconnectedAsync();
            }
            UnityHidApiPlugin.Disconnect(_pluginHandle);
        }
        public override void Read()
        {
            if (!IsReading && _pluginHandle != null && _pluginHandle != IntPtr.Zero)
            {
                var dataCallback = (UnityHidApiPlugin.DataCallbackDelegate)_dataCallbackHandle.Target;
                var eventCallback = (UnityHidApiPlugin.EventCallbackDelegate)_eventCallbackHandle.Target;
                UnityHidApiPlugin.Read(_pluginHandle, dataCallback, eventCallback);
            }

            if (!UnityHidApiPlugin.IsReading(_pluginHandle))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.Classification, _device.GetType().ToString() + " Error: failed to start reading from device");
            }
        }
        public override void StopReading()
        {
            if (IsReading)
            {
                UnityHidApiPlugin.StopReading(_pluginHandle);
            }
        }
        public override void Write(ReadOnlySpan<byte> data)
        {
            ThrowHelper.NotSupported();
        }
        public override Task WriteAsync(ReadOnlyMemory<byte> data)
        {
            return ThrowHelper.NotSupported<Task>();
        }
        // currently no need to write to HID devices so not implemented
        public override Task WriteAsync(byte[] bytes)
        {
            return ThrowHelper.NotSupported<Task>();
        }
    }
}