using System;
using System.Reflection;
using MychIO.Connection;

namespace MychIO.Device
{
    public abstract partial class Device<TZone, TState, TConnProps> : IDevice<TZone, TState> where TZone : Enum where TConnProps : IConnectionProperties where TState : Enum
    {
        // Properties used by DeviceFactory to construct the concrete class. These must be overridden via the new keyword! 
        public static ConnectionType GetConnectionType()
        {
            throw new NotImplementedException("Error GetConnectionType method not overwritten in base class");
        }
        public static DeviceClassification GetDeviceClassification()
        {
            throw new NotImplementedException("Error GetDeviceClassification method not overwitten in base class");
        }
        public static string GetDeviceName()
        {
            throw new NotImplementedException("Error GetDeviceName method not overwitten in base class");
        }
        public static IConnectionProperties GetDefaultConnectionProperties()
        {
            throw new NotImplementedException("Error GetDefaultConnectionProperties method not overwitten in base class");
        }

        public static TConnProps GetDefaultDeviceProperties()
        {
            throw new NotImplementedException("Error GetDefaulDeviceProperties method not overwitten in base class");
        }

        public static Type GetDevicePropertiesType()
        {
            return typeof(TConnProps);
        }
        // Helper method to access these static methods
        private static MethodInfo GetBaseClassStaticMethod(string method, Type type)
        {
            return type.GetMethod(
                method,
                BindingFlags.Static | BindingFlags.Public
            );
        }

    }
}
