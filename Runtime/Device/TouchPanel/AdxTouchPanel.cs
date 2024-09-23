using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;

namespace MychIO.Device
{
    public class AdxTouchPanel : Device<TouchPanelZone, SerialDeviceProperties>
    {
        private static readonly byte[] NO_INPUT = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private static readonly Dictionary<TouchPanelZone, byte> BYTE_MASKS = new Dictionary<TouchPanelZone, byte>
        {
            // Byte 0 (0x00) masks
            { TouchPanelZone.A1, 0b00000001 },
            { TouchPanelZone.A2, 0b00000010 },
            { TouchPanelZone.A3, 0b00000100 },
            { TouchPanelZone.A4, 0b00001000 },
            { TouchPanelZone.A5, 0b00010000 },
            { TouchPanelZone.A6, 0b00100000 },
            { TouchPanelZone.A7, 0b01000000 },
            { TouchPanelZone.A8, 0b10000000 },
            // Byte 1 (0x01) masks
            { TouchPanelZone.B1, 0b00000001 },
            { TouchPanelZone.B2, 0b00000010 },
            { TouchPanelZone.B3, 0b00000100 },
            { TouchPanelZone.B4, 0b00001000 },
            { TouchPanelZone.B5, 0b00010000 },
            { TouchPanelZone.B6, 0b00100000 },
            { TouchPanelZone.B7, 0b01000000 },
            { TouchPanelZone.B8, 0b10000000 },
            // Byte 2 (0x02) masks
            { TouchPanelZone.C1, 0b00000001 },
            { TouchPanelZone.C2, 0b00000010 },
            // Byte 3 (0x03) mask 
            { TouchPanelZone.D1, 0b00000001 },
            { TouchPanelZone.D2, 0b00000010 },
            { TouchPanelZone.D3, 0b00000100 },
            { TouchPanelZone.D4, 0b00001000 },
            { TouchPanelZone.D5, 0b00010000 },
            { TouchPanelZone.D6, 0b00100000 },
            { TouchPanelZone.D7, 0b01000000 },
            { TouchPanelZone.D8, 0b10000000 },
            // Byte 4 (0x04) masks
            { TouchPanelZone.E1, 0b00000001 },
            { TouchPanelZone.E2, 0b00000010 },
            { TouchPanelZone.E3, 0b00000100 },
            { TouchPanelZone.E4, 0b00001000 },
            { TouchPanelZone.E5, 0b00010000 },
            { TouchPanelZone.E6, 0b00100000 },
            { TouchPanelZone.E7, 0b01000000 },
            { TouchPanelZone.E8, 0b10000000 },
        };
        private byte[] _currentState;

        public static readonly IDictionary<TouchPanelCommand, byte[]> Commands = new Dictionary<TouchPanelCommand, byte[]>
        {
            { TouchPanelCommand.Start, new byte[]{ 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } }
        };

        public const string ADX_TOUCH_PANEL = "AdxTocuhPanel";

        public AdxTouchPanel(
            IDictionary<TouchPanelZone, Action<TouchPanelZone, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null
        ) : base(inputSubscriptions, connectionProperties)
        { }

        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.TouchPanel;
        public static new string GetDeviceName() => ADX_TOUCH_PANEL;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new SerialDeviceProperties(
            comPortNumber: "COM21",
            writeTimeoutMS: SerialDeviceProperties.DEFAULT_WRITE_TIMEOUT_MS,
            bufferByteLength: 9,
            pollingRateMs: 10,
            portNumber: 0,
            baudRate: BaudRate.Bd9600,
            stopBit: StopBits.One,
            parityBit: Parity.None,
            dataBits: DataBits.Eight,
            handshake: Handshake.None,
            dtr: false,
            rts: false
        );

        public override async Task OnStartWrite()
        {
            await Write(TouchPanelCommand.Start);
        }

        public override void ReadData(byte[] data)
        {
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT;
        }

        public override async Task Write(params Enum[] interactions)
        {
            var commandBytes = interactions.OfType<TouchPanelCommand>()
            .SelectMany(command =>
            {
                if (Commands.TryGetValue(command, out byte[] bytes))
                {
                    return bytes;
                }
                else
                {
                    throw new ArgumentException("Command not found.", nameof(command));
                }
            }).ToArray();

            await _connection.Write(commandBytes);
        }

        // source: https://stackoverflow.com/a/48599119
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}