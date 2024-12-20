using System;
using System.Collections.Generic;
using System.Reflection;

namespace MychIO.Connection
{

    // Note all public properties of this class will be added to the _properties object
    // Properties should only be updated on object construction through the constructor
    // WARNING! Member variables must be directly accessible (no {get; set;}) or it will break serialization/unserialization
    public abstract class ConnectionProperties : IConnectionProperties
    {
        public int DebounceTimeMs;
        public string Id { get; private set; }
        private Queue<string> _errors = new();
        private IDictionary<string, dynamic> _properties = new Dictionary<string, dynamic>();
        public IDictionary<string, dynamic> GetProperties() => _properties;

        // Modify the constructor to accept DebounceTimeMs
        public ConnectionProperties(int debounceTimeMs = 0)
        {
            DebounceTimeMs = debounceTimeMs;
        }

        public IConnectionProperties UpdateProperties(IDictionary<string, dynamic> updateProperties)
        {
            _properties = MergeProperties(_properties, updateProperties);
            UpdateFieldsFromProperties();
            Id = _properties.TryGetValue("Id", out var id) && id is string v ? v : Guid.NewGuid().ToString();
            return this;
        }

        protected static IDictionary<string, dynamic> MergeProperties(
            IDictionary<string, dynamic> overWrittenProperties, IDictionary<string, dynamic> updateProperties)
        {
            var result = new Dictionary<string, dynamic>(overWrittenProperties);
            foreach (var (key, value) in updateProperties)
            {
                result[key] = value;
            }
            return result;
        }

        protected void PopulatePropertiesFromFields()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var value = field.GetValue(this);
                if (null == value)
                {
                    throw new Exception();
                }
                try
                {
                    _properties[field.Name] = value;
                }
                catch
                {
                    _errors.Enqueue($"Failed to populate property: {field.Name} on {GetType().Name},{field.Name},{GetType().Name}");
                }
            }
        }

        protected void UpdateFieldsFromProperties()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (!_properties.TryGetValue(field.Name, out var value))
                {
                    throw new Exception();
                }
                try
                {
                    field.SetValue(this, value);
                }
                catch
                {
                    _errors.Enqueue($"Failed to apply property: {field.Name} on {GetType().Name},{field.Name},{GetType().Name}");
                }
            }
        }

        public IEnumerable<string> GetErrors()
        {
            while (0 < _errors.Count)
            {
                yield return _errors.Dequeue();
            }
        }
        public TimeSpan GetDebounceTime()
        {
            return TimeSpan.FromMilliseconds(DebounceTimeMs);
        }

        public abstract ConnectionType GetConnectionType();

    }
}