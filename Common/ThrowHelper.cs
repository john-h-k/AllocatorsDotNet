using System;

namespace AllocatorsDotNet
{
    internal static class ThrowHelper
    {
        public static void GenericThrow(Exception e)
            => throw e;

        public static void ThrowIfNull(object obj, string paramName = null)
        {
            if (obj is null)
                ThrowArgumentNullException(paramName);
        }

        public static void ThrowObjectDisposedException(string name = null, Exception inner = null)
            => throw new ObjectDisposedException(name, inner);

        public static void ThrowInvalidOperationException(string message = null, Exception inner = null)
            => throw new InvalidOperationException(message, inner);

        public static void ThrowOutOfMemory(string message = null, Exception inner = null)
            => throw new OutOfMemoryException(message, inner);

        public static void ThrowInsufficientMemoryException(string message = null, Exception inner = null)
            => throw new InsufficientMemoryException(message, inner);

        #region ARG EXCEPTIONS

        public static void ThrowArgumentNullException()
            => throw new ArgumentNullException();

        public static void ThrowArgumentNullException(string paramName, string message = null)
            => throw new ArgumentNullException(paramName);

        public static void ThrowArgumentNullException(string message, Exception inner)
            => throw new ArgumentNullException(message, inner);

        public static void ThrowArgumentOutOfRangeException(string message = null, Exception inner = null)
            => throw new ArgumentOutOfRangeException(message, inner);

        public static void ThrowArgumentException(string message = null, Exception inner = null)
            => throw new ArgumentException(message, inner);

        #endregion
    }
}