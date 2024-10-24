using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;
using MychIO.Helper;
using UnityEditorInternal;
using UnityEngine;

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
        public static new IConnectionProperties GetDefaultConnectionProperties() => new HidDeviceProperties(
            vendorId: 0x0CA3,
            productId: 0x0021,
            bufferSize: BUFFER_SIZE,
            leftBytesToTruncate: LEFT_BYTES_TO_TRUNCATE,
            bytesToRead: BYTES_TO_READ
        );
        // ** Connection Properties
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x00,0x02,0x00,0x0D,0xF8
        };
        private byte[] _currentState = NO_INPUT_PACKET;
        private IDictionary<ButtonRingZone, bool> _currentActiveStates = new Dictionary<ButtonRingZone, bool>
        {
            { ButtonRingZone.BA1, false },
            { ButtonRingZone.BA2, false },
            { ButtonRingZone.BA3, false },
            { ButtonRingZone.BA4, false },
            { ButtonRingZone.BA5, false },
            { ButtonRingZone.BA6, false },
            { ButtonRingZone.BA7, false },
            { ButtonRingZone.BA8, false },
            { ButtonRingZone.ArrowUp, false },
            { ButtonRingZone.Select, false },
            { ButtonRingZone.ArrowDown, false },
            { ButtonRingZone.InsertCoin, false },
        };

        public static readonly IDictionary<TouchPanelCommand, byte[]> Commands = new Dictionary<TouchPanelCommand, byte[]>
        {
            { TouchPanelCommand.Start, new byte[]{ 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } },
            { TouchPanelCommand.Reset, new byte[]{0x7B, 0x52, 0x53, 0x45, 0x54, 0x7D} },
            { TouchPanelCommand.Halt, new byte[]{ 0x7B, 0x48, 0x41, 0x4C, 0x54, 0x7D} },
        };

        public AdxIO4ButtonRing(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        { }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

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

            byte[] currentInput = new byte[BYTES_TO_READ];
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

                handleInputChange(ButtonRingZone.BA3, (InvertedByte3 & LEAST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.ArrowUp, ((currentInput[3] >> 1) & LEAST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.BA1, ((InvertedByte3 >> 2) & LEAST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.BA2, ((InvertedByte3 >> 3) & LEAST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.ArrowDown, ((currentInput[3] << 1) & MOST_SIGNIFICANT_BIT) != 0);
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

                handleInputChange(ButtonRingZone.BA4, (InvertedByte4 & MOST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.BA5, ((InvertedByte4 << 1) & MOST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.BA6, ((InvertedByte4 << 2) & MOST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.BA7, ((InvertedByte4 << 3) & MOST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.BA8, ((InvertedByte4 << 4) & MOST_SIGNIFICANT_BIT) != 0);

                handleInputChange(ButtonRingZone.Select, ((currentInput[4] >> 1) & LEAST_SIGNIFICANT_BIT) != 0);
            }


            // coin -> byte 0 (00000001)
            if (_currentState[0] != currentInput[0])
            {
                handleInputChange(
                    ButtonRingZone.InsertCoin,
                    (currentInput[0] & LEAST_SIGNIFICANT_BIT) != 0
                );
            }

            _currentState = currentInput;

        }

        private void handleInputChange(ButtonRingZone zone, bool result)
        {
            _currentActiveStates.TryGetValue(zone, out var currentActiveState);
            if (result == currentActiveState)
            {
                return;
            }
            _inputSubscriptions.TryGetValue(zone, out var callback);
            var newState = currentActiveState ? InputState.Off : InputState.On;
            callback(zone, newState);
            _currentActiveStates[zone] = !currentActiveState;
        }


        // source: https://stackoverflow.com/a/48599119
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

    }


}