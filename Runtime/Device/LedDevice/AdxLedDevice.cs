using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;
using MychIO.Helper;
using UnityEngine;

namespace MychIO.Device
{
    public class AdxLedDevice : Device<LedInteractions, InputState, SerialDeviceProperties>
    {
        public override string Name
        {
            get => DEVICE_NAME;
        }
        public override bool CanRead
        {
            get => false;
        }
        public override bool CanWrite
        {
            get => true;
        }
        public const string DEVICE_NAME = "AdxLedDevice";

        // Settings for microoptimization
        public const int BYTES_TO_READ = 9;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.LedDevice;
        public static new string GetDeviceName() => DEVICE_NAME;
        public static new IConnectionProperties GetDefaultConnectionProperties() => new SerialDeviceProperties(
            comPortNumber: "COM21",
            writeTimeoutMS: SerialDeviceProperties.DEFAULT_WRITE_TIMEOUT_MS,
            bufferByteLength: 9,
            pollingRateMs: 10,
            portNumber: 0,
            baudRate: BaudRate.Bd115200,
            stopBit: StopBits.One,
            parityBit: Parity.None,
            dataBits: DataBits.Eight,
            handshake: Handshake.None,
            dtr: false,
            rts: false
        );
        public new static SerialDeviceProperties GetDefaultDeviceProperties() => (SerialDeviceProperties)GetDefaultConnectionProperties();
        // ** Connection Properties 
        private static readonly ReadOnlyMemory<byte> NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private byte[] _currentState = new byte[BYTES_TO_READ];
        //private byte[] _currentInput = new byte[BYTES_TO_READ];
        private IDictionary<LedInteractions, bool> _currentActiveStates;

        public static readonly IDictionary<LedCommand, byte[][]> Commands = new Dictionary<LedCommand, byte[][]>
        {
            {
                LedCommand.ClearAll, new byte[][]
                {
                    new byte[] {0xE0, 0x11, 0x01, 0x08, 0x32, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6C},
                    new byte[] {0xE0, 0x11, 0x01, 0x04, 0x39, 0x00, 0x00, 0x00, 0x4F},
                    new byte[] {0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F}
                }
            },
            {
                LedCommand.Update, new byte[][]
                {
                    new byte[] {0xE0, 0x11, 0x01, 0x01, 0x3C, 0x4F }
                }
            },
            {
                LedCommand.SetColorBA1, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA2, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA3, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA4, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA5, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA6, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA7, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColorBA8, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            }
        };

        public AdxLedDevice(
            IDictionary<Enum, Action<Enum, Enum>> inputSubscriptions,
            IDictionary<string, dynamic> connectionProperties = null,
            IOManager manager = null
        ) : base(inputSubscriptions, connectionProperties, manager)
        {
            NO_INPUT_PACKET.CopyTo(_currentState);
            // current states
            _currentActiveStates = new Dictionary<LedInteractions, bool>();
            foreach (LedInteractions zone in Enum.GetValues(typeof(LedInteractions)))
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
            // Establish connection with LED device
            foreach (var command in new byte[][]{
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23},
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23},
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23}
            }
            )
            {
                await _connection.WriteAsync(command);
            }
            await WriteAsync(LedCommand.ClearAll);

        }
        public override void OnDisconnected()
        {
            OnDisconnectedAsync().Wait();
        }
        public async override Task OnDisconnectedAsync()
        {
            await WriteAsync(LedCommand.ClearAll);
        }


        public override void ResetState()
        {
            NO_INPUT_PACKET.CopyTo(_currentState);
        }
        async Task SetColorAsync(Color newColor, int index)
        {
            var packet = Commands[(LedCommand)(2 + index)][0];
            packet[5] = (byte)index;
            packet[6] = (byte)(newColor.r * 255);
            packet[7] = (byte)(newColor.g * 255);
            packet[8] = (byte)(newColor.b * 255);
            packet[9] = CalculateCheckSum(packet.AsSpan().Slice(0, 9));

            await _connection.WriteAsync(packet);
        }
        byte CalculateCheckSum(Span<byte> bytes)
        {
            byte sum = 0;
            for (int i = 1; i < bytes.Length; i++)
            {
                sum += bytes[i];
            }
            return sum;
        }
        LedCommandInfo ParseCommand(ReadOnlyMemory<byte> buffer)
        {
            var bufferSpan = buffer.Span;
            var command = (LedCommand)bufferSpan[0];
            switch (command)
            {
                case LedCommand.SetColorBA1:
                case LedCommand.SetColorBA2:
                case LedCommand.SetColorBA3:
                case LedCommand.SetColorBA4:
                case LedCommand.SetColorBA5:
                case LedCommand.SetColorBA6:
                case LedCommand.SetColorBA7:
                case LedCommand.SetColorBA8:
                    var r = bufferSpan[1];
                    var g = bufferSpan[2];
                    var b = bufferSpan[3];
                    var newColor = new Color(r / 255, g / 255, b / 255);
                    return new((int)command - 2, command, newColor);
                default:
                    return new(-1, command, null);
            }
        }
        public override void Write<T>(params T[] interactions)
        {
            WriteAsync(interactions).Wait();
        }
        public override async Task WriteAsync<T>(params T[] interactions)
        {
            // data = [LedCommand, Red, Green, Blue]
            using (var owner = MemoryPool<byte>.Shared.Rent(4))
            {
                for (var i = 0; i < interactions.Length; i++)
                {
                    if (interactions[i] is null)
                        continue;

                    var value = UnsafeUtility.As<T, int>(ref interactions[i]);
                    var buffer = owner.Memory;
                    MemoryMarshal.Write(buffer.Span, ref value);
                    var cmdInfo = ParseCommand(buffer);
                    var command = cmdInfo.Command;

                    switch (command)
                    {
                        case LedCommand.SetColorBA1:
                        case LedCommand.SetColorBA2:
                        case LedCommand.SetColorBA3:
                        case LedCommand.SetColorBA4:
                        case LedCommand.SetColorBA5:
                        case LedCommand.SetColorBA6:
                        case LedCommand.SetColorBA7:
                        case LedCommand.SetColorBA8:
                            var newColor = (Color)cmdInfo.Color;
                            await SetColorAsync(newColor, cmdInfo.Index);
                            break;
                        default:
                            if (Commands.TryGetValue(command, out byte[][] bytes))
                            {
                                foreach (var _bytes in ArrayHelper.ToEnumerable(bytes))
                                {
                                    await _connection.WriteAsync(_bytes);
                                }
                            }
                            else
                            {
                                throw new ArgumentException("Command not found.", nameof(command));
                            }
                            break;
                    }
                }
            }
        }
        // Not used
        public override void ReadDataWithDebounce(IntPtr intPtr)
        {
            ThrowHelper.NotSupported();
        }
        public override void ReadDataWithDebounce(ReadOnlySpan<byte> data)
        {
            ThrowHelper.NotSupported();
        }
        public override void ReadData(IntPtr intPtr)
        {
            ThrowHelper.NotSupported();
        }
        public override void ReadData(ReadOnlySpan<byte> data)
        {
            ThrowHelper.NotSupported();
        }
        readonly struct LedCommandInfo
        {
            /// <summary>
            /// Indicates which LED the command is effective for. When the value is -1, it means that the command is effective for multiple devices.
            /// </summary>
            public int Index { get; }
            public LedCommand Command { get; }
            public Color? Color { get; }
            public LedCommandInfo(int index, LedCommand command, Color? color)
            {
                Index = index;
                Command = command;
                Color = color;
            }

        }
    }

}