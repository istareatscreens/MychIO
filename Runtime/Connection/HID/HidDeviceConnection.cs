using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Device;
using MychIO.Event;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MychIO.Connection.HidDevice
{
    public unsafe class HidDeviceConnection : Connection
    {

        // Hold callbacks to prevent garbage collection
        private GCHandle _dataCallbackHandle;
        private GCHandle _eventCallbackHandle;

        // Holds C++ plugin object reference
        private IntPtr _pluginHandle;

        public HidDeviceConnection(IDevice device, IConnectionProperties connectionProperties, IOManager manager) :
         base(device, connectionProperties, manager)
        {
            if (connectionProperties is not HidDeviceProperties)
            {
                throw new Exception("Invalid connection object passed to SerialDevice class");
            }
            HidDeviceProperties properties = (HidDeviceProperties)connectionProperties;
            _pluginHandle = UnityHidApiPlugin.Initialize(
                properties.VendorId,
                properties.ProductId,
                properties.BufferSize
            );
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
                UnityHidApiPlugin.Dispose(_pluginHandle);
            }
        }

        public override bool CanConnect(IConnection connectionProperties)
        {
            // It is possible to have multiple devices with the same vendorId and ProductId
            // if in the future multiplayer on the same machine is supported 
            // device path should be used instead requiring a rework of how the plugin is implemented 
            // e.g. add support for device path as a connectionProperty then overload the plugin constructor 
            return !(connectionProperties is HidDeviceProperties) ||
            (
             ((HidDeviceProperties)connectionProperties).VendorId !=
              ((HidDeviceProperties)_connectionProperties).VendorId &&
             ((HidDeviceProperties)connectionProperties).ProductId !=
              ((HidDeviceProperties)_connectionProperties).ProductId
            );
        }

        public override Task Connect()
        {
            if (UnityHidApiPlugin.Connect(_pluginHandle))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Failed to Connect");
            }

            var dataRecievedCallback = new UnityHidApiPlugin.DataCallbackDelegate(_device.ReadData);
            var eventRecievedCallback = new UnityHidApiPlugin.EventCallbackDelegate(
                (string message) =>
                {
                    _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Error: " + message);
                }
            );

            // prevent garbage collection of callbacks
            _dataCallbackHandle = GCHandle.Alloc(dataRecievedCallback);
            _eventCallbackHandle = GCHandle.Alloc(eventRecievedCallback);
            UnityHidApiPlugin.Read(_pluginHandle, dataRecievedCallback, eventRecievedCallback);

            if (!UnityHidApiPlugin.IsReading(_pluginHandle))
            {
                _manager.handleEvent(IOEventType.ConnectionError, _device.GetClassification(), _device.GetType().ToString() + " Error: failed to start reading from device");
            }

            _manager.handleEvent(IOEventType.Attach, _device.GetClassification(), _device.GetType().ToString() + " Device is running properly");

            return Task.CompletedTask;

        }

        public override void Disconnect()
        {
            UnityHidApiPlugin.Disconnect(_pluginHandle);
        }

        // Is Connected means its reading currently
        public override bool IsConnected()
        {
            return UnityHidApiPlugin.IsConnected(_pluginHandle);
        }

        // currently no need to write to HID devices so not implemented
        public override Task Write(byte[] bytes)
        {
            throw new NotImplementedException("Writing to a HID device is not currently implemented");
        }

    }
}