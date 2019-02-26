using System;
using System.Collections.Generic;
using System.Text;
using Allocators.Win32;
using static Allocators.Win32.NativeEnums;

namespace Allocators
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
        // For efficient translation
        internal const AllocFlags ReadWrite = AllocFlags.Read | AllocFlags.Write;
        internal const AllocFlags ReadWriteExe = AllocFlags.Read | AllocFlags.Write | AllocFlags.Execute;
        internal const AllocFlags ReadExe = AllocFlags.Read | AllocFlags.Execute;

        public static ProtectionTypes TranslateToWin32(this AllocFlags flags)
        {
            switch (flags)
            {
                case AllocFlags.None:
                    return ProtectionTypes.PAGE_NOACCESS;
                case AllocFlags.Read:
                    return ProtectionTypes.PAGE_READONLY;
                case AllocFlags.Write:
                    throw new ArgumentException("Cannot have write-only memory");
                case AllocFlags.Execute:
                    return ProtectionTypes.PAGE_READWRITE;

                case ReadWrite:
                    return ProtectionTypes.PAGE_READWRITE;
                case ReadExe:
                    return ProtectionTypes.PAGE_EXECUTE_READ;
                case ReadWriteExe:
                    return ProtectionTypes.PAGE_EXECUTE_READWRITE;
            }

            ThrowHelper.ThrowArgEx("Invalid flag combination");
            return default; // unreachable but necessary
        }
    }
}
