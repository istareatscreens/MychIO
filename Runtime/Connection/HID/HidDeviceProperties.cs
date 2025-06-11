using MychIO.Helper;

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
            int? debounceTimeMs = 0
        ) : base(
            GenerateUniqueIdentifier(
                productId,
                vendorId,
                bufferSize,
                leftBytesToTruncate,
                bytesToRead,
                pollingRateMs,
                debounceTimeMs ?? 0
            ),
            debounceTimeMs ?? 0
        )
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
            int? debounceTimeMs = 0
        ) : base(
            GenerateUniqueIdentifier(
                productId ?? existing.ProductId,
                vendorId ?? existing.VendorId,
                bufferSize ?? existing.BufferSize,
                leftBytesToTruncate ?? existing.LeftBytesToTruncate,
                bytesToRead ?? existing.BytesToRead,
                pollingRateMs ?? existing.PollingRateMs,
                debounceTimeMs ?? 0
            ),
                debounceTimeMs ?? 0
            )
        {
            ProductId = productId ?? existing.ProductId;
            VendorId = vendorId ?? existing.VendorId;
            BufferSize = bufferSize ?? existing.BufferSize;
            LeftBytesToTruncate = leftBytesToTruncate ?? existing.LeftBytesToTruncate;
            BytesToRead = bytesToRead ?? existing.BytesToRead;
            PollingRateMs = pollingRateMs ?? existing.PollingRateMs;
            PopulatePropertiesFromFields();
        }

        private static string GenerateUniqueIdentifier(
            int productId,
            int vendorId,
            int bufferSize,
            int leftBytesToTruncate,
            int bytesToRead,
            int pollingRateMs,
            int debounceTimeMs
        )
        {
            return HelperFunctions.GenerateUniqueHashFromStrings(new string[] {
                productId.ToString(),
                vendorId.ToString(),
                bufferSize.ToString(),
                leftBytesToTruncate.ToString(),
                bytesToRead.ToString(),
                pollingRateMs.ToString(),
                debounceTimeMs.ToString()
            });
        }

        public override ConnectionType ConnectionType
        {
            get => ConnectionType.SerialDevice;
        }
    }
}