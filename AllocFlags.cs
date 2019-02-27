using System;
using AllocatorsDotNet.Win32;

namespace AllocatorsDotNet
{
    [Flags]
    public enum AllocFlags : uint
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4
    }

    internal static class AllocFlagExtensions
    {
        // For easiness to read
        internal const AllocFlags ReadWrite = AllocFlags.Read | AllocFlags.Write;
        internal const AllocFlags ReadWriteExe = AllocFlags.Read | AllocFlags.Write | AllocFlags.Execute;
        internal const AllocFlags ReadExe = AllocFlags.Read | AllocFlags.Execute;

        public static NativeEnums.ProtectionTypes TranslateToWin32(this AllocFlags flags)
        {
            switch (flags)
            {
                // Normal cases

                case AllocFlags.None:
                    return NativeEnums.ProtectionTypes.PAGE_NOACCESS;
                case AllocFlags.Read:
                    return NativeEnums.ProtectionTypes.PAGE_READONLY;
                case AllocFlags.Execute:
                    return NativeEnums.ProtectionTypes.PAGE_READWRITE;
                case ReadWrite:
                    return NativeEnums.ProtectionTypes.PAGE_READWRITE;
                case ReadExe:
                    return NativeEnums.ProtectionTypes.PAGE_EXECUTE_READ;
                case ReadWriteExe:
                    return NativeEnums.ProtectionTypes.PAGE_EXECUTE_READWRITE;

                // Exceptional cases

                case AllocFlags.Write:
                    ThrowHelper.ThrowArgEx("Cannot have write only memory");
                    break;
                case AllocFlags.Write | AllocFlags.Execute:
                    ThrowHelper.ThrowArgEx("Cannot have write + execute only memory");
                    break;
                default:
                    ThrowHelper.ThrowArgEx("Unrecognised enum value");
                    break;
            }

            return default;
        }
    }
}
