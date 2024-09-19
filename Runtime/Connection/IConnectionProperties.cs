using System.Collections.Generic;

namespace MychIO.Connection
{
    public interface IConnectionProperties
    {
        ConnectionType GetConnectionType();
        IDictionary<string, dynamic> GetProperties();
        IConnectionProperties UpdateProperties(IDictionary<string, dynamic> updateProperties);

    }
}