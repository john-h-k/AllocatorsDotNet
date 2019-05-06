using System;
using System.Runtime.InteropServices;

namespace AllocatorsDotNet.Win32.Handles
{
    public abstract unsafe class SafeHandleZeroIsInvalid : SafeHandle
    {
        protected SafeHandleZeroIsInvalid(bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        {
        }

        #region Equality

        // TODO Do we need these
        //public static bool Equals(SafeHandleZeroIsInvalid left, SafeHandleZeroIsInvalid right) =>
        //    left.Equals(right);
        //public override bool Equals(object obj) => obj is SafeHandleZeroIsInvalid other && Equals(other);
        //public override int GetHashCode() => handle.GetHashCode();

        //public static bool operator ==(SafeHandleZeroIsInvalid left, SafeHandleZeroIsInvalid right)
        //{
        //    return Equals(left, right);
        //}

        //public static bool operator !=(SafeHandleZeroIsInvalid left, SafeHandleZeroIsInvalid right)
        //{
        //    return !Equals(left, right);
        //}

        //public bool Equals(SafeHandleZeroIsInvalid other) => handle == other.handle;

        #endregion

        protected SafeHandleZeroIsInvalid(void* pointer, bool ownsHandle)
            : base(IntPtr.Zero, ownsHandle) => handle = (IntPtr)pointer;

        protected abstract override bool ReleaseHandle();

        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}