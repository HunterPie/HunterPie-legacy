using System;
using System.Diagnostics;
using System.IO;
using HunterPie.Core;
using HunterPie.Memory;
using Debugger = HunterPie.Logger.Debugger;
using static HunterPie.Memory.Kernel;
using System.Linq;


namespace HunterPie.Native
{
    /// <summary>
    /// Injection will handle the HunterPie.Native.dll injection
    /// </summary>
    internal static class Injector
    {

        internal static bool IsNativeEnabled { get; private set; }

        internal static bool InjectNative()
        {
            string nativeDllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", "HunterPie.Native.dll");
            bool isNativeFunctionsEnabled = UserSettings.PlayerConfig.HunterPie.EnableNativeFunctions;

            if (!File.Exists(nativeDllPath) || !isNativeFunctionsEnabled)
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

        internal static bool CheckIfCRCBypassExists()
        {
            // I wanted to get whether CRCBypass exists based on the game modules
            // but for some reason they don't appear in the modules list?

            string path = Path.GetDirectoryName(Kernel.Process.MainModule.FileName);

            string dtdata = Path.Combine(path, "dtdata.dll");
            string loader = Path.Combine(path, "loader.dll");
            string crcBypass = Path.Combine(path, "nativePC", "plugins", "!CRCBypass.dll");

            return File.Exists(dtdata) && File.Exists(loader) && File.Exists(crcBypass);
        }

        internal static bool CheckIfAlreadyInjected()
        {
            return Kernel.Process.Modules.Cast<ProcessModule>()
                    .Any(m => m.ModuleName == "HunterPie.Native.dll");
        }
    }
}
