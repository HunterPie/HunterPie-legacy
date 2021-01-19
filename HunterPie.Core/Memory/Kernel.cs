using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Debugger = HunterPie.Logger.Debugger;

namespace HunterPie.Memory
{
    public class Kernel
    {
        const int PROCESS_ALL_ACCESS = 0x001F0FFF;
        const string PROCESS_NAME = "MonsterHunterWorld";
        public const long NULLPTR = 0x00000000L;

        // Process info
        public static IntPtr WindowHandle { get; private set; }
        public static int GameVersion { get; private set; }
        public static int PID { get; private set; }
        public static IntPtr ProcessHandle { get; private set; } = (IntPtr)0;
        public static bool GameIsRunning { get; private set; }
        public static Process Process => MonsterHunter;
        public static bool IsForegroundWindow
        {
            get => isForegroundWindow;
            private set
            {
                if (value == isForegroundWindow)
                {
                    return;
                }

                // Wait until there's a subscriber to dispatch the event
                if (OnGameFocus == null || OnGameUnfocus == null)
                    return;

                isForegroundWindow = value;

                Dispatch(isForegroundWindow ? OnGameFocus : OnGameUnfocus);
            }
        }

        private static Process MonsterHunter;
        private static bool isForegroundWindow = false;

        // Scanner Thread
        private static ThreadStart scanGameMemoryRef;
        private static Thread scanGameMemory;

        // Kernel32 DLL
        [Flags]
        public enum AllocationType : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection : uint
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            bool bInheritHandle,
            int dwProcessId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out int lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect
        );

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreateFlags,
            IntPtr lpThreadId
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        /* Events */
        public delegate void ProcessHandler(object source, EventArgs args);
        public static event ProcessHandler OnGameStart;
        public static event ProcessHandler OnGameClosed;
        public static event ProcessHandler OnGameFocus;
        public static event ProcessHandler OnGameUnfocus;

        protected static void Dispatch(ProcessHandler e)
        {
            if (e is null)
                return;

            foreach (ProcessHandler sub in e.GetInvocationList())
            {
                try
                {
                    sub(nameof(Kernel), EventArgs.Empty);
                } catch (Exception err)
                {
                    Debugger.Error($"Exception in {sub.Method.Name}: {err.Message}");
                }
            }
        }

        /* Core code */
        public static void StartScanning()
        {
            // Start scanner thread
            scanGameMemoryRef = new ThreadStart(GetMonsterHunterProcess);
            scanGameMemory = new Thread(scanGameMemoryRef)
            {
                Name = "Scanner_Memory"
            };
            scanGameMemory.Start();
        }

        public static void StopScanning()
        {
            if (ProcessHandle != (IntPtr)0)
            {
                CloseHandle(ProcessHandle);
                ProcessHandle = IntPtr.Zero;
            }

            scanGameMemory?.Abort();
        }

        public static void GetMonsterHunterProcess()
        {
            bool lockSpam = false;
            bool lockSpam2 = false;
            while (true)
            {
                if (GameIsRunning)
                {
                    IsForegroundWindow = WindowsHelper.GetForegroundWindow() == WindowHandle;
                }
                if (MonsterHunter != null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                
                Process monsterHunterProcess = Process
                    .GetProcessesByName(PROCESS_NAME)
                    .FirstOrDefault(p => !string.IsNullOrEmpty(p.MainWindowTitle));

                // If there's no MHW instance of Monster Hunter: World running
                if (monsterHunterProcess == null)
                {
                    if (!lockSpam)
                    {
                        Debugger.Log("HunterPie is ready! Waiting for game process to start...");
                        lockSpam = true;
                    }
                    GameIsRunning = false;
                    PID = 0;
                }
                else
                {
                    if (string.IsNullOrEmpty(monsterHunterProcess.MainWindowTitle) ||
                        !monsterHunterProcess.MainWindowTitle.ToUpper().StartsWith("MONSTER HUNTER: WORLD"))
                    {
                        if (!lockSpam2 && string.IsNullOrEmpty(monsterHunterProcess.MainWindowTitle))
                        {
                            Debugger.Error($"Found Monster Hunter: World process, but the window title returned \"{monsterHunterProcess.MainWindowTitle}\"." +
                                $"Common causes for this:\n- Window is still loading\n- Stracker's console is the main process window. Click on the game window to fix this issue.");
                            lockSpam2 = true;
                        }
                        Thread.Sleep(500);
                        continue;
                    }

                    MonsterHunter = monsterHunterProcess;

                    PID = MonsterHunter.Id;
                    ProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, PID);

                    // Check if OpenProcess was successful
                    if (ProcessHandle == IntPtr.Zero)
                    {
                        Debugger.Error("Failed to open game process. Run HunterPie as Administrator!");
                        return;
                    }

                    try
                    {
                        GameVersion = int.Parse(MonsterHunter.MainWindowTitle.Split('(')[1].Trim(')'));
                    }
                    catch
                    {
                        Debugger.Error($"Failed to get Monster Hunter: World build version. Loading latest map version instead. Common reasons for this are:" +
                            $"\r- Stracker's Loader is the game process main window.\n" +
                            $"Click on the Monster Hunter: World window and then open HunterPie.");
                        GameVersion = GetLatestMap();
                    }
                    MonsterHunter.EnableRaisingEvents = true;
                    MonsterHunter.Exited += OnGameProcessExit;
                    WindowHandle = MonsterHunter.MainWindowHandle;
                    Dispatch(OnGameStart);
                    Debugger.Log($"Monster Hunter: World ({GameVersion}) found! (PID: {PID})");
                    GameIsRunning = true;
                }

                Thread.Sleep(2000);
            }
        }

        private static int GetLatestMap()
        {
            int[] mapFiles = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "address"))
                .Where(filename => filename.EndsWith(".map"))
                .Select(filename => Convert.ToInt32(filename.Split('.')[1])).ToArray();
            return mapFiles.OrderBy(version => version).Last();
        }

        private static void OnGameProcessExit(object sender, EventArgs e)
        {
            MonsterHunter.Exited -= OnGameProcessExit;
            MonsterHunter.Dispose();
            MonsterHunter = null;
            Debugger.Log("Game process closed!");
            CloseHandle(ProcessHandle);
            ProcessHandle = IntPtr.Zero;
            Dispatch(OnGameClosed);
        }

        /// <summary>
        /// Reads primitive type from Monster Hunter's memory.
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="address">Memory address</param>
        /// <returns>Value the given address is storing</returns>
        public static T Read<T>(long address) where T : struct
        {
            T[] buffer = Buffers.Get<T>();
            ReadProcessMemory(ProcessHandle, (IntPtr)address, buffer, Marshal.SizeOf<T>(), out _);
            return buffer[0];
        }

        /// <summary>
        /// Reads a multilevel pointer and returns the last address the sequence points to
        /// </summary>
        /// <param name="baseAddress">A static address</param>
        /// <param name="offsets">An array of offsets</param>
        /// <returns>The last address the multilevel pointer points to</returns>
        public static long ReadMultilevelPtr(long baseAddress, int[] offsets)
        {
            long address = baseAddress;
            foreach (int offset in offsets)
            {
                long temp = Read<long>(address);

                // In case we get ptr to a null value
                if (temp == NULLPTR)
                {
                    return NULLPTR;
                }

                address = temp + offset;
            }

            return address;
        }

        /// <summary>
        /// Reads a string from address
        /// </summary>
        /// <param name="address">Address where the string is located</param>
        /// <param name="size">Size of the string in bytes</param>
        /// <returns>The string without null characters</returns>
        public static string ReadString(long address, int size)
        {
            byte[] buffer = Buffers.Get<byte>();
            if (buffer.Length < size)
            {
                buffer = new byte[size];
            }

            if (!ReadProcessMemory(ProcessHandle, (IntPtr)address, buffer, size, out _))
                return string.Empty;
            
            string text = Encoding.UTF8.GetString(buffer, 0, size);
            int nullCharIndex = text.IndexOf('\x00');
            // If there's no null char in the string, just return the string itself
            if (nullCharIndex < 0)
                return text;
            // If there's a null char, return a substring
            return text.Substring(0, nullCharIndex);
        }

        /// <summary>
        /// Reads a structure from the game's memory
        /// </summary>
        /// <typeparam name="T">Type of the structure</typeparam>
        /// <param name="address">Address where the structure starts</param>
        /// <returns>The managed structure</returns>
        public static T ReadStructure<T>(long address) where T : struct
        {
            return ReadStructure<T>(address, 1)[0];
        }

        /// <summary>
        /// Reads an array of T values
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="address">Address where the array starts</param>
        /// <param name="count">Length of the array</param>
        /// <returns>Array of T</returns>
        public static T[] ReadStructure<T>(long address, int count) where T : struct
        {
            IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf<T>() * count);
            ReadProcessMemory(ProcessHandle, (IntPtr)address, buffer, Marshal.SizeOf<T>() * count, out _);
            var structures = BufferToStructures<T>(buffer, count);
            Marshal.FreeHGlobal(buffer);
            
            return structures;
        }

        public static bool Write<T>(long address, T data) where T : struct
        {
            int dataSize = Marshal.SizeOf(data);

            byte[] buffer = new byte[dataSize];

            IntPtr unmanagedPtr = Marshal.AllocHGlobal(dataSize);
            Marshal.StructureToPtr(data, unmanagedPtr, true);
            Marshal.Copy(unmanagedPtr, buffer, 0, dataSize);

            Marshal.FreeHGlobal(unmanagedPtr);
            return WriteProcessMemory(ProcessHandle, (IntPtr)address, buffer, buffer.Length, out _);
        }

        public static bool Write<T>(long address, T[] data) where T : struct
        {
            int dataSize = Marshal.SizeOf<T>() * data.Length;
            byte[] buffer = StructureToBuffer(ref data, dataSize);
            bool result = WriteProcessMemory(ProcessHandle, (IntPtr)address, buffer, buffer.Length, out _);
            return result;
        }

        /// <summary>
        /// Writes a UTF-8 string in the game's memory
        /// </summary>
        /// <param name="address">Address to write the string in</param>
        /// <param name="data">string to write</param>
        /// <returns>True if it was written succesfully</returns>
        public static bool Write(long address, string data)
        {
            if (!data.EndsWith("\x00"))
                data += "\x00";

            byte[] buffer = Encoding.UTF8.GetBytes(data);
            return WriteProcessMemory(ProcessHandle, (IntPtr)address, buffer, buffer.Length, out _);
        }

        /// <summary>
        /// Writes a byte buffer to a protected area of the memory
        /// </summary>
        /// <param name="address">Address to be written in</param>
        /// <param name="assembly">byte* to be injected</param>
        /// <returns>True if the operation was successful</returns>
        public static bool InjectAssembly(long address, byte[] assembly)
        {
            uint oldProtect;
            VirtualProtectEx(ProcessHandle, (IntPtr)address, (UIntPtr)assembly.Length, 0x40, out oldProtect);
            bool result = Write(address, assembly);
            VirtualProtectEx(ProcessHandle, (IntPtr)address, (UIntPtr)assembly.Length, oldProtect, out oldProtect);
            return result;
        }

        private static byte[] StructureToBuffer<T>(ref T[] array, int size) where T : struct
        {
            IntPtr malloced = Marshal.AllocHGlobal(size);
            byte[] buffer = new byte[size];

            for (int i = 0; i < array.Length; i++)
            {
                int offset = i * Marshal.SizeOf<T>();
                Marshal.StructureToPtr(array[i], malloced + offset, false);
            }

            Marshal.Copy(malloced, buffer, 0, size);
            Marshal.FreeHGlobal(malloced);
            return buffer;
        }

        /// <summary>
        /// Converts an unmanaged buffer to a managed structure
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="handle">Pointer to the first structure</param>
        /// <param name="count">Length</param>
        /// <returns>Array of structures</returns>
        private static T[] BufferToStructures<T>(IntPtr handle, int count)
        {
            var results = new T[count];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Marshal.PtrToStructure<T>(IntPtr.Add(handle, i * Marshal.SizeOf<T>()));
            }

            return results;
        }
    }
}
