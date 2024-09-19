using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MychIO.Device;

namespace MychIO.Connection
{
    public abstract class Connection: IConnection
    {
        protected IDevice _device;
        protected IConnectionProperties _connectionProperties;

        public Connection(
            IDevice device,
            IConnectionProperties connectionProperties
         ){
            _device = device;
            _connectionProperties = connectionProperties;
          }

        public abstract Task Connect();

        public abstract void Disconnect();

        public abstract bool IsConnected();

        public abstract Task Write(byte[] bytes);
    }

}