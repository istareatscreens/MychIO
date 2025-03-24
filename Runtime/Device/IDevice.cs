using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Generic;

namespace MychIO.Device
{
    public interface IDevice : IIdentifier
    {
        string Name { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        bool IsConnected { get; }
        bool IsReading { get; }
        IConnection Connection { get; }
        DeviceClassification Classification { get; }
        IConnectionProperties ConnectionProperties { get; }

        void ResetState();
        void ReadData(byte[] data);
        void ReadData(IntPtr intPtr);
        Task OnConnected();
        Task OnDisconnected();
        Task<IDevice> Connect();
        Task Disconnect();
        void StopReading();
        void StartReading();
        bool CanConnect(IDevice device);
        Task Write<T>(params T[] interactions) where T: Enum;
    }
    // Where T1 is the input type, e.g. A1, and T2 is the InputState
    interface IDevice<TZone, TState> : IDevice where TZone : Enum where TState : Enum
    {
        // Callback has parameters Input Type, and Interaction State (e.g. On/Off) respectively
        Task SetInputCallbacks(IDictionary<TZone, Action<TZone, TState>> inputSubscriptions);
        void AddInputCallback(TZone interactionZone, Action<TZone, TState> callback);
    }
}