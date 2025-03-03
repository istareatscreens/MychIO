using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;

namespace MychIO.Device
{
    public class AdxIO4ButtonRing : Device<ButtonRingZone, InputState, HidDeviceProperties>
    {

        /*
            no input - 00000000 00000010 00000000 00001101 11111000
            BA1 - 00000000 00000010 00000000 00001001 11111000
            BA2 - 00000000 00000010 00000000 00000101 11111000
            BA3 - 00000000 00000010 00000000 00001100 11111000
            BA4 - 00000000 00000010 00000000 00001101 01111000
            BA5 - 00000000 00000010 00000000 00001101 10111000
            BA6 - 00000000 00000010 00000000 00001101 11011000
            BA7 - 00000000 00000010 00000000 00001101 11101000
            BA8 - 00000000 00000010 00000000 00001101 11110000
            up - 00000000 00000010 00000000 00001111 11111000
            select - 00000000 00000010 00000000 00001101 11111010
            down - 00000000 00000010 00000000 01001101 11111000
            coin - 00000001 00000010 00000000 00001101 11111000
        */

        public const string DEVICE_NAME = "AdxIO4ButtonRing";
        // Rather hardcode it here for micro optimization if you need different 
        // settings just copy this class and change these values
        public const int BUFFER_SIZE = 64;
        public const int LEFT_BYTES_TO_TRUNCATE = 26;
        public const int BYTES_TO_READ = 5;

        // Operational constants
        public const byte NO_INPUT_BYTE3 = 0x0D;
        public const byte NO_INPUT_BYTE4 = 0xF8;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.HID;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.ButtonRing;
        public static new string GetDeviceName() => DEVICE_NAME;
        public override string DeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new HidDeviceProperties(
            vendorId: 0x0CA3,
            productId: 0x0021,
            bufferSize: BUFFER_SIZE,
            leftBytesToTruncate: LEFT_BYTES_TO_TRUNCATE,
            bytesToRead: BYTES_TO_READ,
            pollingRateMs: 0
        );

        public new static HidDeviceProperties GetDefaultDeviceProperties() => (HidDeviceProperties)GetDefaultConnectionProperties();
        // ** Connection Properties
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x00,0x02,0x00,0x0D,0xF8
        };
        private byte[] _currentState = NO_INPUT_PACKET;
        private IDictionary<ButtonRingZone, bool> _currentActiveStates;
        public static readonly IDictionary<ButtonRingCommand, byte[]> Commands = new Dictionary<ButtonRingCommand, byte[]> { };

        public AdxIO4ButtonRing(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            // current states
            _currentActiveStates = new Dictionary<ButtonRingZone, bool>();
            foreach (ButtonRingZone zone in Enum.GetValues(typeof(ButtonRingZone)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override void ReadDataDebounce(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return;
            }

            Span<byte> currentInput = stackalloc byte[BYTES_TO_READ];
            byte* pByte = (byte*)pointer;
            for (int i = 0; i < BYTES_TO_READ; i++)
            {
                currentInput[i] = *(pByte + i);
            }

            if (ByteArraysEqual(_currentState, currentInput))
            {
                return;
            }

            bool HandleBA3(byte input) => handleInputChange(ButtonRingZone.BA3, input, LEAST_SIGNIFICANT_BIT);
            bool HandleArrowUp(byte input) => handleInputChange(ButtonRingZone.ArrowUp, input, 0b00000010);
            bool HandleBA1(byte input) => handleInputChange(ButtonRingZone.BA1, input, 0b00000100);
            bool HandleBA2(byte input) => handleInputChange(ButtonRingZone.BA2, input, 0b00001000);
            bool HandleArrowDown(byte input) => handleInputChange(ButtonRingZone.ArrowDown, input, 0b01000000);

            bool HandleBA4(byte input) => handleInputChange(ButtonRingZone.BA4, input, MOST_SIGNIFICANT_BIT);
            bool HandleBA5(byte input) => handleInputChange(ButtonRingZone.BA5, input, 0b01000000);
            bool HandleBA6(byte input) => handleInputChange(ButtonRingZone.BA6, input, 0b00100000);
            bool HandleBA7(byte input) => handleInputChange(ButtonRingZone.BA7, input, 0b00010000);
            bool HandleBA8(byte input) => handleInputChange(ButtonRingZone.BA8, input, 0b00001000);
            bool HandleSelect(byte input) => handleInputChange(ButtonRingZone.Select, input, 0b00000010);

            bool HandleInsertCoin(byte input) => handleInputChange(ButtonRingZone.InsertCoin, input, LEAST_SIGNIFICANT_BIT);

            if (_currentState[3] != currentInput[3])
            {
                var InvertedByte3 = (byte)~currentInput[3];

                DebouncedHandleInputChange(ButtonRingZone.BA3, HandleBA3, InvertedByte3);
                DebouncedHandleInputChange(ButtonRingZone.ArrowUp, HandleArrowUp, currentInput[3]);
                DebouncedHandleInputChange(ButtonRingZone.BA1, HandleBA1, InvertedByte3);
                DebouncedHandleInputChange(ButtonRingZone.BA2, HandleBA2, InvertedByte3);
                DebouncedHandleInputChange(ButtonRingZone.ArrowDown, HandleArrowDown, currentInput[3]);
            }

            if (_currentState[4] != currentInput[4])
            {
                var InvertedByte4 = (byte)~currentInput[4];

                DebouncedHandleInputChange(ButtonRingZone.BA4, HandleBA4, InvertedByte4);
                DebouncedHandleInputChange(ButtonRingZone.BA5, HandleBA5, InvertedByte4);
                DebouncedHandleInputChange(ButtonRingZone.BA6, HandleBA6, InvertedByte4);
                DebouncedHandleInputChange(ButtonRingZone.BA7, HandleBA7, InvertedByte4);
                DebouncedHandleInputChange(ButtonRingZone.BA8, HandleBA8, InvertedByte4);
                DebouncedHandleInputChange(ButtonRingZone.Select, HandleSelect, currentInput[4]);
            }

            if (_currentState[0] != currentInput[0])
            {
                DebouncedHandleInputChange(ButtonRingZone.InsertCoin, HandleInsertCoin, currentInput[0]);
            }

            currentInput.CopyTo(_currentState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override void ReadData(IntPtr pointer)
        {

            /*
                if the code below causes any crashes or issues it might be better to 
                change this function to safe and copy the bytes this way.
                This is much slower though:

                byte[] currentInput = new byte[BYTES_TO_READ];

                Marshal.Copy(pointer, currentInput, 0, BYTES_TO_READ);
            **/
            /** UNSAFE CODE */
            if (pointer == IntPtr.Zero)
            {
                return;
            }

            Span<byte> currentInput = stackalloc byte[BYTES_TO_READ];
            byte* pByte = (byte*)pointer;
            for (int i = 0; i < BYTES_TO_READ; i++)
            {
                currentInput[i] = *(pByte + i);
            }
            /** UNSAFE CODE */

            // Check if the state has changed
            if (ByteArraysEqual(_currentState, currentInput))
            {
                return;
            }
            /*
                // bit shift right -> off
                BA3  -> byte 3, position 0 OFF
                up  -> byte 3, position 1 ON
                BA1 -> byte 3, position 2 OFF
                BA2 -> byte 3, position 3 OFF
                // bit shift left -> on (new number)
                down -> byte 3, position 1 ON
            */
            if (_currentState[3] != currentInput[3])
            {
                var InvertedByte3 = (byte)~currentInput[3];
                handleInputChange(ButtonRingZone.BA3, InvertedByte3, LEAST_SIGNIFICANT_BIT);

                handleInputChange(ButtonRingZone.ArrowUp, currentInput[3], 0b00000010);

                handleInputChange(ButtonRingZone.BA1, InvertedByte3, 0b00000100);

                handleInputChange(ButtonRingZone.BA2, InvertedByte3, 0b00001000);

                handleInputChange(ButtonRingZone.ArrowDown, currentInput[3], 0b01000000);
            }

            /*
            // bit shift left -> off
                BA4  -> byte 4, position 0 OFF
                BA5  -> byte 4, position 1 OFF
                BA6  -> byte 4, position 2 OFF
                BA7  -> byte 4, position 3 OFF
                BA8  -> byte 4, position 4 OFF
                Select -> byte 4, position 6 ON
            */
            if (_currentState[4] != currentInput[3])
            {
                var InvertedByte4 = (byte)~currentInput[4];

                handleInputChange(ButtonRingZone.BA4, InvertedByte4, MOST_SIGNIFICANT_BIT);

                handleInputChange(ButtonRingZone.BA5, InvertedByte4, 0b01000000);

                handleInputChange(ButtonRingZone.BA6, InvertedByte4, 0b00100000);

                handleInputChange(ButtonRingZone.BA7, InvertedByte4, 0b00010000);

                handleInputChange(ButtonRingZone.BA8, InvertedByte4, 0b00001000);

                handleInputChange(ButtonRingZone.Select, currentInput[4], 0b00000010);
            }

            // coin -> byte 0 (00000001)
            if (_currentState[0] != currentInput[0])
            {
                handleInputChange(
                    ButtonRingZone.InsertCoin,
                    currentInput[0],
                    LEAST_SIGNIFICANT_BIT
                );
            }

            currentInput.CopyTo(_currentState);
            //_currentState = currentInput;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool handleInputChange(ButtonRingZone zone, byte input, byte mask)
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

        // source: https://stackoverflow.com/a/48599119
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        // Not used
        public override Task Write<T>(params T[] interactions)
        {
            return Task.CompletedTask;
        }
        public override Task OnStartWrite()
        {
            return Task.CompletedTask;
        }
        public override void ReadData(byte[] data) { }

        public override void ReadDataDebounce(byte[] data) { }

        public override Task OnDisconnectWrite()
        {
            return Task.CompletedTask;
        }
    }
}