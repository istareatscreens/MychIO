using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;

namespace MychIO.Device
{
    public interface IDevice
    {
        void ResetState();
        // TODO: create multiple read methods (likely dont want wrapper since its slower)
        void ReadData(byte[] data);
        Task OnStartWrite();
        Task<IDevice> Connect();
        void Disconnect();
        bool IsConnected();
        bool CanConnect(IDevice device);
        IConnection GetConnection();
        Task Write(params Enum[] interactions);
    }
    // Where T is the input type, e.g. A1
    interface IDevice<T> : IDevice where T : Enum
    {
        IConnectionProperties GetConnectionProperties();
        // Callback has parameters Input Type, and Interaction State (e.g. On/Off) respectively
        void SetInputCallbacks(IDictionary<T, Action<T, Enum>> inputSubscriptions);
        void AddInputCallback(T interactionZone, Action<T, Enum> callback);

    }
}