using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;
using MychIO.Helper;

namespace MychIO.Connection.TouchPanelDevice
{
    public class TouchPanelDeviceConnection : Connection
    {
        // Is Connected means its reading currently
        public override bool IsConnected
        {
            get => UnityTouchPanelApiPlugin.IsConnected(_pluginHandle);
        }
        public override bool IsReading
        {
            get => UnityTouchPanelApiPlugin.IsReading(_pluginHandle);
        }
        // Hold callbacks to prevent garbage collection
        private GCHandle _dataCallbackHandle;
        private GCHandle _eventCallbackHandle;

        // Holds C++ plugin object reference
        private IntPtr _pluginHandle;

        public TouchPanelDeviceConnection(List<IDevice> devices, IConnectionProperties connectionProperties, IOManager manager) :
         base(devices, connectionProperties, manager)
        {

            ValidateConnectionProperties<TouchPanelDeviceProperties>();

            if (1 != UnityTouchPanelApiPlugin.PluginLoaded())
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                        _classification,
                        "Error loading UnityTouchPanelApiPlugin plugin"
                );
            }

            // UnityTouchPanelApiPlugin.DisposeByClassification((int)device.GetClassification());
            TouchPanelDeviceProperties properties = (TouchPanelDeviceProperties)connectionProperties;
            _pluginHandle = UnityTouchPanelApiPlugin.Initialize(
                (int)_classification,
                properties.PollingRateMs,
                (string message) => { manager.handleEvent(IOEventType.ConnectionError, _classification, message); }
            );
            if (_pluginHandle == IntPtr.Zero)
            {
                manager.handleEvent(
                    IOEventType.ConnectionError,
                    _classification,
                    "Error Initializing Touch Panel Connection plugin, please recreate this device"
                );
                // This will destroy the initialized settings, TouchPanelDeviceConnection failed to initialize
                UnityTouchPanelApiPlugin.ReloadPlugin();
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
                UnityTouchPanelApiPlugin.Dispose(_pluginHandle);
            }
            try
            {
                OnDeviceDisconnected();
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
        public override void Connect()
        {
            ConnectAsync().Wait();
        }
        public override Task ConnectAsync()
        {

            // IsConnected() will always return true here since successful initilization
            // counts as connection so do not check

            var eventReceivedCallback = new UnityTouchPanelApiPlugin.EventCallbackDelegate(
                (string message) =>
                {
                    _manager.handleEvent(IOEventType.TouchPanelDeviceReadError, _classification, _types + " Error: " + message);
                }
            );

            if (!UnityTouchPanelApiPlugin.Connect(_pluginHandle, eventReceivedCallback))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _classification, _types + " Failed to Connect");
            }

            var dataReceivedCallback = GetRecieveDataFunction();

            // prevent garbage collection of callbacks
            _dataCallbackHandle = GCHandle.Alloc(dataReceivedCallback);
            _eventCallbackHandle = GCHandle.Alloc(eventReceivedCallback);
            Read();

            _manager.handleEvent(IOEventType.Attach, _classification, _types + " Device is running properly");

            return Task.CompletedTask;

        }

        private UnityTouchPanelApiPlugin.DataCallbackDelegate GetRecieveDataFunction()
        {

            var dt = _connectionProperties.GetDebounceThreshold();
            if (1 == _devices.Count)
            {
                var device = _devices.First();
                if (dt.TotalMilliseconds > 0)
                {
                    return new UnityTouchPanelApiPlugin.DataCallbackDelegate(device.ReadDataWithDebounce);
                }
                else
                {
                    return new UnityTouchPanelApiPlugin.DataCallbackDelegate(device.ReadData);
                }
            }

            if (dt.TotalMilliseconds > 0)
            {
                return new UnityTouchPanelApiPlugin.DataCallbackDelegate((IntPtr data) =>
                {
                    foreach (var device in _devices)
                    {
                        device.ReadDataWithDebounce(data);
                    }
                });
            }
            else
            {
                return new UnityTouchPanelApiPlugin.DataCallbackDelegate((IntPtr data) =>
                {
                    foreach (var device in _devices)
                    {
                        device.ReadData(data);
                    }
                });
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
                await OnDeviceDisconnectedAsync();
            }
            UnityTouchPanelApiPlugin.Disconnect(_pluginHandle);
        }
        public override void Read()
        {
            if (!IsReading && _pluginHandle != null && _pluginHandle != IntPtr.Zero)
            {
                var dataCallback = (UnityTouchPanelApiPlugin.DataCallbackDelegate)_dataCallbackHandle.Target;
                var eventCallback = (UnityTouchPanelApiPlugin.EventCallbackDelegate)_eventCallbackHandle.Target;
                UnityTouchPanelApiPlugin.Read(_pluginHandle, dataCallback, eventCallback);
            }

            if (!UnityTouchPanelApiPlugin.IsReading(_pluginHandle))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _classification, _types + " Error: failed to start reading from device");
            }
        }
        public override void StopReading()
        {
            if (IsReading)
            {
                UnityTouchPanelApiPlugin.StopReading(_pluginHandle);
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