using System;
using System.Diagnostics.CodeAnalysis;

namespace MychIO.Helper
{
    static class ThrowHelper
    {
        [DoesNotReturn]
        public static void NotSupported()
        {
            throw new NotSupportedException();
        }
        [DoesNotReturn]
        public static void NotSupported(string message)
        {
            throw new NotSupportedException(message);
        }
        [DoesNotReturn]
        public static void NotImplemented()
        {
            throw new NotImplementedException();
        }
        [DoesNotReturn]
        public static void NotImplemented(string message)
        {
            throw new NotImplementedException(message);
        }

        [DoesNotReturn]
        public static TReturn NotSupported<TReturn>()
        {
            throw new NotSupportedException();
        }
        [DoesNotReturn]
        public static TReturn NotSupported<TReturn>(string message)
        {
            throw new NotSupportedException(message);
        }
        [DoesNotReturn]
        public static TReturn NotImplemented<TReturn>()
        {
            throw new NotImplementedException();
        }
        [DoesNotReturn]
        public static TReturn NotImplemented<TReturn>(string message)
        {
            throw new NotImplementedException(message);
        }
    }
}