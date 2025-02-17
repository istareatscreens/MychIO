using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.TouchPanelDevice;

namespace MychIO.Device.TouchPanel
{
    public class TouchPanelTouchPanel : Device<TouchPanelZone, InputState, TouchPanelDeviceProperties>
    {

        public const string DEVICE_NAME = "TouchPanel";

        private const int MAX_TOUCH_POINTS = 10;
        private const int DATA_POINTS = 3;

        private const int SHORT_SIZE = 16;


        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.TouchPanelDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.TouchPanel;
        public static new string GetDeviceName() => DEVICE_NAME;
        public override string DeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new TouchPanelDeviceProperties(
            pollingRateMs: 2
        );
        public new static TouchPanelDeviceProperties GetDefaultDeviceProperties() => (TouchPanelDeviceProperties)GetDefaultConnectionProperties();
        // ** Connection Properties
        private static readonly short[] NO_INPUT_PACKET = new short[MAX_TOUCH_POINTS * DATA_POINTS];
        private short[] _currentState = NO_INPUT_PACKET;
        private IDictionary<TouchPanelZone, bool> _currentActiveStates;
        public static readonly IDictionary<TouchPanelCommand, byte[]> Commands = new Dictionary<TouchPanelCommand, byte[]> { };

        public TouchPanelTouchPanel(
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

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

        public unsafe override void ReadDataDebounce(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
            {
                return;
            }
            byte[] rawInput = new byte[MAX_TOUCH_POINTS * DATA_POINTS * SHORT_SIZE];
            byte* pByte = (byte*)pointer;
            for (int i = 0; i < MAX_TOUCH_POINTS * DATA_POINTS; i++)
            {
                rawInput[i] = *(pByte + i);
            }

            short[] currentInput = new short[MAX_TOUCH_POINTS * DATA_POINTS];
            Buffer.BlockCopy(rawInput, 0, currentInput, 0, currentInput.Length);

            // TODO: Implement

        }

        public unsafe override void ReadData(IntPtr pointer)
        {

            if (pointer == IntPtr.Zero)
            {
                return;
            }
            byte[] rawInput = new byte[MAX_TOUCH_POINTS * DATA_POINTS * SHORT_SIZE];
            byte* pByte = (byte*)pointer;
            for (int i = 0; i < MAX_TOUCH_POINTS * DATA_POINTS; i++)
            {
                rawInput[i] = *(pByte + i);
            }

            short[] currentInput = new short[MAX_TOUCH_POINTS * DATA_POINTS];
            Buffer.BlockCopy(rawInput, 0, currentInput, 0, currentInput.Length);

            // TODO: Implement
            // UnityEngine.Debug.Log(GetTouchEventsString(currentInput));

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void handleInputChange(TouchPanelZone zone, Tuple<short, short> coordinates, short state)
        {
            _currentActiveStates.TryGetValue(zone, out var currentActiveState);
            // TODO: Implement
        }

#if UNITY_EDITOR
        public static string GetTouchEventsString(short[] touchArray)
        {
            // StringBuilder to accumulate the output string
            var result = new System.Text.StringBuilder();

            if (touchArray.Length % 3 != 0)
            {
                result.AppendLine("Invalid array length. The array must be divisible by 3.");
                return result.ToString();
            }

            for (int i = 0; i < touchArray.Length; i += 3)
            {
                short x = touchArray[i];
                short y = touchArray[i + 1];
                short eventValue = touchArray[i + 2];

                // Get the name of the event, or "undefined" if it's not recognized
                string eventName = Enum.IsDefined(typeof(WindowsTouchEvent), eventValue)
                                   ? Enum.GetName(typeof(WindowsTouchEvent), eventValue)
                                   : "undefined";

                // Append the formatted output
                result.AppendLine($"({x}, {y}), {eventName}");
            }

            return result.ToString();
        }
#endif

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