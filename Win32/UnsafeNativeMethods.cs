﻿using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Allocators.Win32.Handles;
using static Allocators.Win32.NativeEnums;
// ReSharper disable InconsistentNaming

namespace Allocators.Win32
{
    internal static unsafe class UnsafeNativeMethods
    {
        // pointer-types are used to represent pointers that are used, IntPtr s are used for opaque handles

        public static IntPtr ProcessHeapHandle = GetProcessHeap();

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", EntryPoint = "HeapAlloc", ExactSpelling = true, SetLastError = true)]
        public static extern void* HeapAlloc(IntPtr heapHandle, HeapAllocFlags flags, IntPtr size);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", EntryPoint = "HeapFree", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool HeapFree(IntPtr heapHandle, HeapAllocFlags flags, void* mem);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", EntryPoint = "VirtualAlloc", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr VirtualAlloc(IntPtr address, IntPtr size, MemAllocTypeFlags allocationType, ProtectionTypes protectionFlags);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", EntryPoint = "VirtualFree", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFree(IntPtr address, IntPtr size, MemFreeTypeFlags freeType);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", EntryPoint = "VirtualProtect", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualProtect(IntPtr address, IntPtr size, ProtectionTypes newProtectionFlags, ProtectionTypes* oldProtectionFlags);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [DllImport("kernel32", EntryPoint = "GetProcessHeap", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcessHeap();

        // For consistency in naming and location
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int GetLastError() => Marshal.GetLastWin32Error();
    }

    internal static unsafe class NativeMethods
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void* HeapAlloc(IntPtr size, HeapAllocFlags flags = 0,
            IntPtr heap = default)
        {
            if (heap == default)
                heap = UnsafeNativeMethods.ProcessHeapHandle;

            return UnsafeNativeMethods.HeapAlloc(heap, flags, size);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool HeapFree(void* mem, HeapAllocFlags flags = 0, IntPtr heap = default)
        {
            if (heap == default)
                heap = UnsafeNativeMethods.ProcessHeapHandle;

            return UnsafeNativeMethods.HeapFree(heap, flags, mem);
        }
        
        public static SafeMemoryHandle Alloc(AllocFlags flags, IntPtr size)
        {
            if (flags == AllocFlagExtensions.ReadWrite)
            {
                // yay! can use HeapAlloc(), keep life easy
                return new SafeHeapAllocHandle(HeapAlloc(size), ownsHandle: true);
            }
            else
            {
                // need to VirtualAlloc() an entire page with the permissions
                return new SafePageHandle((void*)UnsafeNativeMethods.VirtualAlloc(IntPtr.Zero, IntPtr.Zero, MemAllocTypeFlags.MEM_COMMIT | MemAllocTypeFlags.MEM_RESERVE, flags.TranslateToWin32()));
            }
        }
    }

    internal static class NativeEnums
    {
        [Flags]
        public enum MemAllocTypeFlags : uint
        {
            MEM_COMMIT = 0x00001000,
            MEM_RESERVE = 0x00002000,
            MEM_RESET = 0x00080000,
            MEM_RESET_UNDO = 0x1000000
        }

        public enum MemAllocModifiers : uint
        {
            MEM_LARGE_PAGES = 0x20000000,
            MEM_PHYSICAL = 0x00400000,
            MEM_TOP_DOWN = 0x00100000,
            MEM_WRITE_WATCH = 0x00200000
        }

        public enum ProtectionTypes : uint
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_TARGETS_INVALID = 0x40000000,
            PAGE_TARGETS_NO_UPDATE = 0x40000000
        }

        public enum ProtectionTypeModifiers : uint
        {
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        [Obsolete("Not implemented yet", error: true)]
        public enum EnclaveModifiers : uint
        {
            // TODO
        }

        [Flags]
        public enum MemFreeTypeFlags : uint
        {
            MEM_COALESCE_PLACEHOLDERS = 0x00000001,
            MEM_PRESERVE_PLACEHOLDER = 0x00000002,
            MEM_DECOMMIT = 0x00004000,
            MEM_RELEASE = 0x00008000
        }

        [Flags]
        internal enum HeapAllocFlags : uint
        {
            HEAP_GENERATE_EXCEPTIONS = 0x00000004,
            HEAP_NO_SERIALIZE = 0x00000001,
            HEAP_ZERO_MEMORY = 0x00000008
        }
    }
}
