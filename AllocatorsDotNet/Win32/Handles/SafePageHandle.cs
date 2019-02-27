using System;

namespace AllocatorsDotNet.Win32.Handles
{
    public unsafe class SafePageHandle : SafeMemoryHandle
    {
        public SafePageHandle(bool ownsHandle) : base(ownsHandle)
        {
        }

        public SafePageHandle(void* pointer, bool ownsHandle = true) : base(pointer, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
            => IsInvalid || IsClosed ||
               UnsafeNativeMethods.VirtualFree(handle, IntPtr.Zero, NativeEnums.MemFreeTypeFlags.MEM_RELEASE);
    }
}
