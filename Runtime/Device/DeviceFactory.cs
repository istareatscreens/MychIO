using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MychIO.Device
{
    public static class DeviceFactory
    {

        // Potentially replace this dictionary with Reflection
        private static Dictionary<string, Type> _deviceNameToType = new()
        {
            { AdxTouchPanel.GetDeviceName(), typeof(AdxTouchPanel) }
            // Add other devices here...
        };

        public static async Task<IDevice> GetDeviceAsync(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<TouchPanelZone, Action<TouchPanelZone, Enum>> inputSubscriptions = null,
            IDevice[] ConnectedDevices = null
        )
        {
            if (!_deviceNameToType.TryGetValue(deviceName, out var deviceType))
            {
                throw new Exception("Could not find device");
            }

            var constructor = deviceType
                .GetConstructors()
                .First()
            ;


            if (constructor == null)
            {
                throw new Exception($"No suitable constructor found for device type {deviceType}");
            }
            var device = (IDevice)constructor.Invoke(new object[] { inputSubscriptions, connectionProperties });

            // Check if the connection can even be created or if a duplicate device exists
            if (!ConnectedDevices.All(d => d.CanConnect(device)))
            {
                throw new Exception("Duplicate connection already exists cannot connect");
            }

            return await device.Connect();
        }
    }

}