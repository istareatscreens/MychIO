using System;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
     public abstract partial class Device<T1, T2> : IDevice<T1> where T1 : Enum where T2: IConnectionProperties
    {
        // Properties used by DeviceFactory to construct the concrete class. These must be overridden via the new keyword! 
        public static ConnectionType GetConnectionType() {
            throw new NotImplementedException("Error GetConnectionType method not overwritten in base class");
        }
        public static DeviceClassification GetDeviceClassification() {
            throw new NotImplementedException("Error GetDeviceClassification method not overwitten in base class");
        }
        public static string GetDeviceName(){
            throw new NotImplementedException("Error GetDeviceName method not overwitten in base class");
        }
        public static IConnectionProperties GetDefaultConnectionProperties(){
            throw new NotImplementedException("Error GetDefaultConnectionProperties method not overwitten in base class");
        }

    }
}