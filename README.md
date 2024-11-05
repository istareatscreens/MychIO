# MychIO

Input manager that supports Mai2 controllers in Unity.

# Adding to Unity

To add this package to your project simply copy the clone link and go to Window->Package Manager. Hit the + in the window and select add package from git Url.

# API

Full example of using the package is viewable [ here ](https://github.com/istareatscreens/MychIODev/blob/master/Assets/Scripts/TestBoard.cs)

## Initialization

Add the following imports to your project

```C#
using MychIO.Device;
using MychIO;
using MychIO.Event;
```

Retrieve the ExecutionQueue (type `ConcurrentQueue<Action>`) from the IOManager and Instantiate it

```C#
_executionQueue = IOManager.ExecutionQueue;
_ioManager = new IOManager();
```

The executionQueue is used to store callbacks that will be executed on the main thread. Since this input manager uses threading you must execute all code in the context of the ExecutionQueue. Otherwise undefined/undesirable behaviour will likely be observed.

## Event System

To subscribe to error events use IOEventType and generate a Dictionary of callbacks:

```C#
var eventCallbacks = new Dictionary<IOEventType, ControllerEventDelegate>{
            { IOEventType.Attach,
                (eventType, deviceType, message) =>
                {
                    # Note: appendEventText uses the IOManagers ExecutionQueue
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.ConnectionError,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.Debug,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.Detach,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            },
            { IOEventType.SerialDeviceReadError,
                (eventType, deviceType, message) =>
                {
                    appendEventText($"eventType: {eventType} type: {deviceType} message: {message.Trim()}");
                }
            }
        };
```

Then pass the callbacks into the instantiated IOManager

```C#
    _ioManager.SubscribeToEvents(eventCallbacks);
```

Using this method:

```C#
public void SubscribeToEvents(IDictionary<IOEventType, ControllerEventDelegate> eventSubscriptions)
```

Or to subscribe to an individual event or update the subscription:

```C#
public void SubscribeToEvent(IOEventType eventType, ControllerEventDelegate callback)
```

Alternatively you can subscribe to all events with a single callback using the following method:

```C#
public void SubscribeToAllEvents(ControllerEventDelegate callback)
```

### Handling all fallback events

As an alternative to callbacks a hook class can be passed to handle events instead of callbacks directly. Implement the interface MychIO.IDeviceErrorHandler and pass it to the following method on IOManager:

```C#
public void AddDeviceErrorHandler(IDeviceErrorHandler errorHandler)
```

### Types of events

Event types are represented as Enums in the MychIO.Eveent class under IOEventType

| Event Enum Name         | Information                                            |
| ----------------------- | ------------------------------------------------------ |
| `Attach`                | `Sent on controlled device connect`                    |
| `Detach`                | `Sent on controlled device disconnect`                 |
| `ConnectionError`       | `Sent on failure to establsh connecton`                |
| `SerialDeviceReadError` | `Sent on failure of read loop for serial port`         |
| `HidDeviceReadError`    | `Sent on failure of read  loop for hid device`         |
| `ReconnectionError`     | `Sent on exception thrown during Reconnection attempt` |

## Connecting To Devices

Currently this system supports three device types specified in `MychIO.Device.DeviceClassification`.

Each device type has specified interaction zones (type Enums):

Interaction Zones:

- `MychIO.Device.TouchPanelZone`
- `MychIO.Device.ButtonRingZone`

Defined State (type Enum) of these zones:

- `MychIO.Device.InputState`

Using these enums we can construct callbacks that are executed only when a specific zones input state changes. Doing this we can specify any specific logic we want to trigger when these states are changed.

An example of generating these callbacks:

```C#
var touchPanelCallbacks = new Dictionary<TouchPanelZone, Action<TouchPanelZone, InputState>>();
foreach (TouchPanelZone touchZone in System.Enum.GetValues(typeof(TouchPanelZone)))
{
    // _touchIndicatorMap is a map of TouchPanelZone => GameObject
    if (!_touchIndicatorMap.TryGetValue(touchZone, out var touchIndicator))
    {
        throw new Exception($"Failed to find GameObject for {touchZone}");
    }

    touchPanelCallbacks[touchZone] = (TouchPanelZone input, InputState state) =>
    {
        _executionQueue.Enqueue(() =>
        {
            // In this execution queue callback we any changes we need to (This will happen on the MainThread)
            touchIndicator.SetActive(state == InputState.On);
        });
    };
}
```

You can connect to the three supported devices (TouchPanel, ButtonRing and LedDevice) using the following functions as follows:

```C#
_ioManager
    .AddTouchPanel(
        AdxTouchPanel.GetDeviceName(),
        inputSubscriptions: touchPanelCallbacks
    );
```

```C#
_ioManager.AddButtonRing(
    AdxIO4ButtonRing.GetDeviceName(),
    inputSubscriptions: buttonRingCallbacks
);
```

```C#
_ioManager.AddLedDevice(
   AdxLedDevice.GetDeviceName()
);
```

Method Signatures for these methods are as follows:

```C#
public void AddTouchPanel(
    string deviceName,
    IDictionary<string, dynamic> connectionProperties = null,
    IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions = null
)
```

```C#
public void AddButtonRing(
    string deviceName,
    IDictionary<string, dynamic> connectionProperties = null,
    IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions = null
)
```

```C#
public void AddLedDevice(
    string deviceName,
    IDictionary<string, dynamic> connectionProperties = null
)
```

Where,

- deviceName is the unique name of the device e.g. (AdxButtonRing)
- connectionProperties - stores the properties specific to their connection interface. These can be used to overwrite the default device connection properties. These properties implement the `MychIO.Connection.IConnectionProperties` interface and can be easily serialized/unserialized using the instantiated IConnection class (IDictionary<string, dynamic> <==> concrete IConnection object)
- inputSubscriptions - Callbacks that are triggered by controller interaction mapped by device interaction zone enum

## Adding Custom Connection Properties to A Device

To add connection properties to a device you instantiate a new ConnectionProperties class specific to the device you would like to attach. Then instantiate an appropriate IConnnectionProperties class and use its copy constructor. You can call the GetDefaultConnectionProperties method on the specific device you want to generate a properties object for and then pass whatever other properties you wish to change.
Then call the `GetProperties` method on this properties object to serialize the properties into a type agnostic dictionary.

```C#
        var propertiesTouchPanel = new SerialDeviceProperties(
            (SerialDeviceProperties)AdxTouchPanel.GetDefaultConnectionProperties(),
            comPortNumber: "COM10"
        ).GetProperties();

        _ioManager
            .AddTouchPanel(
                AdxTouchPanel.GetDeviceName(),
                propertiesTouchPanel,
                inputSubscriptions: touchPanelCallbacks
        );
```

The current implemented devices have the following connection objects:

| Device Concrete Class | ConnectionProperties Object |
| --------------------- | --------------------------- |
| `AdxTouchPanel`       | `SerialDeviceProperties`    |
| `AdxLedDevice`        | `SerialDeviceProperties`    |
| `AdxIO4ButtonRing`    | `HidDeviceProperties`       |
| `AdxHIDButtonRing`    | `HidDeviceProperties`       |

## Writing to Devices

Once the connection to a device is established, send commands to execute specific functions using the follow IOManager method:

```C#
public async Task WriteToDevice(DeviceClassification deviceClassification, params Enum[] command)
```

Where,

- deviceClassification is the classification of the device see `MychIO.Device.DeviceClassification`
- command is a variadic parameter that takes one or more Enum commands specified in the following Enums
  - `MychIO.Device.ButtonRingCommand`
  - `MychIO.Device.TouchPanelCommand`
  - `MychIO.Device.LedDeviceCommand`

## Changing Interaction Subscriptions

Controller input needs to be dynamic based on scene (i.e. menu versus in game) callbacks can be changed and reloaded using the following functions.

### Adding/Replacing Input (Subscription) callbacks

```C#
public void AddTouchPanelInputSubscriptions(
    IDictionary<TouchPanelZone, Action<TouchPanelZone, InputState>> inputSubscriptions,
    string tag
)

public void AddButtonRingInputSubscriptions(
    IDictionary<ButtonRingZone, Action<ButtonRingZone, InputState>> inputSubscriptions,
    string tag
)
```

Where,

- inputSubscriptions are a list of callbacks mapped to input zone
- tag is a unique identifier specifying which callbacks to load. When you first connect a device its inputSubscriptions are saved under the tag `MychIO.IOManager.STANDARD_INPUT`(type: string)

_Important Note_: this will only update the internal state of the IOManager itself, if you overwrite a tag that already exists and is currently loaded it will not change what the device is currently using for inputSubscriptions. You must call the Change methods. This is intentional as to prevent side effects.

### Switch Input (Subscription) callbacks

To change what callbacks your zones are subscribed to (i.e. update a devices subscription callbacks) you can call the following functions. Internally this will halt reading on the device, then replace the callbacks then start reading again:

```C#
public void ChangeTouchPanelInputSubscriptions(string tag)
```

```C#
public void ChangeButtonRingInputSubscriptions(string tag)
```

Where,

- tag specifies a Dictionary of InputSubscriptions if a tag does not exist it will throw an exception

## Executing input callbacks

To execute the Subscription callbacks simply add the following to your Update() method in unity:

```C#
    while (_executionQueue.TryDequeue(out var action))
    {
        action();
    }
```

This will execute all callbacks passed to the queue every frame

## Fallback/Status methods

**Warning: these methods introduce a lot of overhead (particularly for HID devices) and should only be called in emergency situations**

In case of failure you can use the following methods to determine a devices status or attempt to restore the connection/read loop:

### IsReading

```C#
public bool IsReading(DeviceClassification deviceClassification)
```

- True if the device is currently being read from (no action needed).
- False if it is not running the reading loop from the device. You should call StartReading method to start reading again. It can also indicate that the device is not connected

### StartReading

Attempts to restart the read loop for the connected device (If device is not connected this will fail and return false)

```C#
public bool StartReading(DeviceClassification deviceClassification)
```

- True if the device has started the reading loop succesfully
- False if the device could not re-establish the device reading loop (Should call ReConnect)

### IsConnected

Checks if device port is open and can start reading

```C#
public bool IsConnected(DeviceClassification deviceClassification)
```

- True if device is connected and not ready to read
- False if device is not connected and not ready to read

### Reconnect

If called this will recreate the device as if you are calling `AddTouchDevice`, `AddButtonRing`, etc methods, it will also start reading automatically so no need to call StartReading(). Only to be used as a last ditch effort before just Destorying and recreating the IOManager. If there is a hard exception in this method it will throw a IOEventType.ReconnectionError Event

```C#
public bool ReConnect(DeviceClassification deviceClassification)
```

- True if device reconnected and established reading successfully
- False if device failed to reconnect, should attempt reconnecting manually. This is also a strong indication of failure of the IOManager to reestablish connection to the device. Given the device was previously connected.

# Testing and Development

For integration of other controllers, testing, contributing there exists a unity development project that can be cloned [here](https://github.com/istareatscreens/MychIODev).

## Adding a Device

To integrate a Serial or HID device create a class file in ButtonRing, LedDevice or TouchPanel and inheriting from abstract class `MychIO.Device.Device<T1, T2, T3>`, where:

- T1 : Enum - device Interactions e.g. LedInteractions, TouchPanelZone, ButtonRingZone (what you expect to be triggered by your device)
- T2: Enum - State change caused by an interaction e.g. InputState (On/Off) in most cases
- T3 : Class - [MychIO.Connection.ConnectionProperties](https://github.com/istareatscreens/MychIO/blob/master/Runtime/Connection/ConnectionProperties.cs) class e.g. SerialDeviceProperties, HidDeviceProperties

This abstract class is split into two partial classes ([BuildProperties](https://github.com/istareatscreens/MychIO/blob/master/Runtime/Device/Device.BuildProperties.cs) and [Base](https://github.com/istareatscreens/MychIO/blob/master/Runtime/Device/Device.Base.cs)). You must override all static methods located in Device.BuildProperties for proper instantiation of the device to occur.

After creation of your custom controller interface class simply go to DeviceFactory and add your deviceName to the following Dictionary to facilitate loading it using IOManager:

```C#
private static Dictionary<string, Type> _deviceNameToType = new()
{
    { AdxTouchPanel.GetDeviceName(), typeof(AdxTouchPanel) },
    { AdxIO4ButtonRing.GetDeviceName(), typeof(AdxIO4ButtonRing) },
    { AdxHIDButtonRing.GetDeviceName(), typeof(AdxHIDButtonRing) },
    { AdxLedDevice.GetDeviceName(), typeof(AdxLedDevice) },
    // Add other devices here...
};
```

## Adding a Connection

To create a new connection class inherit from the abstract class MychIO.Connection.Connection. The abstract Connection class is split into two partial classes ([Base](https://github.com/istareatscreens/MychIO/blob/master/Runtime/Connection/Connection.Base.cs) and [BuildProperties](https://github.com/istareatscreens/MychIO/blob/master/Runtime/Connection/Connection.BuildProperties.cs)). Note all static methods in the partial class BuildProperties must be overridden in the child class for proper instantiation of it.

After creation of your custom connection class you must add your class to the MychIO.Connection.ConnectionFactory dictionary:

```C#
private static Dictionary<ConnectionType, Type> _connectionTypeToConnection = new()
{
    { ConnectionType.HID, typeof(HidDeviceConnection) },
    { ConnectionType.SerialDevice, typeof(SerialDeviceConnection) }
    // Add other connections here...
};
```

## Adding a Device Command

Each Device type has a Enum that specifies specific Commands:

- `LedDeviceCommand`
- `ButtonRingCommand`
- `TouchPanelCommand`

These commands are setup in their respected device classes and can be made as complex as required for example `TouchPanelCommands` are placed in the concerete class as follows:

```C#
public static readonly IDictionary<TouchPanelCommand, byte[][]> Commands = new Dictionary<TouchPanelCommand, byte[][]>
{
    { TouchPanelCommand.Start, new byte[][] { new byte[] { 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } } },
    { TouchPanelCommand.Reset, new byte[][] { new byte[] { 0x7B, 0x52, 0x53, 0x45, 0x54, 0x7D } } },
    { TouchPanelCommand.Halt, new byte[][] { new byte[] { 0x7B, 0x48, 0x41, 0x4C, 0x54, 0x7D } } },
};
```
