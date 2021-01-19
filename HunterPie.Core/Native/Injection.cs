using System;
using System.IO;
using static HunterPie.Memory.Kernel;


namespace HunterPie.Native
{
    /// <summary>
    /// Injection will handle the HunterPie.Native.dll injection
    /// </summary>
    internal static class Injection
    {

        internal static bool IsNativeEnabled { get; private set; }

        internal static bool InjectNative()
        {
            string nativeDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "HunterPie.Native.dll");

            if (!File.Exists(nativeDllPath))
            {
                IsNativeEnabled = false;
                return false;
            }

            IntPtr mAlloc = VirtualAllocEx(
                ProcessHandle,
                IntPtr.Zero,
                (uint)nativeDllPath.Length + 1,
                AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite
            );

            if (mAlloc == IntPtr.Zero)
                IsNativeEnabled = false;
            else
            {
                if (!Write((long)mAlloc, nativeDllPath))
                    IsNativeEnabled = false;
                else
                {
                    IntPtr kernel32Addr = GetModuleHandle("kernel32");
                    IntPtr loadLibraryA = GetProcAddress(kernel32Addr, "LoadLibraryA");
                    IntPtr lpThreadId = IntPtr.Zero;
                    IntPtr thread = CreateRemoteThread(
                                        ProcessHandle,
                                        IntPtr.Zero,
                                        0,
                                        loadLibraryA,
                                        mAlloc,
                                        0,
                                        lpThreadId
                                    );
                    IsNativeEnabled = thread != IntPtr.Zero;
                }
            }

            return IsNativeEnabled;
        }
    }
}
