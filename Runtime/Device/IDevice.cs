using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MychIO.Connection;
using MychIO.Generic;

namespace MychIO.Device
{
    public interface IDevice : IIdentifier, IDisposable
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
        void ReadData(ReadOnlyMemory<byte> data);
        void ReadData(ReadOnlySpan<byte> data);
        void ReadData(IntPtr intPtr);
        void ReadDataWithDebounce(ReadOnlyMemory<byte> data);
        void ReadDataWithDebounce(ReadOnlySpan<byte> data);
        void ReadDataWithDebounce(IntPtr intPtr);
        void OnConnected();
        Task OnConnectedAsync();
        void OnDisconnected();
        Task OnDisconnectedAsync();
        IDevice Connect();
        Task<IDevice> ConnectAsync();
        void Disconnect();
        Task DisconnectAsync();
        void StopReading();
        void StartReading();
        bool CanConnect(IDevice device);
        void Write<T>(params T[] interactions) where T: Enum;
        Task WriteAsync<T>(params T[] interactions) where T : Enum;
    }
    // Where TZone is the input type, e.g. A1, and TState is the InputState
    interface IDevice<TZone, TState> : IDevice where TZone : Enum where TState : Enum
    {
        // Callback has parameters Input Type, and Interaction State (e.g. On/Off) respectively
        void SetInputCallbacks(IDictionary<TZone, Action<TZone, TState>> inputSubscriptions);
        Task SetInputCallbacksAsync(IDictionary<TZone, Action<TZone, TState>> inputSubscriptions);
        void AddInputCallback(TZone interactionZone, Action<TZone, TState> callback);
    }
}