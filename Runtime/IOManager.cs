using Unity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Device;
using System.IO.Ports;

namespace MychIO
{
    public class IOManager
    {

/*
        protected IDictionary<DeviceClassification,> _deviceNameToDevice = new Dictionary<string, ISerialDevice>();
        protected IDictionary<AdxControllerDevice, ISerialDevice> _deviceTypeToDevice = new Dictionary<AdxControllerDevice, ISerialDevice>();
        protected IDictionary<ControllerEventType, ControllerEventDelegate> _eventTypeToCallback = new Dictionary<ControllerEventType, ControllerEventDelegate>();

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
