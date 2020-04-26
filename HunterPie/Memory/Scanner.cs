using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Debugger = HunterPie.Logger.Debugger;

namespace HunterPie.Memory
{
    class Scanner
    {
        const int LATEST_GAME_VERSION = 410014;

        // Process info
        const int PROCESS_VM_READ = 0x0010;
        const string PROCESS_NAME = "MonsterHunterWorld";
        public static IntPtr WindowHandle { get; private set; }
        public static int GameVersion;
        public static int PID;
        static Process MonsterHunter;
        public static IntPtr ProcessHandle { get; private set; } = (IntPtr)0;
        public static bool GameIsRunning = false;
        private static bool _isForegroundWindow = false;
        public static bool IsForegroundWindow
        {
            get => _isForegroundWindow;
            private set
            {
                if (value != _isForegroundWindow)
                {
                    // Wait until there's a subscriber to dispatch the event
                    if (OnGameFocus == null || OnGameUnfocus == null) return;
                    _isForegroundWindow = value;
                    if (_isForegroundWindow) { _onGameFocus(); }
                    else { _onGameUnfocus(); }
                }
            }
        }

        // Scanner Thread
        static private ThreadStart ScanGameMemoryRef;
        static private Thread ScanGameMemory;

        // Kernel32 DLL
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        // user32 DLL
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int style);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        /* DPI Awareness */
        public enum PROCESS_DPI_AWARENESS
        {
            PROCESS_DPI_UNAWARE = 0,
            PROCESS_SYSTEM_DPI_AWARE = 1,
            PROCESS_PER_MONITOR_DPI_AWARE = 2
        }

        public enum DPI_AWARENESS_CONTEXT
        {
            DPI_AWARENESS_CONTEXT_UNAWARE = 16,
            DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
            DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 34
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        [DllImport("SHCore.dll", SetLastError = true)]
        public static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        /* Events */
        public delegate void ProcessHandler(object source, EventArgs args);
        public static event ProcessHandler OnGameStart;
        public static event ProcessHandler OnGameClosed;
        public static event ProcessHandler OnGameFocus;
        public static event ProcessHandler OnGameUnfocus;

        // On Game start
        protected static void _onGameStart() => OnGameStart?.Invoke(typeof(Scanner), EventArgs.Empty);

        // On game close
        protected static void _onGameClosed() => OnGameClosed?.Invoke(typeof(Scanner), EventArgs.Empty);

        protected static void _onGameFocus() => OnGameFocus?.Invoke(typeof(Scanner), EventArgs.Empty);

        protected static void _onGameUnfocus() => OnGameUnfocus?.Invoke(typeof(Scanner), EventArgs.Empty);

        /* Core code */
        public static void StartScanning()
        {
            // Start scanner thread
            ScanGameMemoryRef = new ThreadStart(GetMonsterHunterProcess);
            ScanGameMemory = new Thread(ScanGameMemoryRef)
            {
                Name = "Scanner_Memory"
            };
            ScanGameMemory.Start();
        }

        public static void StopScanning()
        {
            if (ProcessHandle != (IntPtr)0) CloseHandle(ProcessHandle);
            ScanGameMemory?.Abort();
        }

        public static void GetMonsterHunterProcess()
        {
            bool lockSpam = false;
            while (true)
            {
                if (GameIsRunning)
                {
                    IsForegroundWindow = GetForegroundWindow() == WindowHandle;
                }
                if (MonsterHunter != null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                Process MonsterHunterProcess = Process.GetProcessesByName(PROCESS_NAME).FirstOrDefault();
                // If there's no MHW instance of Monster Hunter: World running
                if (MonsterHunterProcess == null)
                {
                    if (!lockSpam)
                    {
                        Debugger.Log("HunterPie is ready! Waiting for game process to start...");
                        lockSpam = true;
                    }
                    GameIsRunning = false;
                    PID = 0;
                } else
                {
                    if (string.IsNullOrEmpty(MonsterHunterProcess.MainWindowTitle))
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    MonsterHunter = MonsterHunterProcess;

                    PID = MonsterHunter.Id;
                    ProcessHandle = OpenProcess(PROCESS_VM_READ, false, PID);

                    // Check if OpenProcess was successful
                    if (ProcessHandle == IntPtr.Zero)
                    {
                        Debugger.Error("Failed to open game process. Run HunterPie as Administrator!");
                        return;
                    }

                    try
                    {
                        GameVersion = int.Parse(MonsterHunter.MainWindowTitle.Split('(')[1].Trim(')'));
                    } catch(Exception err)
                    {
                        Debugger.Error($"{err}\nFailed to get Monster Hunter: World build version. Loading latest map version instead.");
                        GameVersion = LATEST_GAME_VERSION;
                    }
                    MonsterHunter.EnableRaisingEvents = true;
                    MonsterHunter.Exited += OnGameProcessExit;
                    WindowHandle = MonsterHunter.MainWindowHandle;
                    _onGameStart();
                    Debugger.Log($"Monster Hunter: World ({GameVersion}) found! (PID: {PID})");
                    GameIsRunning = true;
                }
                
                Thread.Sleep(1000);
            }
        }

        private static void OnGameProcessExit(object sender, EventArgs e)
        {
            MonsterHunter.Exited -= OnGameProcessExit;
            MonsterHunter.Dispose();
            MonsterHunter = null;
            Debugger.Log("Game process closed!");
            CloseHandle(ProcessHandle);
            CloseHandle(WindowHandle);
            ProcessHandle = IntPtr.Zero;
            _onGameClosed();
        }

        /* Helpers */
        public static T Read<T>(Int64 Address) where T: struct
        {
            int bytesRead = 0;
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, buffer, Marshal.SizeOf(typeof(T)), ref bytesRead);
            return BufferToStructure<T>(buffer);
        }

        private static T BufferToStructure<T>(byte[] buffer) where T: struct
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally { handle.Free(); }
        }
        
        public static Int64 READ_MULTILEVEL_PTR(Int64 Base_Address, int[] offsets)
        {
            Int64 Address = Read<long>(Base_Address);
            for (int offsetIndex = 0; offsetIndex < offsets.Length - 1; offsetIndex++)
            {
                Address = Read<long>(Address + offsets[offsetIndex]);
            }
            Address = Address + offsets[offsets.Length - 1];
            return Address;
        }

        public static string READ_STRING(Int64 Address, int size)
        {
            byte[] Buffer = new byte[size];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, size, ref bytesRead);
            string String = Encoding.UTF8.GetString(Buffer, 0, size);
            int nullCharIndex = String.IndexOf('\x00');
            // If there's no null char in the string, just return the string itself
            if (nullCharIndex < 0) return String;
            // If there's a null char, return a substring
            else { return String.Substring(0, String.IndexOf('\x00')); }
        }
    }
}
