using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Generic;
using PlasticGui.WorkspaceWindow;

namespace MychIO.Device
{
    public interface IDevice : IIdentifier
    {
        void ResetState();
        void ReadData(byte[] data);
        void ReadData(IntPtr data);
        Task OnStartWrite();
        Task<IDevice> Connect();
        void Disconnect();
        bool IsConnected();
        bool CanConnect(IDevice device);
        IConnection GetConnection();
        Task Write(params Enum[] interactions);
        DeviceClassification GetClassification();
    }
    // Where T is the input type, e.g. A1
    interface IDevice<T1, T2> : IDevice where T1 : Enum where T2 : Enum
    {
        IConnectionProperties GetConnectionProperties();
        // Callback has parameters Input Type, and Interaction State (e.g. On/Off) respectively
        void SetInputCallbacks(IDictionary<T1, Action<T1, T2>> inputSubscriptions);
        void AddInputCallback(T1 interactionZone, Action<T1, T2> callback);

    }
}