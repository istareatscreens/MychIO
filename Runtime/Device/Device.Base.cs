using Unity;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;
using MychIO.Connection;
using PlasticGui.WorkspaceWindow.PendingChanges;

namespace MychIO.Device
{
    public abstract partial class Device<T1, T2> : IDevice<T1> where T1 : Enum where T2: IConnectionProperties
    {

        protected readonly IConnectionProperties _connectionProperties;
        protected IDictionary<T1, Action<T1, Enum>> _inputSubscriptions; 
        protected IConnection _connection;

        protected Device(
            IDictionary<T1, Action<T1, Enum>> inputSubscriptions,
            IDictionary<string,dynamic> connectionProperties = null
        )
        {
            _inputSubscriptions = inputSubscriptions;
            _connectionProperties = (null != connectionProperties) ?
                GetDefaultConnectionProperties().UpdateProperties(connectionProperties) :
                GetDefaultConnectionProperties();
                // TODO: Add connection factory
        }

        public IConnectionProperties GetConnectionProperties() => _connectionProperties;

        public void SetInputCallbacks(IDictionary<T1, Action<T1, Enum>> inputSubscriptions)
        {
            _inputSubscriptions = inputSubscriptions;
        }

        public void AddInputCallback(T1 interactionZone, Action<T1, Enum> callback)
        {
            _inputSubscriptions[interactionZone] = callback;
        }

        public async Task<IDevice> Connect(){
            await _connection.Connect();
            return this;
        }
        public void Disconnect(){
            _connection.Disconnect();
        }

        public abstract void ResetState();

        public abstract Task OnStartWrite();

        public abstract void ReadData(byte[] data);

        public abstract Task Write(params Enum[] interactions);
    }
}