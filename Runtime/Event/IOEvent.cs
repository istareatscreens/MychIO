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
        TouchPanelDeviceReadError,
        Debug,
        ReconnectionError,
        InvalidDevicePropertyError,
    }
    public delegate void ControllerEventDelegate(
        IOEventType eventType,
        DeviceClassification deviceType,
        string message
    );

}