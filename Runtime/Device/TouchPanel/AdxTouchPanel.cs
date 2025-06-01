using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;
using MychIO.Helper;

namespace MychIO.Device
{
    public class AdxTouchPanel : Device<TouchPanelZone, InputState, AdxTouchPanelProperties>
    {

        /**
            // Byte 0 = (
            // Byte 1 
             0b00000001, TouchPanelZone.A1 
             0b00000010, TouchPanelZone.A2 
             0b00000100, TouchPanelZone.A3 
             0b00001000, TouchPanelZone.A4 
             0b00010000, TouchPanelZone.A5 

            // Byte 2 
             0b00000001, TouchPanelZone.A6 
             0b00000010, TouchPanelZone.A7 
             0b00000100, TouchPanelZone.A8 
             0b00001000, TouchPanelZone.B1 
             0b00010000, TouchPanelZone.B2 

            // Byte 3
             0b00000001, TouchPanelZone.B3 
             0b00000010, TouchPanelZone.B4 
             0b00000100, TouchPanelZone.B5 
             0b00001000, TouchPanelZone.B6 
             0b00010000, TouchPanelZone.B7 
            // Byte 4
             0b00000001, TouchPanelZone.B8 
             0b00000010, TouchPanelZone.C1 
             0b00000100, TouchPanelZone.C2 
             0b00001000, TouchPanelZone.D1 
             0b00010000, TouchPanelZone.D2 

            // Byte 5 (0x03) masks
             0b00000001, TouchPanelZone.D3 
             0b00000010, TouchPanelZone.D4 
             0b00000100, TouchPanelZone.D5 
             0b00001000, TouchPanelZone.D6 
             0b00010000, TouchPanelZone.D7 
            // Byte 6 (0x04) masks
             0b10000001, TouchPanelZone.D8 
             ...
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
            get => true;
        }
        public const string DEVICE_NAME = "AdxTouchPanel";

        // Settings for microoptimization
        public const int BYTES_TO_READ = 9;

        const int BIT_1ST_MASK = 0b00000001;
        const int BIT_2ND_MASK = 0b00000010;
        const int BIT_3RD_MASK = 0b00000100;
        const int BIT_4TH_MASK = 0b00001000;
        const int BIT_5TH_MASK = 0b00010000;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.TouchPanel;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new AdxTouchPanelProperties(
            comPortNumber: "COM3",
            writeTimeoutMS: SerialDeviceProperties.DEFAULT_WRITE_TIMEOUT_MS,
            bufferByteLength: 9,
            pollingRateMs: 10,
            portNumber: 0,
            baudRate: BaudRate.Bd9600,
            stopBit: StopBits.One,
            parityBit: Parity.None,
            dataBits: DataBits.Eight,
            handshake: Handshake.None,
            dtr: false,
            rts: false
        );
        public new static AdxTouchPanelProperties GetDefaultDeviceProperties() => (AdxTouchPanelProperties)GetDefaultConnectionProperties();

        // ** Connection Properties 

        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private byte[] _currentState = NO_INPUT_PACKET;
        //private byte[] _currentInput = new byte[BYTES_TO_READ];
        private IDictionary<TouchPanelZone, bool> _currentActiveStates;
        readonly DebounceCallbackHandler<TouchPanelZone, byte, byte> _debounceCallbackHandler;

        public static readonly IDictionary<TouchPanelCommand, byte[][]> Commands = new Dictionary<TouchPanelCommand, byte[][]>
        {
            { TouchPanelCommand.Start, new byte[][] { new byte[] { 0x7B, 0x53, 0x54, 0x41, 0x54, 0x7D } } },
            { TouchPanelCommand.Reset, new byte[][] { new byte[] { 0x7B, 0x52, 0x53, 0x45, 0x54, 0x7D } } },
            { TouchPanelCommand.Halt, new byte[][] { new byte[] { 0x7B, 0x48, 0x41, 0x4C, 0x54, 0x7D } } },
        };

        public AdxTouchPanel(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            _debounceCallbackHandler = HandleInputChangeInternal;
            // current states
            _currentActiveStates = new Dictionary<TouchPanelZone, bool>();
            foreach (TouchPanelZone zone in Enum.GetValues(typeof(TouchPanelZone)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override void OnConnected()
        {
            OnConnectedAsync().Wait();
        }

        public override async Task OnConnectedAsync()
        {
            await WriteAsync(TouchPanelCommand.Reset, TouchPanelCommand.Halt);
            // Calibration
            for (byte a = 0x41; a <= 0x62; a++)
            {
                await _connection.WriteAsync(Encoding.UTF8.GetBytes("{L" + (char)a + "r2}"));
            }
            dynamic sens = 0;
            var connProperties = _connectionProperties.Properties;
            var sensitivityOverride = connProperties.TryGetValue("SensitivityOverride", out var _sensitivityOverride) &&
                                      connProperties.TryGetValue("Sensitivity", out sens) && _sensitivityOverride;
            if (sensitivityOverride)
            {
                try
                {
                    for (byte a = 0x41; a <= 0x62; a++)
                    {
                        var value = GetSensitivityValue(a, sens);
                        await _connection.WriteAsync(Encoding.UTF8.GetBytes($"{{{"L"}{(char)a}k{(char)value}}}"));
                    }
                }
                catch (Exception e)
                {
                    _manager.handleEvent(Event.IOEventType.Debug,
                                         DeviceClassification.TouchPanel,
                                         $"An error occurred while setting sensitivity:\n{e}");
                }
            }
            await WriteAsync(TouchPanelCommand.Start);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void ReadData(ReadOnlySpan<byte> currentInput)
        {
            // ensure buffer is aligned
            var headIndexs = GetPacketHeadIndexs(stackalloc int[currentInput.Length], currentInput);

            if (headIndexs.IsEmpty)
            {
                return;
            }
            for (var i = 0; i < headIndexs.Length; i++)
            {
                var headIndex = headIndexs[i];
                if (headIndex + BYTES_TO_READ > currentInput.Length)
                {
                    return;
                }
                var packet = currentInput.Slice(headIndexs[i], BYTES_TO_READ);
                var tail = packet[BYTES_TO_READ - 1];
                if (')' != tail)
                {
                    continue;
                }

                for (var j = 1; j < 8; j++)
                {
                    var @byte = packet[j];
                    if (@byte == _currentState[j])
                    {
                        continue;
                    }
                    HandleInputChangeInternal((TouchPanelZone)(0 + ((j - 1) * 5)), @byte, BIT_1ST_MASK);
                    HandleInputChangeInternal((TouchPanelZone)(1 + ((j - 1) * 5)), @byte, BIT_2ND_MASK);
                    HandleInputChangeInternal((TouchPanelZone)(2 + ((j - 1) * 5)), @byte, BIT_3RD_MASK);
                    HandleInputChangeInternal((TouchPanelZone)(3 + ((j - 1) * 5)), @byte, BIT_4TH_MASK);
                    HandleInputChangeInternal((TouchPanelZone)(4 + ((j - 1) * 5)), @byte, BIT_5TH_MASK);
                }
                packet.CopyTo(_currentState);
            }
        }
        public override void ReadDataWithDebounce(ReadOnlySpan<byte> currentInput)
        {
            // ensure buffer is aligned
            var headIndexs = GetPacketHeadIndexs(stackalloc int[currentInput.Length], currentInput);

            if (headIndexs.IsEmpty)
            {
                return;
            }
            for (var i = 0; i < headIndexs.Length; i++)
            {
                var headIndex = headIndexs[i];
                if (headIndex + BYTES_TO_READ > currentInput.Length)
                {
                    return;
                }
                var packet = currentInput.Slice(headIndexs[i], BYTES_TO_READ);
                var tail = packet[BYTES_TO_READ - 1];
                if (')' != tail)
                {
                    continue;
                }

                for (var j = 1; j < 8; j++)
                {
                    var @byte = packet[j];
                    if (@byte == _currentState[j])
                    {
                        continue;
                    }

                    var zone1 = (TouchPanelZone)(0 + ((j - 1) * 5));
                    var zone2 = (TouchPanelZone)(1 + ((j - 1) * 5));
                    var zone3 = (TouchPanelZone)(2 + ((j - 1) * 5));
                    var zone4 = (TouchPanelZone)(3 + ((j - 1) * 5));
                    var zone5 = (TouchPanelZone)(4 + ((j - 1) * 5));

                    DebounceHandle<TouchPanelZone, byte, byte>(zone1, _debounceCallbackHandler, zone1, @byte, BIT_1ST_MASK);
                    DebounceHandle<TouchPanelZone, byte, byte>(zone2, _debounceCallbackHandler, zone2, @byte, BIT_2ND_MASK);
                    DebounceHandle<TouchPanelZone, byte, byte>(zone3, _debounceCallbackHandler, zone3, @byte, BIT_3RD_MASK);
                    DebounceHandle<TouchPanelZone, byte, byte>(zone4, _debounceCallbackHandler, zone4, @byte, BIT_4TH_MASK);
                    DebounceHandle<TouchPanelZone, byte, byte>(zone5, _debounceCallbackHandler, zone5, @byte, BIT_5TH_MASK);
                }
                packet.CopyTo(_currentState);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HandleInputChangeInternal(TouchPanelZone zone, byte input, byte mask)
        {
            // TODO: Remove this check this should not be happening its inefficient 
            if (zone > TouchPanelZone.E8 || zone < TouchPanelZone.A1)
            {
                return false;
            }

            var oldState = _currentActiveStates[zone];
            var newState = (input & mask) != 0;
            var isChanged = oldState != newState;

            if (isChanged)
            {
                var callback = _inputSubscriptions[zone];
                callback(zone,
                         newState ? InputState.On : InputState.Off);
                _currentActiveStates[zone] = newState;
            }

            return isChanged;
        }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }

        public override void Write<T>(params T[] interactions)
        {
            WriteAsync(interactions).Wait();
        }

        public override async Task WriteAsync<T>(params T[] interactions)
        {
            var commandBytes = interactions.OfType<TouchPanelCommand>()
            .SelectMany(command =>
            {
                if (Commands.TryGetValue(command, out byte[][] bytes))
                {
                    return bytes;
                }
                else
                {
                    throw new ArgumentException("Command not found.", nameof(command));
                }
            }).ToArray();

            foreach (var command in commandBytes)
            {
                await _connection.WriteAsync(command);
            }
        }

        ReadOnlySpan<int> GetPacketHeadIndexs(Span<int> buffer, ReadOnlySpan<byte> packet)
        {
            if (buffer.Length < packet.Length)
            {
                throw new ArgumentException();
            }
            int x = -1;
            for (var y = 0; y < packet.Length; y++)
            {
                var @byte = packet[y];
                if ('(' == @byte)
                {
                    buffer[++x] = y;
                }
            }
            if (x == -1)
                return ReadOnlySpan<int>.Empty;
            return buffer.Slice(0, x + 1);
        }
        // Not used
        public override void ReadData(IntPtr intPtr)
        {
            ThrowHelper.NotImplemented();
        }
        public override void ReadDataWithDebounce(IntPtr intPtr)
        {
            ThrowHelper.NotImplemented();
        }

        public override void OnDisconnected()
        {
            return;
        }

        public override Task OnDisconnectedAsync()
        {
            return Task.CompletedTask;
        }
        byte GetSensitivityValue(byte sensor, int sens)
        {
            if (sensor > 0x62 || sensor < 0x41)
                return 0x28;
            if (sensor < 0x49)
            {
                return sens switch
                {
                    -5 => 0x5A,
                    -4 => 0x50,
                    -3 => 0x46,
                    -2 => 0x3C,
                    -1 => 0x32,
                    1 => 0x1E,
                    2 => 0x1A,
                    3 => 0x17,
                    4 => 0x14,
                    5 => 0x0A,
                    _ => 0x28
                };
            }
            else
            {
                return sens switch
                {
                    -5 => 0x46,
                    -4 => 0x3C,
                    -3 => 0x32,
                    -2 => 0x28,
                    -1 => 0x1E,
                    1 => 0x14,
                    2 => 0x0F,
                    3 => 0x0A,
                    4 => 0x05,
                    5 => 0x01,
                    _ => 0x01
                };
            }
        }
#if UNITY_EDITOR
        public static string formatAdxTouchPanelOutput(byte[] data)
        {
            return Helper.HelperFunctions.BytesToString(new byte[] { data[0] })
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[1])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[2])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[3])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[4])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[5])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[6])
              + " "
              + Helper.HelperFunctions.ByteToBitString(data[7])
              + " "
              + Helper.HelperFunctions.BytesToString(new byte[] { data[8] })
              ;
        }
#endif
    }
}