using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MychIO.Connection.HidDevice;
using MychIO.Connection.SerialDevice;
using MychIO.Device;

namespace MychIO.Connection
{
    public class ConnectionFactory
    {
        // Potentially replace this dictionary with Reflection
        private static Dictionary<ConnectionType, Type> _connectionTypeToConnection = new()
        {
            { ConnectionType.HID, typeof(HidDeviceConnection) },
            { ConnectionType.SerialDevice, typeof(SerialDeviceConnection) },
            // Add other connections here...
        };

        private static readonly ConcurrentDictionary<string, Func<List<IDevice>, IConnection>> connectionCreatorCallbacks = new();
        private static readonly ConcurrentDictionary<string, IConnection> createdConnections = new();
        private static readonly ConcurrentDictionary<string, List<IDevice>> deviceToConnectionMap = new();
        // Map of ConnectionProperties Unique String -> created connections
        // TODO: Add cleanup of this dictionary when no device holds the connection
        private static Dictionary<string, IConnection> spawnedConnections = new();


        public static void PrepareConnection(IDevice device, IConnectionProperties props, IOManager manager)
        {
            string id = props.UniqueConnectionIdentifier;

            // If connection creation callback doesnt exist:
            if (!deviceToConnectionMap.TryGetValue(id, out var deviceList))
            {
                deviceList = new List<IDevice>();
                deviceToConnectionMap[id] = deviceList;

                var connectionType = GetConnectionTypeFromDevice(device);

                if (!_connectionTypeToConnection.TryGetValue(connectionType, out var connectionClassType))
                    throw new Exception($"No connection type registered for {connectionType}");

                connectionCreatorCallbacks[id] = (List<IDevice> devices) =>
                {
                    var constructor = connectionClassType.GetConstructors().First();
                    return (IConnection)constructor.Invoke(new object[] { devices, props, manager });
                };
            }

            // else device creation factory exists we can add the device directly to the list
            if (!deviceList.Contains(device))
            {
                deviceList.Add(device);
            }
        }

        internal static IConnection GetConnection(IDevice device)
        {
            string id = device.ConnectionProperties.UniqueConnectionIdentifier;

            // connection was already created using factory callback
            if (createdConnections.TryGetValue(id, out var existingConnection))
            {
                return existingConnection;
            }

            if (!connectionCreatorCallbacks.TryGetValue(id, out var createConnection))
            {
                throw new Exception($"No connection prepared for device with ID: {id}");
            }

            // create connection
            var connection = createConnection(deviceToConnectionMap[id]);
            createdConnections[id] = connection;
            return connection;
        }

        private static ConnectionType GetConnectionTypeFromDevice(IDevice device)
        {
            var method = device.GetType().GetMethod(
                "GetConnectionType",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
            );

            return (ConnectionType)method.Invoke(null, null);
        }

        // Probably should use these somewhere:
        internal static void ReleaseConnection(string id)
        {
            createdConnections.TryRemove(id, out _);
            connectionCreatorCallbacks.TryRemove(id, out _);
            deviceToConnectionMap.TryRemove(id, out _);
        }

        internal static void ClearAllConnections()
        {
            createdConnections.Clear();
            connectionCreatorCallbacks.Clear();
            deviceToConnectionMap.Clear();
        }

        private static void CleanupConnectionIfUnused(string connectionId)
        {
            if (deviceToConnectionMap.TryGetValue(connectionId, out var devices) && devices.Count == 0)
            {
                ReleaseConnection(connectionId);
            }
        }

    }
}