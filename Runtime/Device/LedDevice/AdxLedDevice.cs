using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Connection.SerialDevice;
using MychIO.Helper;
using UnityEngine;

namespace MychIO.Device
{
    public class AdxLedDevice : Device<LedInteractions, InputState, SerialDeviceProperties>
    {
        public const string DEVICE_NAME = "AdxLedDevice";

        // Settings for microoptimization
        public const int BYTES_TO_READ = 9;

        // ** Connection Properties -- Required by factory: 
        public static new ConnectionType GetConnectionType() => ConnectionType.SerialDevice;
        public static new DeviceClassification GetDeviceClassification() => DeviceClassification.LedDevice;
        public static new string GetDeviceName() => DEVICE_NAME;
        public override string DeviceName() => DEVICE_NAME;
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
        private static readonly byte[] NO_INPUT_PACKET = new byte[]
        {
            0x28, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x29
        };

        private byte[] _currentState = NO_INPUT_PACKET;
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
            // current states
            _currentActiveStates = new Dictionary<LedInteractions, bool>();
            foreach (LedInteractions zone in Enum.GetValues(typeof(LedInteractions)))
            {
                _currentActiveStates[zone] = false;
            }
        }

        public override async Task OnStartWrite()
        {
            // Establish connection with LED device
            foreach (var command in new byte[][]{
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23},
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23},
                new byte[]{0xE0, 0x11, 0x01, 0x01, 0x10, 0x23}
            }
            )
            {
                await _connection.Write(command);
            }
            await Write(LedCommand.ClearAll);

        }

        public async override Task OnDisconnectWrite()
        {
            await Write(LedCommand.ClearAll);
        }

        public override void ReadData(byte[] data) { }
        public override void ReadDataDebounce(byte[] data) { }

        public override void ResetState()
        {
            _currentState = NO_INPUT_PACKET;
        }
        async Task SetColorAsync(Color newColor,int index)
        {
            var packet = Commands[(LedCommand)(2 + index)][0];
            packet[5] = (byte)index;
            packet[6] = (byte)(newColor.r * 255);
            packet[7] = (byte)(newColor.g * 255);
            packet[8] = (byte)(newColor.b * 255);
            packet[9] = CalculateCheckSum(packet.AsSpan().Slice(0, 9));

            await _connection.Write(packet);
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
                    return new ((int)command - 2,command, newColor);
                default:
                    return new(-1, command, null);
            }
        }
        public override async Task Write<T>(params T[] interactions)
        {
            // data = [LedCommand, Red, Green, Blue]
            using (var owner = MemoryPool<byte>.Shared.Rent(4))
            {
                for (var i = 0; i < interactions.Length; i++)
                {
                    if (interactions[i] is null)
                        continue;

                    var value = Unsafe.As<T, int>(ref interactions[i]);
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
                                    await _connection.Write(_bytes);
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

        // source: https://stackoverflow.com/a/48599119
        private static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        // Not used
        public override void ReadData(IntPtr intPtr)
        {
            throw new NotImplementedException();
        }
        public override void ReadDataDebounce(IntPtr intPtr) { }

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