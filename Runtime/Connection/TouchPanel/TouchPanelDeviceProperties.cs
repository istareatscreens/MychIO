namespace MychIO.Connection.TouchPanelDevice
{
    public class TouchPanelDeviceProperties : ConnectionProperties
    {
        public int PollingRateMs;

        // Constructor that initializes all properties
        public TouchPanelDeviceProperties(
            int pollingRateMs = 2,
            int? debounceTimeMs = 0
        ) : base(debounceTimeMs ?? 0)
        {
            PollingRateMs = pollingRateMs;
            PopulatePropertiesFromFields();
        }

        // Copy Constructor used for creating properties objects from default properties
        public TouchPanelDeviceProperties(
            TouchPanelDeviceProperties existing,
            int? pollingRateMs = null,
            // Device Class specific properties
            int? debounceTimeMs = 0
        ) : base(debounceTimeMs ?? existing.DebounceTimeMs)
        {
            PollingRateMs = pollingRateMs ?? existing.PollingRateMs;
            PopulatePropertiesFromFields();
        }

        public override ConnectionType GetConnectionType() => ConnectionType.TouchPanelDevice;
    }
}