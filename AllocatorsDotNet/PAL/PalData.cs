using System;
using System.Runtime.InteropServices;

namespace AllocatorsDotNet.PAL
{
    public enum OS
    {
        Windows = 1,
        Linux,
        Osx,
        FreeBsd
    }
    public static class PalData
    {
        public static bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsLinux { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        public static bool IsOsx { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsFreeBsd { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);

        public static OS CurrentPlatform { get; }


        static PalData()
        {
            if (IsWindows)
                CurrentPlatform = OS.Windows;
            else if (IsOsx)
                CurrentPlatform = OS.Osx;
            else if (IsLinux)
                CurrentPlatform = OS.Linux;
            else if (IsFreeBsd)
                CurrentPlatform = OS.FreeBsd;
            else
            {
                ThrowHelper.ThrowPlatformNotSupportedException("Unrecognised OS");
            }
        }
    }
}