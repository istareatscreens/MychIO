using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;

namespace MychIO.Device
{
    public class AdxTouchPanel : Device<TouchPanelZone, InputState, SerialDeviceProperties>
    {
        public const string DEVICE_NAME = "AdxTocuhPanel";

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.TouchPanel;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new SerialDeviceProperties(
            comPortNumber: "COM3",
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
        // ** Connection Properties 

        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private static readonly IDictionary<int, IDictionary<byte, TouchPanelZone>> BYTE_MASKS_TO_INPUT = new Dictionary<int, IDictionary<byte, TouchPanelZone>>
        {
            { 1, new Dictionary<byte, TouchPanelZone>
                {
                    // Byte 0 (0x00) masks
                    { 0b00000001, TouchPanelZone.A1 },
                    { 0b00000010, TouchPanelZone.A2 },
                    { 0b00000100, TouchPanelZone.A3 },
                    { 0b00001000, TouchPanelZone.A4 },
                    { 0b00010000, TouchPanelZone.A5 },
                    { 0b00100000, TouchPanelZone.A6 },
                    { 0b01000000, TouchPanelZone.A7 },
                    { 0b10000000, TouchPanelZone.A8 },
                }
            },
            { 2, new Dictionary<byte, TouchPanelZone>
                {
                    // Byte 1 (0x01) masks
                    { 0b00000001, TouchPanelZone.B1 },
                    { 0b00000010, TouchPanelZone.B2 },
                    { 0b00000100, TouchPanelZone.B3 },
                    { 0b00001000, TouchPanelZone.B4 },
                    { 0b00010000, TouchPanelZone.B5 },
                    { 0b00100000, TouchPanelZone.B6 },
                    { 0b01000000, TouchPanelZone.B7 },
                    { 0b10000000, TouchPanelZone.B8 },
                }
            },
            { 3, new Dictionary<byte, TouchPanelZone>
                {
                    // Byte 2 (0x02) masks
                    { 0b00000001, TouchPanelZone.C1 },
                    { 0b00000010, TouchPanelZone.C2 },
                }
            },
            { 4, new Dictionary<byte, TouchPanelZone>
                {
                    // Byte 3 (0x03) masks
                    { 0b00000001, TouchPanelZone.D1 },
                    { 0b00000010, TouchPanelZone.D2 },
                    { 0b00000100, TouchPanelZone.D3 },
                    { 0b00001000, TouchPanelZone.D4 },
                    { 0b00010000, TouchPanelZone.D5 },
                    { 0b00100000, TouchPanelZone.D6 },
                    { 0b01000000, TouchPanelZone.D7 },
                    { 0b10000000, TouchPanelZone.D8 },
                }
            },
            { 5, new Dictionary<byte, TouchPanelZone>
                {
                    // Byte 4 (0x04) masks
                    { 0b00000001, TouchPanelZone.E1 },
                    { 0b00000010, TouchPanelZone.E2 },
                    { 0b00000100, TouchPanelZone.E3 },
                    { 0b00001000, TouchPanelZone.E4 },
                    { 0b00010000, TouchPanelZone.E5 },
                    { 0b00100000, TouchPanelZone.E6 },
                    { 0b01000000, TouchPanelZone.E7 },
                    { 0b10000000, TouchPanelZone.E8 },
                }
            }
        };

        private byte[] _currentState = NO_INPUT_PACKET;
        private int[] _currentActiveStates = new int[34];

        public static readonly IDictionary<TouchPanelCommand, byte[]> Commands = new Dictionary<TouchPanelCommand, byte[]>
        {
            { TouchPanelCommand.Start, new byte[]{ 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } },
            { TouchPanelCommand.Reset, new byte[]{0x7B, 0x52, 0x53, 0x45, 0x54, 0x7D} },
            { TouchPanelCommand.Halt, new byte[]{ 0x7B, 0x48, 0x41, 0x4C, 0x54, 0x7D} },
        };


        public AdxTouchPanel(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        { }


        public override async Task OnStartWrite()
        {
            await Write(TouchPanelCommand.Reset, TouchPanelCommand.Halt);
            for (byte a = 0x41; a <= 0x62; a++)
            {
                await _connection.Write(Encoding.UTF8.GetBytes("{L" + (char)a + "r2}"));
            }

            await Write(TouchPanelCommand.Start);
        }

        public override void ReadData(byte[] data)
        {
            // ensure buffer is aligned
            if (data[0] != '(')
            {
                return;
            }
            // TODO: Microoptimize here we can store this as a member variable to prevent reallocation
            byte[] currentInput = new byte[9];
            Buffer.BlockCopy(data, data.Length - 9, currentInput, 0, 9);
            /* 
                if the current state has no state And
                the newest packet also contains no new input And
                currentState is the same as the data being passed 
                then nothing to do
            */
            if (
                ByteArraysEqual(currentInput, _currentState)
            )
            {
                return;
            }

            foreach (var indexByteAndInput in BYTE_MASKS_TO_INPUT)
            {
                // No change in state from previous record for this zone so skip it
                if (currentInput[indexByteAndInput.Key] == _currentState[indexByteAndInput.Key])
                {
                    continue;
                }

                foreach (var maskToInput in indexByteAndInput.Value)
                {
                    var result = maskToInput.Key & currentInput[indexByteAndInput.Key];
                    // TODO: microoptimize here maybe (we can store the previous computed state for faster access)
                    var previousResult = maskToInput.Key & _currentState[indexByteAndInput.Key];
                    if (previousResult == result)
                    {
                        continue;
                    }
                    //_manager.handleEvent(Event.IOEventType.Debug, DeviceClassification.TouchPanel, maskToInput.ToString() + " | " + HelperFunctions.formatAdxTouchPanelOutput(data));
                    _inputSubscriptions.TryGetValue(maskToInput.Value, out var callback);
                    callback(maskToInput.Value, 1 == result ? InputState.On : InputState.Off);
                }

            }
            _currentState = currentInput;
            //_manager.handleEvent(Event.IOEventType.Debug, GetDeviceClassification(), Encoding.ASCII.GetString(data).Trim().Replace("\0", "_"));
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
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

        // Not used
        public override void ReadData(IntPtr intPtr)
        {
            throw new NotImplementedException();
        }

#if UNITY_EDITOR
        public static string formatAdxTouchPanelOutput(byte[] data)
        {
            return Helper.HelperFunctions.BytesToString(new byte[] { data[0] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[1] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[2] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[3] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[4] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[5] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[6] })
              + " "
              + Helper.HelperFunctions.ByteArrayToBitString(new byte[] { data[7] })
              + " "
              + Helper.HelperFunctions.BytesToString(new byte[] { data[8] })
              ;
        }
#endif
    }
}