using System;
using System.Buffers;
using System.Threading;
using AllocatorsDotNet.PAL;
using AllocatorsDotNet.Win32;
using AllocatorsDotNet.Win32.Handles;

namespace AllocatorsDotNet.Unmanaged
{
    public unsafe class LinearAllocator<T> : MemoryPool<T> where T : unmanaged
    {
        private readonly SafeHandleZeroIsInvalid _start;
        private readonly int _length;
        private int _nextStart;
        private bool _disposed;



        // ReSharper disable once StaticMemberInGenericType
        private static readonly int AllocSize = Allocation.PageSize;
        public LinearAllocator(int size, AllocFlags flags = AllocFlags.Read | AllocFlags.Write)
        {
            _start = Allocation.Alloc(flags, (IntPtr)size);
            _length = size;
        }

        internal static bool HandleEquals(LinearAllocator<T> left, LinearAllocator<T> right)
            => left._start.DangerousGetHandle() == right._start.DangerousGetHandle();

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            _start.Dispose();

            _disposed = true;
        }

        public override IMemoryOwner<T> Rent(int minBufferSize = -1)
        {
            ThrowIfDisposed();
            if (minBufferSize == -1) minBufferSize = AllocSize;
            int length = minBufferSize * sizeof(T);
            if (length + _nextStart > _length)
                ThrowHelper.ThrowInsufficientMemoryException($"Required {length} bytes, but buffer could only provide {_length - _nextStart}");

            var block = new BlockManager(_start, _nextStart, length);
            _nextStart += length;
            return block;
        }

        public override int MaxBufferSize { get; } = int.MaxValue; // TODO

        private void ThrowIfDisposed()
        {
            if (_disposed)
                ThrowHelper.ThrowObjectDisposedException();
        }

        private class BlockManager : MemoryManager<T>
        {
            private readonly SafeHandleZeroIsInvalid _block;
            private readonly int _start;
            private readonly int _length;

            public BlockManager(SafeHandleZeroIsInvalid handle, int start, int length)
            {
                _block = handle;
                _start = start;
                _length = length;
            }

            private bool _disposed = false;
            protected override void Dispose(bool disposing)
            {
                _disposed = true;
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                    ThrowHelper.ThrowObjectDisposedException("Object disposed");
            }

            public override Span<T> GetSpan()
            {
                ThrowIfDisposed();
                return new Span<T>((void*)(_block.DangerousGetHandle() + _start), _length);
            }

            public override MemoryHandle Pin(int elementIndex = 0)
            {
                ThrowIfDisposed();
                var success = false;
                do
                {
                    _block.DangerousAddRef(ref success);
                } while (!success);

                return new MemoryHandle(pointer: (void*)(_block.DangerousGetHandle() + elementIndex * sizeof(T)), pinnable: this);
            }

            public override void Unpin()
            {
                ThrowIfDisposed();
                _block.DangerousRelease();
            }
        }
    }
}
