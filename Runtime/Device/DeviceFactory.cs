using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
public static class DeviceFactory
{

    private static Dictionary<string, Type> _deviceNameToType = new()
    {
        { AdxTouchPanel.GetDeviceName(), typeof(AdxTouchPanel) }
        // Add other devices here...
    };

    public static async Task<IDevice> GetDeviceAsync(string deviceName,
        IDictionary<string, dynamic> connectionProperties = null,
        IDictionary<TouchPanelZone, Action<TouchPanelZone, Enum>> inputSubscriptions = null)
    {
        if (!_deviceNameToType.TryGetValue(deviceName, out var deviceType))
        {
            throw new Exception("Could not find device");
        }

        // Get the constructor that matches the required parameters
        var constructor = deviceType.GetConstructors()
            .FirstOrDefault(c => c.GetParameters().Length == 2);

        if (constructor == null)
        {
            throw new Exception($"No suitable constructor found for device type {deviceType}");
        }

        // Create an instance of the device using reflection
        var deviceInstance = (IDevice)constructor.Invoke(new object[] { inputSubscriptions, connectionProperties });
        
        // Call the Connect method
        await deviceInstance.Connect();

        return deviceInstance;
    }
}

}