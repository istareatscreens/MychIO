using System;

namespace MychIO.Generic
{
    /* 
     TODO: Use this property instead of device classification to support duplicate devices (multiplayer support)
        - Updating IOManager Dictionaries to map by classification and deviceId.
        - Updating IOEvents to include deviceID
        - Updating Device.CanConnect method to support multiple devices in check (prevent conflicting properties with other devices)
        - Update IOManager methods to use device Id when interacting with an individual device
    */
    public interface IIdentifier
    {
        public string Id
        {
            get => throw new NotImplementedException($"Identification getter not implemented for device {GetType().FullName}");
        }

    }
}