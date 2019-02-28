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
        private AllocFlags _allocFlags;

        public AllocFlags AllocFlags => _allocFlags;

        // ReSharper disable once StaticMemberInGenericType
        private const int AllocSize = 1024;

        public LinearAllocator(int size) : this(AllocFlags.Read | AllocFlags.Write, size) { }

        internal static bool HandleEquals(LinearAllocator<T> left, LinearAllocator<T> right)
            => left._start.DangerousGetHandle() == right._start.DangerousGetHandle();

        internal MemInfo GetCurrentMemInfo()
        {
            MemInfo info;
            UnsafeNativeMethods.VirtualQuery((void*)_start.DangerousGetHandle(), &info, (IntPtr)sizeof(MemInfo));
            return info;
        }

        public void DangerousChangeProtection(ref bool success, AllocFlags newFlags)
        {
            if (success)
                ThrowHelper.ThrowArgEx("ref bool must be initialized to false");

            NativeEnums.ProtectionTypes dummy;

            success = UnsafeNativeMethods.VirtualProtect(
                _start.DangerousGetHandle(),
                (IntPtr) _length, newFlags.TranslateToWin32(),
                &dummy);
        }

        public LinearAllocator(AllocFlags flags = AllocFlags.Read | AllocFlags.Write, int size = AllocSize)
        {
            _start = NativeMethods.Alloc(flags, (IntPtr)size);
            _length = size;
            _allocFlags = flags;
        }

        public LinearAllocator(LinearAllocator<T> other, AllocFlags? changeFlags = null)
        {
            _start = other._start;
            _length = other._length;

            if (changeFlags is null)
            {
                _allocFlags = other._allocFlags;
            }
            else
            {
                NativeEnums.ProtectionTypes dummy;

                bool result = UnsafeNativeMethods.VirtualProtect(other._start.DangerousGetHandle(), (IntPtr)other._length,
                    changeFlags.Value.TranslateToWin32(), &dummy);

                if (!result)
                    throw new Exception("Unexpected failure"); // TODO

                _allocFlags = changeFlags.Value;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            // We don't dispose of the handle in case other managers are pointing to it
            // the GC recognises when it needs to be finalized

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
