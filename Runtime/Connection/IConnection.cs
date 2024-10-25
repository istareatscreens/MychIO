using System.Threading.Tasks;

namespace MychIO.Connection
{
    public interface IConnection
    {
        void Disconnect();
        Task Connect();
        bool IsConnected();
        bool CanConnect(IConnection connection);
        bool IsReading();
        void StopReading();
        void Read();
        Task Write(byte[] data);
    }
}