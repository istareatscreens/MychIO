using Unity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Device;
using System.IO.Ports;
using System.Linq;

namespace MychIO
{

    public delegate void InputEvent(Enum key, Enum value);

    public class IOManager
    {

        protected IDictionary<DeviceClassification, IDevice> _deviceClassificationToDevice = new Dictionary<DeviceClassification, IDevice>();
        protected IDictionary<DeviceClassification, IDictionary<Enum, InputEvent>> _deviceClassificationToDeviceAction = new Dictionary<DeviceClassification, IDictionary<Enum, InputEvent>>();

        public IOManager() { }

        public async Task<IOManager> AddDevice(
            string deviceName,
            IDictionary<string, dynamic> connectionProperties,
            IDictionary<Enum, InputEvent> inputSubscriptions = null
        )
        {
            var deviceClassification = DeviceFactory.GetClassificationFromDeviceName(deviceName);
            if (null != inputSubscriptions)
            {
                _deviceClassificationToDeviceAction.Add(deviceClassification, inputSubscriptions);
            }
            else if (_deviceClassificationToDeviceAction.TryGetValue(deviceClassification, out var setDeviceClassification))
            {
                inputSubscriptions = setDeviceClassification;
            }

            var device = await DeviceFactory.GetDeviceAsync(
                deviceName,
                connectionProperties,
                inputSubscriptions,
                _deviceClassificationToDevice.Values.ToArray()
            );
            _deviceClassificationToDevice.Add(device.GetClassification(), device);

            return this;
        }

        public void Destroy()
        {
            foreach (var (_, device) in _deviceClassificationToDevice)
            {
                device.Disconnect();
            }
        }

        /*
                public AdxControllerDesktopObservable(
                        IDictionary<TouchPanelZone, Action<TouchPanelZone, InteractionState>> touchPanelZoneToCallback,
                        IDictionary<ButtonRingZone, Action<ButtonRingZone, InteractionState>> buttonRingZoneToCallback,
                        IDictionary<ControllerEventType, ControllerEventDelegate> eventTypeToCallback,
                        IDecoder<TouchPanelZone> touchPanelDecoder = null,
                        IDecoder<ButtonRingZone> buttonRingDecoder = null
                    ) : base(
                            touchPanelZoneToCallback,
                            buttonRingZoneToCallback,
                            eventTypeToCallback,
                            touchPanelDecoder,
                            buttonRingDecoder
                        )
                { }

                public override void Destroy()
                {
                    foreach (var (_, device) in _deviceNameToDevice)
                    {
                        device.Detach();
                    }
                }

                internal override ISerialDevice GetSerialDevice<T>(
                    string deviceName,
                    AdxControllerDevice deviceType,
                    ConnectionProperties connectionProperties,
                    IDecoder<T> decoder
                 )
                {
                    return new DesktopSerialDevice<T>(
                             adxControllerConnector: this,
                             deviceName: deviceName,
                             deviceType: deviceType,
                             connectionProperties,
                             decoder
                         );
                }

                public override string[] GetUsbDeviceList()
                {
                    return SerialPort.GetPortNames();
                }


        #if UNITY_EDITOR
                public override void SubscribeToDebugEvents(Action<string> callback) { }
        #endif
            */

    }
}
