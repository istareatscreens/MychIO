namespace MychIO.Device
{
    public enum DeviceClassification
    {
        TouchPanel,
        ButtonRing,
        LedDevice,
        MultipleDevice,
        // Used exclusively for events not specific to a device
        Undefined,
    }
}