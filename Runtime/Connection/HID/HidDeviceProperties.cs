namespace MychIO.Connection.HidDevice
{
    public class HidDeviceProperties : ConnectionProperties
    {
        // WARNING: Adding {get; set;} will break serialization/unserialization of properties!
        public int ProductId;
        public int VendorId;
        public int BufferSize;
        public int LeftBytesToTruncate;
        public int BytesToRead;
        public int PollingRateMs;

        // Constructor that initializes all properties
        public HidDeviceProperties(
            int productId = 0x0021,
            int vendorId = 0x0CA3,
            int bufferSize = 64,
            int leftBytesToTruncate = 0,
            int bytesToRead = 64,
            int pollingRateMs = 0,
            // Device Class specific properties
            int? debounceTimeMs = 0
        ) : base(debounceTimeMs ?? 0)
        {
            ProductId = productId;
            VendorId = vendorId;
            BufferSize = bufferSize;
            LeftBytesToTruncate = leftBytesToTruncate;
            BytesToRead = bytesToRead;
            PollingRateMs = pollingRateMs;
            PopulatePropertiesFromFields();
        }

        // Copy Constructor used for creating properties objects from default properties
        public HidDeviceProperties(
            HidDeviceProperties existing,
            int? productId = null,
            int? vendorId = null,
            int? bufferSize = null,
            int? leftBytesToTruncate = null,
            int? bytesToRead = null,
            int? pollingRateMs = null,
            // Device Class specific properties
            int? debounceTimeMs = 0
        ) : base(debounceTimeMs ?? existing.DebounceTimeMs)
        {
            ProductId = productId ?? existing.ProductId;
            VendorId = vendorId ?? existing.VendorId;
            BufferSize = bufferSize ?? existing.BufferSize;
            LeftBytesToTruncate = leftBytesToTruncate ?? existing.LeftBytesToTruncate;
            BytesToRead = bytesToRead ?? existing.BytesToRead;
            PollingRateMs = pollingRateMs ?? existing.PollingRateMs;
            PopulatePropertiesFromFields();
        }


        public override ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
    }
}