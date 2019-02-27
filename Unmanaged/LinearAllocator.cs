using System;
using System.Buffers;
using System.Threading;
using AllocatorsDotNet.Win32;
using AllocatorsDotNet.Win32.Handles;

namespace AllocatorsDotNet.Unmanaged
{
    public unsafe class LinearAllocator<T> : MemoryManager<T> where T : unmanaged
    {
        private SafeMemoryHandle _start;
        private int _length;
        private int _pinCount;
        private bool _disposed;

        // ReSharper disable once StaticMemberInGenericType
        private const int AllocSize = 1024;

        public LinearAllocator(int size) : this(AllocFlags.Read | AllocFlags.Write, size) { }

        public LinearAllocator(AllocFlags flags = AllocFlags.Read | AllocFlags.Write, int size = AllocSize)
        {
            _start = NativeMethods.Alloc(flags, (IntPtr)size);
            _length = size;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            _start.Dispose();
            _start.SetHandleAsInvalid();

            _disposed = true;
        }

        public void Dispose() => ((IDisposable)this).Dispose();

        private void ThrowDisposed()
        {
            if (_disposed)
                ThrowHelper.ThrowDisposed();
        }

        public override Span<T> GetSpan() // unsafe method
        {
            ThrowDisposed();

            if (_start.IsInvalid || _length == 0)
                throw new InvalidOperationException("Buffer doesn't exist");

            return new Span<T>(_start.DangerousGetHandle().ToPointer(), _length);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            ThrowDisposed();

            var success = false;
            do
            {
                _start.DangerousAddRef(ref success);
                // DEADLOCK TODO

            } while (!success);

            Interlocked.Increment(ref _pinCount);
            return new MemoryHandle((_start.DangerousGetHandle() + elementIndex * sizeof(T)).ToPointer(), default, this);
        }

        public override void Unpin()
        {
            ThrowDisposed();

            if (_pinCount < 1)
                throw new InvalidOperationException("Object was not pinned");

            _start.DangerousRelease();
            Interlocked.Decrement(ref _pinCount);
        }
    }
}
