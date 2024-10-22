using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.HidDevice;
using MychIO.Helper;

namespace MychIO.Device
{
    public class AdxIO4ButtonRing : Device<ButtonRingZone, InputState, HidDeviceProperties>
    {
        public const string DEVICE_NAME = "AdxIO4ButtonRing";
        // Rather hardcode it here for micro optimization
        public const int BUFFER_SIZE = 64;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.HID;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.TouchPanel;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new HidDeviceProperties(
            vendorId: 0x2e3c,
            productId: 0x5750,
            bufferSize: BUFFER_SIZE
        );
        // ** Connection Properties
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
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

        public unsafe override void ReadData(IntPtr data)
        {
            Span<byte> managedData = new Span<byte>(data.ToPointer(), BUFFER_SIZE);
            // If needed, convert to a managed array for further processing
            byte[] result = managedData.ToArray();
            _manager.handleEvent(Event.IOEventType.Debug, GetDeviceClassification(), HelperFunctions.ConvertByteArrayToBitString(result));
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