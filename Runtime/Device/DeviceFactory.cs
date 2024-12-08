using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MychIO.Device.TouchPanel;
using MychIO.Event;

namespace MychIO.Device
{
    public static class DeviceFactory
    {

        // Potentially replace this dictionary with Reflection
        private static Dictionary<string, Type> _deviceNameToType = new()
        {
            { AdxTouchPanel.GetDeviceName(), typeof(AdxTouchPanel) },
            { TouchPanelTouchPanel.GetDeviceName(), typeof(TouchPanelTouchPanel) },
            { AdxIO4ButtonRing.GetDeviceName(), typeof(AdxIO4ButtonRing) },
            { AdxHIDButtonRing.GetDeviceName(), typeof(AdxHIDButtonRing) },
            { AdxLedDevice.GetDeviceName(), typeof(AdxLedDevice) },
            { LTEKHIDDancePad.GetDeviceName(), typeof(LTEKHIDDancePad) },
            // Add other devices here...
        };

        public static async Task<IDevice> GetDeviceAsync(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions = null,
            IDevice[] ConnectedDevices = null,
            IOManager manager = null
        )
        {
            if (!_deviceNameToType.TryGetValue(deviceName, out var deviceType))
            {
                manager.handleEvent(IOEventType.ConnectionError, message: $"Could not find device {deviceName}");
            }

            var constructor = deviceType
                .GetConstructors()
                .First()
            ;

            if (constructor == null)
            {
                manager.handleEvent(IOEventType.ConnectionError, message: $"No suitable constructor found for device type {deviceType}");
            }
            var device = (IDevice)constructor.Invoke(new object[] { inputSubscriptions, connectionProperties, manager });

            // Check if the connection can even be created or if a duplicate device exists
            if (!ConnectedDevices.All(d => d.CanConnect(device)))
            {
                manager.handleEvent(IOEventType.ConnectionError, message: $"Duplicate connection for {device.GetType().Name} already exists cannot connect");
            }

            return await device.Connect();
        }

        public static DeviceClassification GetClassificationFromDeviceName(string deviceName)
        {
            if (!_deviceNameToType.TryGetValue(deviceName, out var deviceType))
            {
                throw new Exception("Could not find device");
            }
            var deviceClassificationMethod = deviceType
                .GetMethod(
                    "GetDeviceClassification",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public
                 );
            return (DeviceClassification)deviceClassificationMethod.Invoke(null, null);
        }


    }
}
