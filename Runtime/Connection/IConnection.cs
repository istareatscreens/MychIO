using System;
using System.Threading.Tasks;

namespace MychIO.Connection
{
    public interface IConnection: IDisposable
    {
        bool IsConnected { get; }
        bool IsReading { get; }

        void Disconnect();
        Task DisconnectAsync();
        void Connect();
        Task ConnectAsync();
        bool CanConnect(IConnection connection);
        void StopReading();
        void Read();
        void Write(ReadOnlySpan<byte> data);
        Task WriteAsync(byte[] data);
        Task WriteAsync(ReadOnlyMemory<byte> data);
    }
}