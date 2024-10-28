using MychIO.Device;

namespace MychIO.Event
{
    public enum IOEventType
    {
        Attach,
        Detach,
        ConnectionError,
        SerialDeviceReadError,
        HidDeviceReadError,
        Debug,

    }
    public delegate void ControllerEventDelegate(
        IOEventType eventType,
        DeviceClassification deviceType,
        string message
    );

}