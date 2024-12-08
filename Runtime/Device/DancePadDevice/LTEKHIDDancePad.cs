using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;

namespace MychIO.Device
{
    public class LTEKHIDDancePad : Device<DancePadZone, InputState, HidDeviceProperties>
    {

        /*
            No buttons  - 0x010000  -  00000001 00000000 00000000 00000000
            Left        - 0x010100  -  00000001 00000001 00000000 00000000
            Right       - 0x010200  -  00000001 00000010 00000000 00000000
            Up          - 0x010400  -  00000001 00000100 00000000 00000000
            Down        - 0x010800  -  00000001 00001000 00000000 00000000
            Minus       - 0x010004  -  00000001 00000000 00000100 00000000
            Plus        - 0x010008  -  00000001 00000000 00001000 00000000
        */

        public const string DEVICE_NAME = "LTEKHIDDancePad";
        // Rather hardcode it here for micro optimization if you need different 
        // settings just copy this class and change these values
        public const int BUFFER_SIZE = 4;
        public const int LEFT_BYTES_TO_TRUNCATE = 1;
        public const int BYTES_TO_READ = 2;

        // Operational constants
        public const byte EMPTY_BYTE = 0x00;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.HID;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.DancePad;
        public static new string GetDeviceName() => DEVICE_NAME;
        public override string DeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new HidDeviceProperties(
            vendorId: 0x03EB,
            productId: 0x8041,
            bufferSize: BUFFER_SIZE,
            leftBytesToTruncate: LEFT_BYTES_TO_TRUNCATE,
            bytesToRead: BYTES_TO_READ,
            pollingRateMs: 0
        );

        public new static HidDeviceProperties GetDefaultDeviceProperties() => (HidDeviceProperties)GetDefaultConnectionProperties();
        // ** Connection Properties
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x00,0x00
        };
        private byte[] _currentState = NO_INPUT_PACKET;
        private IDictionary<DancePadZone, bool> _currentActiveStates;
        public static readonly IDictionary<DancePadCommand, byte[]> Commands = new Dictionary<DancePadCommand, byte[]> { };

        public LTEKHIDDancePad(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            // current states
            _currentActiveStates = new Dictionary<DancePadZone, bool>();
            foreach (DancePadZone zone in Enum.GetValues(typeof(DancePadZone)))
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

            bool HandleLeft(byte input) => handleInputChange(DancePadZone.Left, input, 0b00000001);
            bool HandleRight(byte input) => handleInputChange(DancePadZone.Right, input, 0b00000010);
            bool HandleUp(byte input) => handleInputChange(DancePadZone.Up, input, 0b00000100);
            bool HandleDown(byte input) => handleInputChange(DancePadZone.Down, input, 0b00001000);
            bool HandleMinus(byte input) => handleInputChange(DancePadZone.Minus, input, 0b00000100);
            bool HandlePlus(byte input) => handleInputChange(DancePadZone.Plus, input, 0b00001000);

            if (_currentState[0] != currentInput[0])
            {
                DebouncedHandleInputChange(DancePadZone.Left, HandleLeft, currentInput[0]);
                DebouncedHandleInputChange(DancePadZone.Right, HandleRight, currentInput[0]);
                DebouncedHandleInputChange(DancePadZone.Up, HandleUp, currentInput[0]);
                DebouncedHandleInputChange(DancePadZone.Down, HandleDown, currentInput[0]);
            }

            if (_currentState[1] != currentInput[1])
            {
                DebouncedHandleInputChange(DancePadZone.Minus, HandleMinus, currentInput[1]);
                DebouncedHandleInputChange(DancePadZone.Plus, HandlePlus, currentInput[1]);
            }

            currentInput.CopyTo(_currentState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe override void ReadData(IntPtr pointer)
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

            // Check if the state has changed
            if (ByteArraysEqual(_currentState, currentInput))
            {
                return;
            }

            if (_currentState[0] != currentInput[0])
            {
                handleInputChange(DancePadZone.Left, currentInput[0], 0b00000001);
                handleInputChange(DancePadZone.Right, currentInput[0], 0b00000010);
                handleInputChange(DancePadZone.Up, currentInput[0], 0b00000100);
                handleInputChange(DancePadZone.Down, currentInput[0], 0b00001000);
            }

            if (_currentState[1] != currentInput[1])
            {
                handleInputChange(DancePadZone.Minus, currentInput[1], 0b00000100);
                handleInputChange(DancePadZone.Plus, currentInput[1], 0b00001000);
            }

            currentInput.CopyTo(_currentState);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool handleInputChange(DancePadZone zone, byte input, byte mask)
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
        public override Task Write(params Enum[] interactions)
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