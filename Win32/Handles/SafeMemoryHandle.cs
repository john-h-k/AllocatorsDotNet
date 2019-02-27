using System;
using System.Runtime.InteropServices;

namespace AllocatorsDotNet.Win32.Handles
{
    public abstract unsafe class SafeMemoryHandle : SafeHandle
    {
        protected SafeMemoryHandle(bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        {
        }

        protected SafeMemoryHandle(void* pointer, bool ownsHandle) 
            : base(IntPtr.Zero, ownsHandle) => handle = (IntPtr)pointer;

        protected abstract override bool ReleaseHandle();

        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}