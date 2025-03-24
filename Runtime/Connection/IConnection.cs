using System.Threading.Tasks;

namespace MychIO.Connection
{
    public interface IConnection
    {
        bool IsConnected { get; }
        bool IsReading { get; }

        Task Disconnect();
        Task Connect();
        bool CanConnect(IConnection connection);
        void StopReading();
        void Read();
        Task Write(byte[] data);
    }
}