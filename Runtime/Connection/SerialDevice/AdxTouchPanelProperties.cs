using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MychIO.Connection.SerialDevice
{
    public class AdxTouchPanelProperties: SerialDeviceProperties
    {
        public bool SensitivityOverride;
        public int Sensitivity;

        public AdxTouchPanelProperties(
            string comPortNumber = "COM21",
            int pollingRateMs = 2,
            int bufferByteLength = 9,
            int writeTimeoutMS = DEFAULT_WRITE_TIMEOUT_MS,
            int portNumber = 0,
            BaudRate baudRate = BaudRate.Bd9600,
            StopBits stopBit = StopBits.One,
            Parity parityBit = Parity.None,
            DataBits dataBits = DataBits.Eight,
            Handshake handshake = Handshake.None,
            bool dtr = false,
            bool rts = false,
            // Device Class specific properties
            int? debounceTimeMs = 0,
            bool sensitivityOverride = false,
            int sensitivity = 0
        ): base(comPortNumber,
            pollingRateMs, 
            bufferByteLength, 
            writeTimeoutMS, 
            portNumber, 
            baudRate, 
            stopBit, 
            parityBit, 
            dataBits, 
            handshake, 
            dtr,
            rts,
            debounceTimeMs)
        {
            SensitivityOverride = sensitivityOverride;
            Sensitivity = sensitivity;
            PopulatePropertiesFromFields();
        }

        public AdxTouchPanelProperties(
            AdxTouchPanelProperties existing,
            string? comPortNumber = "COM21",
            int? pollingRateMs = 2,
            int? bufferByteLength = 9,
            int? writeTimeoutMS = DEFAULT_WRITE_TIMEOUT_MS,
            int? portNumber = 0,
            BaudRate? baudRate = BaudRate.Bd9600,
            StopBits? stopBit = StopBits.One,
            Parity? parityBit = Parity.None,
            DataBits? dataBits = DataBits.Eight,
            Handshake? handshake = Handshake.None,
            bool? dtr = false,
            bool? rts = false,
            // Device Class specific properties
            int? debounceTimeMs = 0,
            bool? sensitivityOverride = false,
            int? sensitivity = 0
        ) : base(existing,
            comPortNumber,
            pollingRateMs,
            bufferByteLength,
            writeTimeoutMS,
            portNumber,
            baudRate,
            stopBit,
            parityBit,
            dataBits,
            handshake,
            dtr,
            rts,
            debounceTimeMs)
        {
            SensitivityOverride = sensitivityOverride ?? existing.SensitivityOverride;
            Sensitivity = sensitivity ?? existing.Sensitivity;
            PopulatePropertiesFromFields();
        }

    }
}
