using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics ;
using System.Linq;

namespace HunterPie.Memory {
    class Scanner {
        // Byte sizes
        const int INT = 4;
        const int CHAR = 1;
        const int FLOAT = 4;
        const int LONGLONG = 8;
        const int LONG = 4;

        // Process info
        const int PROCESS_VM_READ = 0x0010;
        const string PROCESS_NAME = "MonsterHunterWorld";
        static public IntPtr WindowHandle { get; private set; }
        static public int GameVersion;
        static public int PID;
        static Process MonsterHunter;
        static public IntPtr ProcessHandle { get; private set; } = (IntPtr)0;
        static public bool GameIsRunning = false;
        static private bool _isForegroundWindow = false;
        static public bool IsForegroundWindow {
            get { return _isForegroundWindow; }
            private set {
                if (value != _isForegroundWindow) {
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

        /* Events */
        public delegate void ProcessHandler(object source, EventArgs args);
        public static event ProcessHandler OnGameStart;
        public static event ProcessHandler OnGameClosed;
        public static event ProcessHandler OnGameFocus;
        public static event ProcessHandler OnGameUnfocus;

        // On Game start
        protected static void _onGameStart() {
            OnGameStart?.Invoke(typeof(Scanner), EventArgs.Empty);
        }

        // On game close
        protected static void _onGameClosed() {
            OnGameClosed?.Invoke(typeof(Scanner), EventArgs.Empty);
        }
        
        protected static void _onGameFocus() {
            OnGameFocus?.Invoke(typeof(Scanner), EventArgs.Empty);
        }

        protected static void _onGameUnfocus() {
            OnGameUnfocus?.Invoke(typeof(Scanner), EventArgs.Empty);
        }

        /* Core code */
        public static void StartScanning() {
            // Start scanner thread
            ScanGameMemoryRef = new ThreadStart(GetProcess);
            ScanGameMemory = new Thread(ScanGameMemoryRef) {
                Name = "Scanner_Memory"
            };
            ScanGameMemory.Start();
        }

        public static void StopScanning() {
            if (ProcessHandle != (IntPtr)0) CloseHandle(ProcessHandle);
            ScanGameMemory?.Abort();
        }

        public static void GetProcess() {
            bool lockSpam = false;
            while (true) {
                MonsterHunter = Process.GetProcessesByName(PROCESS_NAME).FirstOrDefault();
                if (MonsterHunter == null) {
                    if (!lockSpam) {
                        Logger.Debugger.Log("HunterPie is ready! Waiting for game process to start...");
                        lockSpam = true;
                    }
                    if (GameIsRunning) {
                        Logger.Debugger.Log("Game process was closed by user!");
                        CloseHandle(ProcessHandle);
                        ProcessHandle = IntPtr.Zero;
                        _onGameClosed();
                    }
                    GameIsRunning = false;
                    PID = 0;
                } else if (!GameIsRunning || PID != MonsterHunter?.Id) {
                    while (MonsterHunter == null || MonsterHunter?.MainWindowTitle == "") {
                        MonsterHunter = Process.GetProcessesByName(PROCESS_NAME).FirstOrDefault();
                        Thread.Sleep(1000);
                    }
                    if (PID != 0 && PID != MonsterHunter.Id) {
                        _onGameClosed();
                    }
                    PID = MonsterHunter.Id;
                    ProcessHandle = OpenProcess(PROCESS_VM_READ, false, PID);
                    if (ProcessHandle == (IntPtr)0) {
                        Logger.Debugger.Error("Failed to open game process. Try running HunterPie as administrator.");
                        return;
                    }
                    GameVersion = int.Parse(MonsterHunter.MainWindowTitle.Split('(')[1].Trim(')'));
                    WindowHandle = MonsterHunter.MainWindowHandle;
                    _onGameStart();
                    Logger.Debugger.Log($"MonsterHunterWorld.exe found! (PID: {PID})");
                    GameIsRunning = true;
                }
                if (GameIsRunning) {
                    IsForegroundWindow = GetForegroundWindow() == WindowHandle;
                }
                MonsterHunter?.Dispose();
                Thread.Sleep(1000);
            }
        }

        /* Helpers */
        public static byte READ_BYTE(Int64 Address) {
            int bytesRead = 0;
            byte[] Buffer = new byte[1];
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, 1, ref bytesRead);
            return Buffer[0];
        }

        public static int READ_INT(Int64 Address) {
            int bytesRead = 0;
            byte[] Buffer = new byte[INT];
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, INT, ref bytesRead);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public static Int64 READ_LONGLONG(Int64 Address) {
            int bytesRead = 0;
            byte[] Buffer = new byte[LONGLONG];
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, LONGLONG, ref bytesRead);
            return BitConverter.ToInt64(Buffer, 0);
        }

        public static Int64 READ_MULTILEVEL_PTR(Int64 Base_Address, int[] offsets) {
            Int64 Address = READ_LONGLONG(Base_Address);
            for (int offsetIndex = 0; offsetIndex < offsets.Length - 1; offsetIndex++) {
                Address = READ_LONGLONG(Address + offsets[offsetIndex]);
            }
            Address = Address + offsets[offsets.Length - 1];
            return Address;
        }

        public static string READ_STRING(Int64 Address, int size) {
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

        public static float READ_FLOAT(Int64 Address) {
            byte[] Buffer = new byte[FLOAT];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, FLOAT, ref bytesRead);
            return BitConverter.ToSingle(Buffer, 0);
        }
    }
}
