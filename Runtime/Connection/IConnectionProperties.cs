using System;
using System.Collections.Generic;
using MychIO.Generic;

namespace MychIO.Connection
{
    public interface IConnectionProperties : IIdentifier
    {
        ConnectionType ConnectionType { get; }
        IDictionary<string, dynamic> Properties { get; }
        IConnectionProperties UpdateProperties(IDictionary<string, dynamic> updateProperties);
        // Used internally to store read errors
        IEnumerable<string> Errors { get; }
        TimeSpan GetDebounceThreshold();
    }
}