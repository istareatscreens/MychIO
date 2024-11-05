using System;
using System.Collections.Generic;

namespace MychIO.Connection.Keyboard
{
#nullable enable
    public class KeyboardProperties : ConnectionProperties
    {
        // WARNING: Adding {get; set;} will break serialization/unserialization of properties!
        public int PollTimeoutMs;
        public IDictionary<Enum, Enum>? Mapping;

        // Constructor that initializes all properties
        public KeyboardProperties()
        {
            PopulatePropertiesFromFields();
        }

        // Copy Constructor used for creating properties objects from default properties
        public KeyboardProperties(
            IDictionary<Enum, Enum>? mapping = null
        )
        {
            Mapping = mapping;
        }

        public override ConnectionType GetConnectionType() => ConnectionType.Keyboard;
    }
}