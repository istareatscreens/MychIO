using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;
using MychIO.Helper;

namespace MychIO.Device
{
    public class AdxTouchPanel : Device<TouchPanelZone, InputState, SerialDeviceProperties>
    {

        /**
            // Byte 0 = (
            // Byte 1 
             0b00000001, TouchPanelZone.A1 
             0b00000010, TouchPanelZone.A2 
             0b00000100, TouchPanelZone.A3 
             0b00001000, TouchPanelZone.A4 
             0b00010000, TouchPanelZone.A5 

            // Byte 2 
             0b00000001, TouchPanelZone.A6 
             0b00000010, TouchPanelZone.A7 
             0b00000100, TouchPanelZone.A8 
             0b00001000, TouchPanelZone.B1 
             0b00010000, TouchPanelZone.B2 

            // Byte 3
             0b00000001, TouchPanelZone.B3 
             0b00000010, TouchPanelZone.B4 
             0b00000100, TouchPanelZone.B5 
             0b00001000, TouchPanelZone.B6 
             0b00010000, TouchPanelZone.B7 
            // Byte 4
             0b00000001, TouchPanelZone.B8 
             0b00000010, TouchPanelZone.C1 
             0b00000100, TouchPanelZone.C2 
             0b00001000, TouchPanelZone.D1 
             0b00010000, TouchPanelZone.D2 

            // Byte 5 (0x03) masks
             0b00000001, TouchPanelZone.D3 
             0b00000010, TouchPanelZone.D4 
             0b00000100, TouchPanelZone.D5 
             0b00001000, TouchPanelZone.D6 
             0b00010000, TouchPanelZone.D7 
            // Byte 6 (0x04) masks
             0b10000001, TouchPanelZone.D8 
             ...
        */

        public const string DEVICE_NAME = "AdxTouchPanel";

        // Settings for microoptimization
        public const int BYTES_TO_READ = 9;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.TouchPanel;
        public static new string GetDeviceName() => DEVICE_NAME;
        public override string DeviceName() => DEVICE_NAME;
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
        public new static SerialDeviceProperties GetDefaultDeviceProperties() => (SerialDeviceProperties)GetDefaultConnectionProperties();

        // ** Connection Properties 

        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private byte[] _currentState = NO_INPUT_PACKET;
        //private byte[] _currentInput = new byte[BYTES_TO_READ];
        private IDictionary<TouchPanelZone, bool> _currentActiveStates;

        public static readonly IDictionary<TouchPanelCommand, byte[][]> Commands = new Dictionary<TouchPanelCommand, byte[][]>
        {
            { TouchPanelCommand.Start, new byte[][] { new byte[] { 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } } },
            { TouchPanelCommand.Reset, new byte[][] { new byte[] { 0x7B, 0x52, 0x53, 0x45, 0x54, 0x7D } } },
            { TouchPanelCommand.Halt, new byte[][] { new byte[] { 0x7B, 0x48, 0x41, 0x4C, 0x54, 0x7D } } },
        };

        public AdxTouchPanel(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            // current states
            _currentActiveStates = new Dictionary<TouchPanelZone, bool>();
            foreach (TouchPanelZone zone in Enum.GetValues(typeof(TouchPanelZone)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override async Task OnStartWrite()
        {
            await Write(TouchPanelCommand.Reset, TouchPanelCommand.Halt);
            // Calibration
            for (byte a = 0x41; a <= 0x62; a++)
            {
                await _connection.Write(Encoding.UTF8.GetBytes("{L" + (char)a + "r2}"));
            }

            await Write(TouchPanelCommand.Start);
        }

        public override void ReadDataDebounce(byte[] data)
        {
            if (data[0] != '(')
            {
                return;
            }

            var _data = data.AsSpan().Slice(data.Length - BYTES_TO_READ);
            Span<byte> currentInput = stackalloc byte[BYTES_TO_READ];
            _data.CopyTo(currentInput);

            if (ByteArraysEqual(_currentState, currentInput))
            {
                return;
            }

            bool HandleA1(byte input) => handleInputChange(TouchPanelZone.A1, input, 0b00000001);
            bool HandleA2(byte input) => handleInputChange(TouchPanelZone.A2, input, 0b00000010);
            bool HandleA3(byte input) => handleInputChange(TouchPanelZone.A3, input, 0b00000100);
            bool HandleA4(byte input) => handleInputChange(TouchPanelZone.A4, input, 0b00001000);
            bool HandleA5(byte input) => handleInputChange(TouchPanelZone.A5, input, 0b00010000);

            if (currentInput[1] != _currentState[1])
            {
                DebouncedHandleInputChange(TouchPanelZone.A1, HandleA1, currentInput[1]);
                DebouncedHandleInputChange(TouchPanelZone.A2, HandleA2, currentInput[1]);
                DebouncedHandleInputChange(TouchPanelZone.A3, HandleA3, currentInput[1]);
                DebouncedHandleInputChange(TouchPanelZone.A4, HandleA4, currentInput[1]);
                DebouncedHandleInputChange(TouchPanelZone.A5, HandleA5, currentInput[1]);
            }

            bool HandleA6(byte input) => handleInputChange(TouchPanelZone.A6, input, 0b00000001);
            bool HandleA7(byte input) => handleInputChange(TouchPanelZone.A7, input, 0b00000010);
            bool HandleA8(byte input) => handleInputChange(TouchPanelZone.A8, input, 0b00000100);
            bool HandleB1(byte input) => handleInputChange(TouchPanelZone.B1, input, 0b00001000);
            bool HandleB2(byte input) => handleInputChange(TouchPanelZone.B2, input, 0b00010000);

            if (currentInput[2] != _currentState[2])
            {
                DebouncedHandleInputChange(TouchPanelZone.A6, HandleA6, currentInput[2]);
                DebouncedHandleInputChange(TouchPanelZone.A7, HandleA7, currentInput[2]);
                DebouncedHandleInputChange(TouchPanelZone.A8, HandleA8, currentInput[2]);
                DebouncedHandleInputChange(TouchPanelZone.B1, HandleB1, currentInput[2]);
                DebouncedHandleInputChange(TouchPanelZone.B2, HandleB2, currentInput[2]);
            }

            bool HandleB3(byte input) => handleInputChange(TouchPanelZone.B3, input, 0b00000001);
            bool HandleB4(byte input) => handleInputChange(TouchPanelZone.B4, input, 0b00000010);
            bool HandleB5(byte input) => handleInputChange(TouchPanelZone.B5, input, 0b00000100);
            bool HandleB6(byte input) => handleInputChange(TouchPanelZone.B6, input, 0b00001000);
            bool HandleB7(byte input) => handleInputChange(TouchPanelZone.B7, input, 0b00010000);

            if (currentInput[3] != _currentState[3])
            {
                DebouncedHandleInputChange(TouchPanelZone.B3, HandleB3, currentInput[3]);
                DebouncedHandleInputChange(TouchPanelZone.B4, HandleB4, currentInput[3]);
                DebouncedHandleInputChange(TouchPanelZone.B5, HandleB5, currentInput[3]);
                DebouncedHandleInputChange(TouchPanelZone.B6, HandleB6, currentInput[3]);
                DebouncedHandleInputChange(TouchPanelZone.B7, HandleB7, currentInput[3]);
            }

            bool HandleB8(byte input) => handleInputChange(TouchPanelZone.B8, input, 0b00000001);
            bool HandleC1(byte input) => handleInputChange(TouchPanelZone.C1, input, 0b00000010);
            bool HandleC2(byte input) => handleInputChange(TouchPanelZone.C2, input, 0b00000100);
            bool HandleD1(byte input) => handleInputChange(TouchPanelZone.D1, input, 0b00001000);
            bool HandleD2(byte input) => handleInputChange(TouchPanelZone.D2, input, 0b00010000);

            if (currentInput[4] != _currentState[4])
            {
                DebouncedHandleInputChange(TouchPanelZone.B8, HandleB8, currentInput[4]);
                DebouncedHandleInputChange(TouchPanelZone.C1, HandleC1, currentInput[4]);
                DebouncedHandleInputChange(TouchPanelZone.C2, HandleC2, currentInput[4]);
                DebouncedHandleInputChange(TouchPanelZone.D1, HandleD1, currentInput[4]);
                DebouncedHandleInputChange(TouchPanelZone.D2, HandleD2, currentInput[4]);
            }

            bool HandleD3(byte input) => handleInputChange(TouchPanelZone.D3, input, 0b00000001);
            bool HandleD4(byte input) => handleInputChange(TouchPanelZone.D4, input, 0b00000010);
            bool HandleD5(byte input) => handleInputChange(TouchPanelZone.D5, input, 0b00000100);
            bool HandleD6(byte input) => handleInputChange(TouchPanelZone.D6, input, 0b00001000);
            bool HandleD7(byte input) => handleInputChange(TouchPanelZone.D7, input, 0b00010000);

            if (currentInput[5] != _currentState[5])
            {
                DebouncedHandleInputChange(TouchPanelZone.D3, HandleD3, currentInput[5]);
                DebouncedHandleInputChange(TouchPanelZone.D4, HandleD4, currentInput[5]);
                DebouncedHandleInputChange(TouchPanelZone.D5, HandleD5, currentInput[5]);
                DebouncedHandleInputChange(TouchPanelZone.D6, HandleD6, currentInput[5]);
                DebouncedHandleInputChange(TouchPanelZone.D7, HandleD7, currentInput[5]);
            }

            bool HandleD8(byte input) => handleInputChange(TouchPanelZone.D8, input, 0b00000001);
            bool HandleE1(byte input) => handleInputChange(TouchPanelZone.E1, input, 0b00000010);
            bool HandleE2(byte input) => handleInputChange(TouchPanelZone.E2, input, 0b00000100);
            bool HandleE3(byte input) => handleInputChange(TouchPanelZone.E3, input, 0b00001000);
            bool HandleE4(byte input) => handleInputChange(TouchPanelZone.E4, input, 0b00010000);

            if (currentInput[6] != _currentState[6])
            {
                DebouncedHandleInputChange(TouchPanelZone.D8, HandleD8, currentInput[6]);
                DebouncedHandleInputChange(TouchPanelZone.E1, HandleE1, currentInput[6]);
                DebouncedHandleInputChange(TouchPanelZone.E2, HandleE2, currentInput[6]);
                DebouncedHandleInputChange(TouchPanelZone.E3, HandleE3, currentInput[6]);
                DebouncedHandleInputChange(TouchPanelZone.E4, HandleE4, currentInput[6]);
            }

            bool HandleE5(byte input) => handleInputChange(TouchPanelZone.E5, input, 0b00000001);
            bool HandleE6(byte input) => handleInputChange(TouchPanelZone.E6, input, 0b00000010);
            bool HandleE7(byte input) => handleInputChange(TouchPanelZone.E7, input, 0b00000100);
            bool HandleE8(byte input) => handleInputChange(TouchPanelZone.E8, input, 0b00001000);

            if (currentInput[7] != _currentState[7])
            {
                DebouncedHandleInputChange(TouchPanelZone.E5, HandleE5, currentInput[7]);
                DebouncedHandleInputChange(TouchPanelZone.E6, HandleE6, currentInput[7]);
                DebouncedHandleInputChange(TouchPanelZone.E7, HandleE7, currentInput[7]);
                DebouncedHandleInputChange(TouchPanelZone.E8, HandleE8, currentInput[7]);
            }

            currentInput.CopyTo(_currentState);
        }

        public override void ReadData(byte[] data)
        {
            // ensure buffer is aligned
            if (data[0] != '(')
            {
                return;
            }

            byte[] currentInput = new byte[BYTES_TO_READ];

            Buffer.BlockCopy(data, data.Length - 9, currentInput, 0, 9);

            if (ByteArraysEqual(_currentState, currentInput))
            {
                return;
            }

            if (currentInput[1] != _currentState[1])
            {
                handleInputChange(TouchPanelZone.A1, currentInput[1], 0b00000001);
                handleInputChange(TouchPanelZone.A2, currentInput[1], 0b00000010);
                handleInputChange(TouchPanelZone.A3, currentInput[1], 0b00000100);
                handleInputChange(TouchPanelZone.A4, currentInput[1], 0b00001000);
                handleInputChange(TouchPanelZone.A5, currentInput[1], 0b00010000);
            }

            if (currentInput[2] != _currentState[2])
            {
                handleInputChange(TouchPanelZone.A6, currentInput[2], 0b00000001);
                handleInputChange(TouchPanelZone.A7, currentInput[2], 0b00000010);
                handleInputChange(TouchPanelZone.A8, currentInput[2], 0b00000100);
                handleInputChange(TouchPanelZone.B1, currentInput[2], 0b00001000);
                handleInputChange(TouchPanelZone.B2, currentInput[2], 0b00010000);
            }

            if (currentInput[3] != _currentState[3])
            {
                handleInputChange(TouchPanelZone.B3, currentInput[3], 0b00000001);
                handleInputChange(TouchPanelZone.B4, currentInput[3], 0b00000010);
                handleInputChange(TouchPanelZone.B5, currentInput[3], 0b00000100);
                handleInputChange(TouchPanelZone.B6, currentInput[3], 0b00001000);
                handleInputChange(TouchPanelZone.B7, currentInput[3], 0b00010000);
            }

            if (currentInput[4] != _currentState[4])
            {
                handleInputChange(TouchPanelZone.B8, currentInput[4], 0b00000001);
                handleInputChange(TouchPanelZone.C1, currentInput[4], 0b00000010);
                handleInputChange(TouchPanelZone.C2, currentInput[4], 0b00000100);
                handleInputChange(TouchPanelZone.D1, currentInput[4], 0b00001000);
                handleInputChange(TouchPanelZone.D2, currentInput[4], 0b00010000);
            }

            if (currentInput[5] != _currentState[5])
            {
                handleInputChange(TouchPanelZone.D3, currentInput[5], 0b00000001);
                handleInputChange(TouchPanelZone.D4, currentInput[5], 0b00000010);
                handleInputChange(TouchPanelZone.D5, currentInput[5], 0b00000100);
                handleInputChange(TouchPanelZone.D6, currentInput[5], 0b00001000);
                handleInputChange(TouchPanelZone.D7, currentInput[5], 0b00010000);
            }
            if (currentInput[6] != _currentState[6])
            {
                handleInputChange(TouchPanelZone.D8, currentInput[6], 0b00000001);
                handleInputChange(TouchPanelZone.E1, currentInput[6], 0b00000010);
                handleInputChange(TouchPanelZone.E2, currentInput[6], 0b00000100);
                handleInputChange(TouchPanelZone.E3, currentInput[6], 0b00001000);
                handleInputChange(TouchPanelZone.E4, currentInput[6], 0b00010000);
            }

            if (currentInput[7] != _currentState[7])
            {
                handleInputChange(TouchPanelZone.E5, currentInput[7], 0b00000001);
                handleInputChange(TouchPanelZone.E6, currentInput[7], 0b00000010);
                handleInputChange(TouchPanelZone.E7, currentInput[7], 0b00000100);
                handleInputChange(TouchPanelZone.E8, currentInput[7], 0b00001000);
            }

            _currentState = currentInput;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool handleInputChange(TouchPanelZone zone, byte input, byte mask)
        {
            var currentActiveState = _currentActiveStates[zone];
            if (((input & mask) != 0) != currentActiveState)
            {
                _inputSubscriptions[zone]
                (
                    zone,
                    currentActiveState ? InputState.Off : InputState.On
                );
                _currentActiveStates[zone] = !currentActiveState;
                return true;
            }
            return false;
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
                if (Commands.TryGetValue(command, out byte[][] bytes))
                {
                    return bytes;
                }
                else
                {
                    throw new ArgumentException("Command not found.", nameof(command));
                }
            }).ToArray();

            foreach (var command in commandBytes)
            {
                await _connection.Write(command);
            }
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
        public override void ReadDataDebounce(IntPtr intPtr) { }
        public override Task OnDisconnectWrite()
        {
            return Task.CompletedTask;
        }

#if UNITY_EDITOR
        public static string formatAdxTouchPanelOutput(byte[] data)
        {
            return Helper.HelperFunctions.BytesToString(new byte[] { data[0] })
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[1])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[2])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[3])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[4])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[5])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[6])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[7])
              + " "
              + Helper.HelperFunctions.BytesToString(new byte[] { data[8] })
              ;
        }
#endif
    }
}