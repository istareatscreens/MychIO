namespace MychIO.Connection.TouchPanelDevice
{
    public class TouchPanelDeviceProperties : ConnectionProperties
    {
        public int PollingRateMs;

        // Constructor that initializes all properties
        public TouchPanelDeviceProperties(
            int pollingRateMs = 2,
            int? debounceTimeMs = 0
        ) : base("NOT IMPLEMENTED", debounceTimeMs ?? 0)
        {
            PollingRateMs = pollingRateMs;
            PopulatePropertiesFromFields();
        }

        // Copy Constructor used for creating properties objects from default properties
        public TouchPanelDeviceProperties(
            TouchPanelDeviceProperties existing,
            int? pollingRateMs = null,
            int? debounceTimeMs = 0
        ) : base("NOT IMPLEMENTED", debounceTimeMs ?? existing.DebounceTimeMs)
        {
            PollingRateMs = pollingRateMs ?? existing.PollingRateMs;
            PopulatePropertiesFromFields();
        }

        public override ConnectionType ConnectionType
        {
            get => ConnectionType.TouchPanelDevice;
        }
    }
}