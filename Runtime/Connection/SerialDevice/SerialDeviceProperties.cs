using System.Collections.Generic;
using System.Linq.Expressions;
using MychIO.Helper;

namespace MychIO.Connection.SerialDevice
{
    public class SerialDeviceProperties : ConnectionProperties
    {
        public const int DEFAULT_READ_TIMEOUT_MS = 3000;
        public const int DEFAULT_WRITE_TIMEOUT_MS = 100;
        // WARNING: Adding {get; set;} will break serialization/unserialization of properties!
        public string ComPortNumber;
        public int PollTimeoutMs;
        public int BufferByteLength;
        public int ReadTimeoutMS;
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
            int pollingRateMs = 2,
            int bufferByteLength = 9,
            int readTimeoutMS = DEFAULT_READ_TIMEOUT_MS,
            int writeTimeoutMS = DEFAULT_WRITE_TIMEOUT_MS,
            int portNumber = 0,
            BaudRate baudRate = BaudRate.Bd9600,
            StopBits stopBit = StopBits.One,
            Parity parityBit = Parity.None,
            DataBits dataBits = DataBits.Eight,
            Handshake handshake = Handshake.None,
            bool dtr = false,
            bool rts = false,
            int? debounceTimeMs = 0
        ) : base(
            GenerateUniqueIdentifier(
                comPortNumber,
                pollingRateMs,
                bufferByteLength,
                readTimeoutMS,
                writeTimeoutMS,
                portNumber,
                baudRate,
                stopBit,
                parityBit,
                dataBits,
                handshake,
                dtr,
                rts,
                debounceTimeMs ?? 0
            ),
            debounceTimeMs ?? 0
        )
        {
            ComPortNumber = comPortNumber;
            PollTimeoutMs = pollingRateMs;
            BufferByteLength = bufferByteLength;
            ReadTimeoutMS = readTimeoutMS;
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
            int? readTimeoutMS = null,
            int? writeTimeoutMS = null,
            int? portNumber = null,
            BaudRate? baudRate = null,
            StopBits? stopBit = null,
            Parity? parityBit = null,
            DataBits? dataBits = null,
            Handshake? handshake = null,
            bool? dtr = null,
            bool? rts = null,
            int? debounceTimeMs = 0
        ) : base(
            GenerateUniqueIdentifier(
             comPortNumber ?? existing.ComPortNumber,
             pollingRateMs ?? existing.PollTimeoutMs,
             bufferByteLength ?? existing.BufferByteLength,
             readTimeoutMS ?? existing.ReadTimeoutMS,
             writeTimeoutMS ?? existing.WriteTimeoutMS,
             portNumber ?? existing.PortNumber,
             baudRate ?? existing.BaudRate,
             stopBit ?? existing.StopBit,
             parityBit ?? existing.ParityBit,
             dataBits ?? existing.DataBits,
             handshake ?? existing.Handshake,
             dtr ?? existing.Dtr,
             rts ?? existing.Rts
            ),
            debounceTimeMs ?? 0
        )
        {
            ComPortNumber = comPortNumber ?? existing.ComPortNumber;
            PollTimeoutMs = pollingRateMs ?? existing.PollTimeoutMs;
            BufferByteLength = bufferByteLength ?? existing.BufferByteLength;
            ReadTimeoutMS = readTimeoutMS ?? existing.ReadTimeoutMS;
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

        private static string GenerateUniqueIdentifier(
            string comPortNumber,
            int pollingRateMs,
            int bufferByteLength,
            int readTimeoutMS,
            int writeTimeoutMS,
            int portNumber,
            BaudRate baudRate,
            StopBits stopBit,
            Parity parityBit,
            DataBits dataBits,
            Handshake handshake,
            bool dtr,
            bool rts,
            int? debounceTimeMs = 0
        )
        {
            return HelperFunctions.GenerateUniqueHashFromStrings(new string[] {
                comPortNumber.ToString(),
                pollingRateMs.ToString(),
                bufferByteLength.ToString(),
                readTimeoutMS.ToString(),
                writeTimeoutMS.ToString(),
                portNumber.ToString(),
                baudRate.ToString(),
                stopBit.ToString(),
                parityBit.ToString(),
                dataBits.ToString(),
                handshake.ToString(),
                dtr.ToString(),
                rts.ToString(),
                debounceTimeMs.HasValue ? debounceTimeMs.Value.ToString() : "0"
            });
        }

        public override ConnectionType ConnectionType
        {
            get => ConnectionType.SerialDevice;
        }
    }
}