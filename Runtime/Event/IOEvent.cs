using MychIO.Device;

namespace MychIO.Event
{
    public enum IOEventType
    {
        Attach,
        Detach,
        ConnectionError,
        SerialDeviceReadError,
#if UNITY_EDITOR
        Debug,
#endif
    }
    public delegate void ControllerEventDelegate(
        IOEventType eventType,
        DeviceClassification deviceType,
        string message
    );

}