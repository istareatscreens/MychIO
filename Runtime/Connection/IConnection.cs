using System.Threading.Tasks;

namespace MychIO.Connection
{
    public interface IConnection
    {
        void Disconnect();
        Task Connect();
        bool IsConnected();
        Task Write(byte[] data);
    }
}