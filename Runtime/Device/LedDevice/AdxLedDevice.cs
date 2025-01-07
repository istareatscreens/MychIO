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
                LedCommand.SetColor0, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor1, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor2, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor3, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor4, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor5, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor6, new byte[][]
                {
                    new byte[] { 0xE0, 0x11, 0x01, 0x05, 0x31, 0x01, 0x00, 0x00, 0x00, 0x00 }
                }
            },
            {
                LedCommand.SetColor7, new byte[][]
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
        async Task SetColor(Color newColor,int index)
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
        public override async Task Write<T>(params T[] interactions)
        {
            // data = [LedCommand, Red, Green, Blue]
            for (var i = 0; i < interactions.Length; i++)
            {
                var value = Unsafe.As<T, int>(ref interactions[i]);
                var data = ArrayPool<byte>.Shared.Rent(4);
                MemoryMarshal.Write(data, ref value);
                var command = (LedCommand)data[0];
                switch(command)
                {
                    case LedCommand.SetColor0:
                    case LedCommand.SetColor1:
                    case LedCommand.SetColor2:
                    case LedCommand.SetColor3:
                    case LedCommand.SetColor4:
                    case LedCommand.SetColor5:
                    case LedCommand.SetColor6:
                    case LedCommand.SetColor7:
                        var r = data[1];
                        var g = data[2];
                        var b = data[3];
                        var newColor = new Color(r / 255, g / 255, b / 255);
                        await SetColor(newColor, (int)command - 2);
                        break;
                    default:
                        if(Commands.TryGetValue(command, out byte[][] bytes))
                        {
                            foreach(var _bytes in ArrayHelper.ToEnumerable(bytes))
                            {
                                await _connection.Write(_bytes);
                            }
                        }
                        else
                        {
                            ArrayPool<byte>.Shared.Return(data);
                            throw new ArgumentException("Command not found.", nameof(command));
                        }
                        break;
                }
                ArrayPool<byte>.Shared.Return(data);
            }
            
            //var commandBytes = interactions.OfType<LedCommand>()
            //.SelectMany(command =>
            //{
            //    if (Commands.TryGetValue(command, out byte[][] bytes))
            //    {
            //        return bytes;
            //    }
            //    else
            //    {
            //        throw new ArgumentException("Command not found.", nameof(command));
            //    }
            //}).ToArray();

            //foreach (var command in commandBytes)
            //{
            //    await _connection.Write(command);
            //}
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

    }

}