using System;

namespace AllocatorsDotNet.Win32.Handles
{
    public unsafe class SafePageBlockHandle : SafeHandleZeroIsInvalid
    {
        public SafePageBlockHandle(bool ownsHandle) : base(ownsHandle)
        {
        }

        public SafePageBlockHandle(void* pointer, bool ownsHandle = true) : base(pointer, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
            => IsInvalid || IsClosed ||
               UnsafeNativeMethods.VirtualFree(handle, IntPtr.Zero, NativeEnums.MemFreeTypeFlags.MEM_RELEASE);
    }
}
