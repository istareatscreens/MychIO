using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MychIO.Helper
{
    public static class ArrayHelper
    {
        public static ValueEnumerable<T> ToEnumerable<T>(T[] source)
        {
            return new ValueEnumerable<T>(source);
        }
    }
    public struct ValueEnumerable<T>
    {
        T[] _source;

        public ValueEnumerable(T[] source)
        {
            _source = source;
        }
        public ValueEnumerator<T> GetEnumerator() => new ValueEnumerator<T>(_source);
    }
    public struct ValueEnumerator<T>
    {
        public T? Current { get; private set; }
        int _index;
        T[] _source;
        public ValueEnumerator(T[] source)
        {
            _index = 0;
            _source = source;
            Current = default;
        }
        public bool MoveNext()
        {
            if (_index >= _source.Length)
                return false;
            Current = _source[_index++];
            return true;
        }
    }
}
