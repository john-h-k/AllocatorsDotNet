namespace Allocators.Win32.Handles
{
    public unsafe class SafeHeapAllocHandle : SafeMemoryHandle
    {
        public SafeHeapAllocHandle(bool ownsHandle) : base(ownsHandle)
        {
        }

        public SafeHeapAllocHandle(void* handle, bool ownsHandle = true) : base(handle, ownsHandle)
        {
        }

        protected override bool ReleaseHandle() 
            => IsInvalid || IsClosed || NativeMethods.HeapFree(handle.ToPointer());
    }
}