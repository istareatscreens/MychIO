using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;

namespace MychIO.Connection.TouchPanelDevice
{
    public class TouchPanelDeviceConnection : Connection
    {

        // Hold callbacks to prevent garbage collection
        private GCHandle _dataCallbackHandle;
        private GCHandle _eventCallbackHandle;

        // Holds C++ plugin object reference
        private IntPtr _pluginHandle;

        public TouchPanelDeviceConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        {

            ValidateConnectionProperties<TouchPanelDeviceProperties>();

            if (1 != UnityTouchPanelApiPlugin.PluginLoaded())
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                        _device.GetClassification(),
                        "Error loading UnityTouchPanelApiPlugin plugin"
                );
            }

            // UnityTouchPanelApiPlugin.DisposeByClassification((int)device.GetClassification());
            TouchPanelDeviceProperties properties = (TouchPanelDeviceProperties)connectionProperties;
            _pluginHandle = UnityTouchPanelApiPlugin.Initialize(
                (int)device.GetClassification(),
                properties.PollingRateMs,
                (string message) => { manager.handleEvent(IOEventType.ConnectionError, device.GetClassification(), message); }
            );
            if (_pluginHandle == IntPtr.Zero)
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                    _device.GetClassification(),
                    "Error Initializing Touch Panel Connection plugin, please recreate this device"
                );
                // This will destroy the initialized settings, TouchPanelDeviceConnection failed to initialize
                UnityTouchPanelApiPlugin.ReloadPlugin();
                throw new Exception();
            }
        }

        private void OnDestroy()
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
                UnityTouchPanelApiPlugin.Dispose(_pluginHandle);
            }
            try
            {
                _device?.OnDisconnectWrite();
            }
            catch { }
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            // Can only have on touch device connected at a time,
            // if you wanted to have multiple connection events in a single program
            // you would likely need to use different window handles
            return connectionProperties is not TouchPanelDeviceProperties;
        }

        public override Task Connect()
        {

            // IsConnected() will always return true here since successful initilization
            // counts as connection so do not check
            
            var eventReceivedCallback = new UnityTouchPanelApiPlugin.EventCallbackDelegate(
                (string message) =>
                {
                    _manager.handleEvent(IOEventType.TouchPanelDeviceReadError, _device.GetClassification(), _device.GetType().ToString() + " Error: " + message);
                }
            );

            if (!UnityTouchPanelApiPlugin.Connect(_pluginHandle, eventReceivedCallback))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Failed to Connect");
            }

            var dataReceivedCallback = GetRecieveDataFunction();

            // prevent garbage collection of callbacks
            _dataCallbackHandle = GCHandle.Alloc(dataReceivedCallback);
            _eventCallbackHandle = GCHandle.Alloc(eventReceivedCallback);
            Read();

            _manager.handleEvent(IOEventType.Attach, _device.GetClassification(), _device.GetType().ToString() + " Device is running properly");

            return Task.CompletedTask;

        }

        private UnityTouchPanelApiPlugin.DataCallbackDelegate GetRecieveDataFunction()
        {
            return _connectionProperties.GetDebounceTime() > TimeSpan.FromMilliseconds(0) ?
                         new UnityTouchPanelApiPlugin.DataCallbackDelegate(_device.ReadDataDebounce) :
                         new UnityTouchPanelApiPlugin.DataCallbackDelegate(_device.ReadData);
        }

        public override async Task Disconnect()
        {
            if (IsConnected())
            {
                await _device.OnDisconnectWrite();
            }
            UnityTouchPanelApiPlugin.Disconnect(_pluginHandle);
        }

        // Is Connected means its reading currently
        public override bool IsConnected()
        {
            return UnityTouchPanelApiPlugin.IsConnected(_pluginHandle);
        }

        // currently no need to write to HID devices so not implemented
        public override Task Write(byte[] bytes)
        {
            return Task.CompletedTask;
        }

        public override bool IsReading()
        {
            return UnityTouchPanelApiPlugin.IsReading(_pluginHandle);
        }

        public override void Read()
        {
            if (!IsReading() && _pluginHandle != null && _pluginHandle != IntPtr.Zero)
            {
                var dataCallback = (UnityTouchPanelApiPlugin.DataCallbackDelegate)_dataCallbackHandle.Target;
                var eventCallback = (UnityTouchPanelApiPlugin.EventCallbackDelegate)_eventCallbackHandle.Target;
                UnityTouchPanelApiPlugin.Read(_pluginHandle, dataCallback, eventCallback);
            }

            if (!UnityTouchPanelApiPlugin.IsReading(_pluginHandle))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Error: failed to start reading from device");
            }
        }

        public override void StopReading()
        {
            if (IsReading())
            {
                UnityTouchPanelApiPlugin.StopReading(_pluginHandle);
            }
        }
    }
}