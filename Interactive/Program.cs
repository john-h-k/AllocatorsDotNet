using System;
using System.Buffers;
using System.Diagnostics;
using AllocatorsDotNet.Unmanaged;

namespace Interactive
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var allocator = new LinearAllocator<byte>(1024);
            IMemoryOwner<byte> block = allocator.Rent(512);
            Span<byte> span = block.Memory.Span;
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = unchecked((byte)i);
            }
        }

        private static void Method(Memory<byte> mem)
        {
            Span<byte> span = mem.Span;
            for (var i = 0; i < span.Length; i++)
            {
                Debug.Assert(span[i] == unchecked((byte)i));
            }
        }
    }
}
