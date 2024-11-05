using System.Collections.Generic;
using MychIO.Generic;

namespace MychIO.Connection
{
    public interface IConnectionProperties : IIdentifier
    {
        ConnectionType GetConnectionType();
        IDictionary<string, dynamic> GetProperties();
        IConnectionProperties UpdateProperties(IDictionary<string, dynamic> updateProperties);
        // Used internally to store read errors
        public IEnumerable<string> GetErrors();
    }
}