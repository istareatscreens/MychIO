namespace MychIO.Connection.SerialDevice
{
    public class SerialDeviceProperties : ConnectionProperties
    {
        public const int DEFAULT_WRITE_TIMEOUT_MS = 1000;
        // WARNING: Adding {get; set;} will break serialization/unserialization of properties!
        public string ComPortNumber;
        public int PollTimeoutMs;
        public int BufferByteLength;
        public int WriteTimeoutMS;
        public int PortNumber;
        public BaudRate BaudRate;
        public StopBits StopBit;
        public Parity ParityBit;
        public DataBits DataBits;
        public Handshake Handshake;
        public bool Dtr;
        public bool Rts;

        // Constructor that initializes all properties
        public SerialDeviceProperties(
            string comPortNumber = "COM21",
            int pollingRateMs = 0,
            int bufferByteLength = 9,
            int writeTimeoutMS = DEFAULT_WRITE_TIMEOUT_MS,
            int portNumber = 0,
            BaudRate baudRate = BaudRate.Bd9600,
            StopBits stopBit = StopBits.One,
            Parity parityBit = Parity.None,
            DataBits dataBits = DataBits.Eight,
            Handshake handshake = Handshake.None,
            bool dtr = false,
            bool rts = false
        )
        {
            ComPortNumber = comPortNumber;
            PollTimeoutMs = pollingRateMs;
            BufferByteLength = bufferByteLength;
            WriteTimeoutMS = writeTimeoutMS;
            PortNumber = portNumber;
            BaudRate = baudRate;
            StopBit = stopBit;
            ParityBit = parityBit;
            DataBits = dataBits;
            Handshake = handshake;
            Dtr = dtr;
            Rts = rts;
            PopulatePropertiesFromFields();
        }

        // Copy Constructor used for creating properties objects from default properties
        public SerialDeviceProperties(
            SerialDeviceProperties existing,
            string comPortNumber = null,
            int? pollingRateMs = null,
            int? bufferByteLength = null,
            int? writeTimeoutMS = null,
            int? portNumber = null,
            BaudRate? baudRate = null,
            StopBits? stopBit = null,
            Parity? parityBit = null,
            DataBits? dataBits = null,
            Handshake? handshake = null,
            bool? dtr = null,
            bool? rts = null
        )
        {
            ComPortNumber = comPortNumber ?? existing.ComPortNumber;
            PollTimeoutMs = pollingRateMs ?? existing.PollTimeoutMs;
            BufferByteLength = bufferByteLength ?? existing.BufferByteLength;
            WriteTimeoutMS = writeTimeoutMS ?? existing.WriteTimeoutMS;
            PortNumber = portNumber ?? existing.PortNumber;
            BaudRate = baudRate ?? existing.BaudRate;
            StopBit = stopBit ?? existing.StopBit;
            ParityBit = parityBit ?? existing.ParityBit;
            DataBits = dataBits ?? existing.DataBits;
            Handshake = handshake ?? existing.Handshake;
            Dtr = dtr ?? existing.Dtr;
            Rts = rts ?? existing.Rts;
            PopulatePropertiesFromFields();
        }

        public override ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
    }
}