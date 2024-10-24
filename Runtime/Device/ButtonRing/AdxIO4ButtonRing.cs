using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;
using MychIO.Helper;
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

                   11010000 110100011 101001011 0100111 1010100
            (coin) chane on byte 0
            (BA8 - BA4, up, select) change on byte 4
            (BA3 - BA1, down) change on byte 3

            BA1 -      00000000 00000010 00000000 00001001 11111000
            BA2 -      00000000 00000010 00000000 00000101 11111000
            no input - 00000000 00000010 00000000 00001101 11111000

            // bit shift right -> off
            BA3  -> byte 3, position 0 OFF
            up  -> byte 3, position 1 ON
            BA1 -> byte 3, position 2 OFF
            BA2 -> byte 3, position 3 OFF
            // bit shift left -> on (new number)
            down -> byte 3, position 1 ON

            // bit shift left -> off
            BA4  -> byte 4, position 0 OFF
            BA5  -> byte 4, position 1 OFF
            BA6  -> byte 4, position 2 OFF
            BA7  -> byte 4, position 3 OFF
            BA8  -> byte 4, position 4 OFF
            Select -> byte 4, position 6 ON


            coin -> byte 0 (00000001 && 00000001)
        */

        public const string DEVICE_NAME = "AdxIO4ButtonRing";
        // Rather hardcode it here for micro optimization if you need different 
        // settings just copy this class and change these values
        public const int BUFFER_SIZE = 64;
        public const int LEFT_BYTES_TO_TRUNCATE = 26;
        public const int BYTES_TO_READ = 5;

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
        private int[] _currentActiveStates = new int[12];

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

            /** UNSAFE CODE */
            /*
                if the code below causes any crashes or issues it might be better to 
                change this function to safe and copy the bytes this way:

                byte[] byteArray = new byte[BYTES_TO_READ];

                Marshal.Copy(pointer, byteArray, 0, BYTES_TO_READ);
            */
            if (pointer == IntPtr.Zero)
            {
                return;
            }

            // Create a byte array to hold the data
            byte[] currentInput = new byte[BYTES_TO_READ];

            // Use an unsafe block to read data directly from the unmanaged pointer
            byte* pByte = (byte*)pointer;

            // Ensure you do not exceed the size of byteArray
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

            Debug.Log(
                HelperFunctions.ConvertByteArrayToBitString(currentInput)
            );

            _currentState = currentInput;
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