using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using AllocatorsDotNet.Win32;
using AllocatorsDotNet.Win32.Handles;
using static AllocatorsDotNet.PAL.PalData;

namespace AllocatorsDotNet.PAL
{
    internal static unsafe class Allocation
    {
        public static readonly int PageSize = Environment.SystemPageSize;

        static Allocation()
        {
            Trace.Assert((PageSize & (PageSize - 1)) == 0);
        }

        public static SafeHandleZeroIsInvalid Alloc(AllocFlags flags, int size) => Alloc(flags, (IntPtr)size);
        public static SafeHandleZeroIsInvalid Alloc(AllocFlags flags, IntPtr size)
        {
            if (IsWindows)
            {
                // need to VirtualAlloc() an entire page with the permissions
                NativeEnums.ProtectionTypes protectionFlags = flags.TranslateToNativeFlags();
                var handle = new SafePageBlockHandle(
                    (void*)UnsafeNativeMethods.VirtualAlloc(
                        IntPtr.Zero,
                        (IntPtr)PageSize,
                        NativeEnums.MemAllocTypeFlags.MEM_COMMIT | NativeEnums.MemAllocTypeFlags.MEM_RESERVE,
                        protectionFlags));

                if (handle.IsInvalid)
                    ThrowHelper.ThrowInvalidOperationException(
                        $"Allocation failed, with HRESULT {UnsafeNativeMethods.GetLastError():X8}");
                return handle;
            }
            else
            {
                ThrowHelper.ThrowNotImplementedException();
                return default;
            }
        }

        public static void ChangeProtection(SafeHandleZeroIsInvalid handleZeroIsInvalid, IntPtr size, AllocFlags newFlags, ref bool success)
        {
            if (success)
                ThrowHelper.ThrowArgumentException("ref bool must be initialized to false");

            if (IsWindows)
            {
                NativeEnums.ProtectionTypes dummy;
                success = UnsafeNativeMethods.VirtualProtect(
                    handleZeroIsInvalid.DangerousGetHandle(),
                    size,
                    newFlags.TranslateToNativeFlags(),
                    &dummy);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static MemInfo QueryMemInfo(SafeHandleZeroIsInvalid handleZeroIsInvalid, IntPtr length)
        {
            if (IsWindows)
            {
                MemInfo mem = default;
                IntPtr sizeBuf = UnsafeNativeMethods.VirtualQuery(handleZeroIsInvalid.DangerousGetHandle(), &mem, length);

                if (sizeBuf == IntPtr.Zero)
                {
                    ThrowHelper.GenericThrow(new Win32Exception($"{nameof(UnsafeNativeMethods.VirtualQuery)} failed"));
                }

                return mem;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
