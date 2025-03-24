using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;
using MychIO.Helper;

namespace MychIO.Device
{
    public class AdxHIDButtonRing : Device<ButtonRingZone, InputState, HidDeviceProperties>
    {

        /*              byte index
            No Input:   ?
            BA1:        4 
            BA2:        3
            BA3:        2
            BA4:        1
            BA5:        8
            BA6:        7
            BA7:        6
            BA8:        5
            up :        9
            select:     10
            down:       11
            coin:       12
        */
        public override string Name
        {
            get => DEVICE_NAME;
        }
        public override bool CanRead
        {
            get => true;
        }
        public override bool CanWrite
        {
            get => false;
        }
        public const string DEVICE_NAME = "AdxHIDButtonRing";
        // Rather hardcode it here for micro optimization if you need different 
        // settings just copy this class and change these values
        public const int BUFFER_SIZE = 13;
        public const int LEFT_BYTES_TO_TRUNCATE = 1;
        public const int BYTES_TO_READ = 12;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.HID;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.ButtonRing;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new HidDeviceProperties(
            vendorId: 0x2e3c,
            productId: 0x5750,
            bufferSize: BUFFER_SIZE,
            leftBytesToTruncate: LEFT_BYTES_TO_TRUNCATE,
            bytesToRead: BYTES_TO_READ,
            pollingRateMs: 0
        );
        public new static HidDeviceProperties GetDefaultDeviceProperties() => (HidDeviceProperties)GetDefaultConnectionProperties();
        // ** Connection Properties
        private static readonly ReadOnlyMemory<byte> NO_INPUT_PACKET = new byte[]
        {
            0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,
            0x00,0x00
        };
        private byte[] _currentState = new byte[BYTES_TO_READ];
        private IDictionary<ButtonRingZone, bool> _currentActiveStates;
        readonly DebounceCallbackHandler<ButtonRingZone, byte> _debounceCallbackHandler;
        public static readonly IDictionary<ButtonRingCommand, byte[]> Commands = new Dictionary<ButtonRingCommand, byte[]> { };

        public AdxHIDButtonRing(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            _debounceCallbackHandler = HandleInputChangeInternal;
            NO_INPUT_PACKET.CopyTo(_currentState);
            // current states
            _currentActiveStates = new Dictionary<ButtonRingZone, bool>();
            foreach (ButtonRingZone zone in Enum.GetValues(typeof(ButtonRingZone)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override void ResetState()
        {
            NO_INPUT_PACKET.CopyTo(_currentState);
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
            Span<byte> fromDeviceData = new Span<byte>((void*)pointer, BYTES_TO_READ);
            Span<byte> currentInput = stackalloc byte[BYTES_TO_READ];
            fromDeviceData.CopyTo(currentInput);
            ReadData(currentInput);
            /** UNSAFE CODE */
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReadData(ReadOnlySpan<byte> data)
        {
            // Check if the state has changed
            //if (ByteArraysEqual(_currentState, data))
            //{
            //    return;
            //}

            HandleInputChangeInternal(ButtonRingZone.BA3, data[1]);
            HandleInputChangeInternal(ButtonRingZone.ArrowUp, data[8]);
            HandleInputChangeInternal(ButtonRingZone.BA1, data[3]);
            HandleInputChangeInternal(ButtonRingZone.BA2, data[2]);
            HandleInputChangeInternal(ButtonRingZone.ArrowDown, data[10]);
            HandleInputChangeInternal(ButtonRingZone.BA4, data[0]);
            HandleInputChangeInternal(ButtonRingZone.BA5, data[7]);
            HandleInputChangeInternal(ButtonRingZone.BA6, data[6]);
            HandleInputChangeInternal(ButtonRingZone.BA7, data[5]);
            HandleInputChangeInternal(ButtonRingZone.BA8, data[4]);
            HandleInputChangeInternal(ButtonRingZone.Select, data[9]);
            HandleInputChangeInternal(ButtonRingZone.InsertCoin, data[11]);

            data.CopyTo(_currentState);
        }
        public unsafe override void ReadDataWithDebounce(IntPtr pointer)
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
            Span<byte> fromDeviceData = new Span<byte>((void*)pointer, BYTES_TO_READ);
            Span<byte> currentInput = stackalloc byte[BYTES_TO_READ];
            fromDeviceData.CopyTo(currentInput);
            ReadDataWithDebounce(currentInput);
            /** UNSAFE CODE */
        }
        public override void ReadDataWithDebounce(ReadOnlySpan<byte> data)
        {
            // Check if the state has changed
            //if (ByteArraysEqual(_currentState, data))
            //{
            //    return;
            //}

            DebounceHandle(ButtonRingZone.BA3, _debounceCallbackHandler, ButtonRingZone.BA3,data[1]);
            DebounceHandle(ButtonRingZone.ArrowUp, _debounceCallbackHandler, ButtonRingZone.ArrowUp, data[8]);
            DebounceHandle(ButtonRingZone.BA1, _debounceCallbackHandler, ButtonRingZone.BA1, data[3]);
            DebounceHandle(ButtonRingZone.BA2, _debounceCallbackHandler, ButtonRingZone.BA2, data[2]);
            DebounceHandle(ButtonRingZone.ArrowDown, _debounceCallbackHandler, ButtonRingZone.ArrowDown, data[10]);
            DebounceHandle(ButtonRingZone.BA4, _debounceCallbackHandler, ButtonRingZone.BA4, data[0]);
            DebounceHandle(ButtonRingZone.BA5, _debounceCallbackHandler, ButtonRingZone.BA5, data[7]);
            DebounceHandle(ButtonRingZone.BA6, _debounceCallbackHandler, ButtonRingZone.BA6, data[6]);
            DebounceHandle(ButtonRingZone.BA7, _debounceCallbackHandler, ButtonRingZone.BA7, data[5]);
            DebounceHandle(ButtonRingZone.BA8, _debounceCallbackHandler, ButtonRingZone.BA8, data[4]);
            DebounceHandle(ButtonRingZone.Select, _debounceCallbackHandler, ButtonRingZone.Select, data[9]);
            DebounceHandle(ButtonRingZone.InsertCoin, _debounceCallbackHandler, ButtonRingZone.InsertCoin, data[11]);

            data.CopyTo(_currentState);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleInputChangeInternal(ButtonRingZone zone, byte input)
        {
            var currentState =  _currentActiveStates[zone];
            var newState = LEAST_SIGNIFICANT_BIT == input;
            var callback = _inputSubscriptions[zone];
            callback(zone,
                     newState ? InputState.Off : InputState.On);
            _currentActiveStates[zone] = newState;
            return newState != currentState;
        }

        // source: https://stackoverflow.com/a/48599119
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        // Not used
        public override void Write<T>(params T[] interactions)
        {
            ThrowHelper.NotSupported();
        }
        public override Task WriteAsync<T>(params T[] interactions)
        {
            return ThrowHelper.NotSupported<Task>();
        }
    }
}