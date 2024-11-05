using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.Keyboard;
using MychIO.Connection.SerialDevice;
using UnityEngine;

namespace MychIO.Device
{
    public class KeyboardDevice : Device<LedInteractions, InputState, SerialDeviceProperties>
    {
        public const string DEVICE_NAME = "Keyboard";

        // Class specific constants
        private const int NO_INPUT_PACKET = 0;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.Keyboard;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.Keyboard;
        public static new string GetDeviceName() => DEVICE_NAME;
        public override string DeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new KeyboardProperties(
            mapping: (new Dictionary<Enum, Enum>() { { TouchPanelZone.B1, TouchPanelZone.A1 } })
        );
        private int _currentState = NO_INPUT_PACKET;

        // ** Connection Properties 
        private IDictionary<LedInteractions, bool> _currentActiveStates;

        public static readonly IDictionary<LedCommand, byte[][]> Commands = new Dictionary<LedCommand, byte[][]>
        {
        };

        public KeyboardDevice(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            // current states
            _currentActiveStates = new Dictionary<LedInteractions, bool>();
            /*
            foreach (LedInteractions zone in Enum.GetValues(typeof(LedInteractions)))
            {
                _currentActiveStates[zone] = false;
            }
            */
        }

        public override void ReadData(string data)
        {
            _manager.handleEvent(Event.IOEventType.Debug, DeviceClassification.Keyboard, $"{data}");
            if (_currentState == 0)
            {
                return;
            }
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

        // Not used
        public override Task Write(params Enum[] interactions)
        {
            throw new NotImplementedException();
        }
        public override Task OnStartWrite()
        {
            return Task.CompletedTask;
        }
        public override void ReadData(IntPtr intPtr)
        {
            throw new NotImplementedException();
        }
        public override void ReadData(byte[] data)
        {
            throw new NotImplementedException();
        }
        public override Task OnDisconnectWrite()
        {
            throw new NotImplementedException();
        }

    }

}