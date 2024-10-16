using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Device;
using System.Linq;
using UnityEngine.EventSystems;
using MychIO.Event;

namespace MychIO
{

    public class IOManager
    {

        protected IDictionary<DeviceClassification, IDevice> _deviceClassificationToDevice = new Dictionary<DeviceClassification, IDevice>();
        protected IDictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>> _deviceClassificationToDeviceInputAction = new Dictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>();
        protected IDictionary<IOEventType, ControllerEventDelegate> _eventTypeToCallback = new Dictionary<IOEventType, ControllerEventDelegate>();

        public IOManager() { }

        public async Task AddDeviceByName(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions = null,
            DeviceClassification deviceClassification = DeviceClassification.Undefined
        )
        {
            if (DeviceClassification.Undefined == deviceClassification)
            {
                deviceClassification = DeviceFactory.GetClassificationFromDeviceName(deviceName);
            }
            if (DeviceClassification.Undefined == deviceClassification)
            {
                throw new Exception("Invalid classification passed to function");
            }
            if (null != inputSubscriptions)
            {
                _deviceClassificationToDeviceInputAction.Add(deviceClassification, inputSubscriptions);
            }
            else if (_deviceClassificationToDeviceInputAction.TryGetValue(deviceClassification, out var setDeviceClassification))
            {
                inputSubscriptions = setDeviceClassification;
            }

            if (_deviceClassificationToDevice.TryGetValue(deviceClassification, out var oldDevice))
            {
                oldDevice.Disconnect();
            }

            var device = await DeviceFactory.GetDeviceAsync(
                deviceName,
                connectionProperties,
                inputSubscriptions,
                _deviceClassificationToDevice.Values.ToArray(),
                this
            );
            _deviceClassificationToDevice.Add(device.GetClassification(), device);
        }

        public async Task AddTouchPanel(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions = null
        )
        {
            await AddDeviceByName
            (
                deviceName,
                connectionProperties,
                ConvertDictionary(inputSubscriptions),
                DeviceClassification.TouchPanel
            );
        }

        public async Task AddButtonRing(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties = null,
            IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions = null
        )
        {
            await AddDeviceByName
            (
                deviceName,
                connectionProperties,
                ConvertDictionary(inputSubscriptions),
                DeviceClassification.ButtonRing
            );
        }

        private IDictionary<Enum, Action<Enum, Enum>> ConvertDictionary<T1, T2>(IDictionary<T1, Action<T1, T2>> dictionary) where T1 : Enum where T2 : Enum
        {
            var newDict = new Dictionary<Enum, Action<Enum, Enum>>();
            foreach (var kvp in dictionary)
            {
                newDict[kvp.Key] = (arg1, arg2) =>
                    kvp.Value((T1)arg1, (T2)arg2);
            }

            return newDict;
        }

        public async Task WriteToDevice(DeviceClassification deviceClassification, Enum[] command)
        {
            if (_deviceClassificationToDevice.TryGetValue(deviceClassification, out var device) && device.IsConnected())
            {
                await device.Write(command);
            }
        }

        public void Destroy()
        {
            foreach (var (_, device) in _deviceClassificationToDevice)
            {
                device.Disconnect();
            }
            _deviceClassificationToDevice = new Dictionary<DeviceClassification, IDevice>();
            _deviceClassificationToDeviceInputAction = new Dictionary<DeviceClassification, IDictionary<Enum, Action<Enum, Enum>>>();
            _eventTypeToCallback = new Dictionary<IOEventType, ControllerEventDelegate>();
        }

        // Events
        public void SubscribeToEvent(IOEventType eventType, ControllerEventDelegate callback)
        {
            _eventTypeToCallback[eventType] = callback;
        }

        public void SubscribeToEvents(IDictionary<IOEventType, ControllerEventDelegate> eventSubscriptions)
        {
            // clone to prevent side effects
            _eventTypeToCallback = new Dictionary<IOEventType, ControllerEventDelegate>(eventSubscriptions);
        }

        public void handleEvent(
            IOEventType eventType,
            DeviceClassification deviceType = DeviceClassification.Undefined,
            string message = "N/A"
         )
        {
            if (!_eventTypeToCallback.TryGetValue(eventType, out ControllerEventDelegate eventDelegate))
            {
                return;
            }
            eventDelegate(eventType, deviceType, message);
        }

    }

}