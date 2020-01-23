using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics ;
using HunterPie.Logger;

namespace HunterPie.Memory {
    class Scanner {
        // Byte sizes
        const int INT = 4;
        const int CHAR = 1;
        const int FLOAT = 4;
        const int LONGLONG = 8;
        const int LONG = 4;

        // Process info
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const string PROCESS_NAME = "MonsterHunterWorld";
        static public int GameVersion;
        static public int PID;
        static Process[] MonsterHunter;
        static public IntPtr ProcessHandle { get; private set; } = (IntPtr)0;
        static public bool GameIsRunning = false;

        // Scanner Thread
        static private ThreadStart ScanGameMemoryRef;
        static private Thread ScanGameMemory;

        // Kernel32 DLL
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kerneel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        /* Events */
        public delegate void ProcessHandler(object source, EventArgs args);
        public static event ProcessHandler OnGameStart;
        public static event ProcessHandler OnGameClosed;

        // On Game start
        protected static void _onGameStart() {
            OnGameStart?.Invoke(typeof(Scanner), EventArgs.Empty);
        }

        // On game close
        protected static void _onGameClosed() {
            OnGameClosed?.Invoke(typeof(Scanner), EventArgs.Empty);
        }
        
        /* Core code */
        public static void StartScanning() {
            // Start scanner thread
            ScanGameMemoryRef = new ThreadStart(GetProcess);
            ScanGameMemory = new Thread(ScanGameMemoryRef);
            ScanGameMemory.Name = "Scanner_Memory";
            ScanGameMemory.Start();
        }

        public static void StopScanning() {
            if (ProcessHandle != (IntPtr)0) CloseHandle(ProcessHandle);
            ScanGameMemory.Abort();
        }

        public static void GetProcess() {
            bool lockSpam = false;
            while (true) {
                MonsterHunter = Process.GetProcessesByName(PROCESS_NAME);
                if (MonsterHunter.Length == 0) {
                    if (!lockSpam) {
                        Logger.Debugger.Log("HunterPie is ready! Waiting for game process to start...");
                        lockSpam = true;
                    }
                    if (GameIsRunning) {
                        Logger.Debugger.Log("Game process was closed by user!");
                        ProcessHandle = (IntPtr)0;
                        _onGameClosed();
                    }
                    GameIsRunning = false;
                    PID = 0;
                } else if (!GameIsRunning) {
                    while (MonsterHunter.Length == 0 || MonsterHunter[0].MainWindowTitle == "") {
                        MonsterHunter = Process.GetProcessesByName(PROCESS_NAME);
                        Thread.Sleep(100);
                    }
                    PID = MonsterHunter[0].Id;
                    ProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, false, PID);
                    GameVersion = int.Parse(MonsterHunter[0].MainWindowTitle.Split('(')[1].Trim(')'));
                    _onGameStart();
                    Logger.Debugger.Log($"MonsterHunterWorld.exe found! (PID: {PID})");
                    GameIsRunning = true;
                }
                Thread.Sleep(1000);
            }
        }

        /* Helpers */
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

        public static Int64 READ_MULTILEVEL_PTR(Int64 Base_Address, Int64[] offsets) {
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
            return Encoding.UTF8.GetString(Buffer, 0, size);
        }

        public static float READ_FLOAT(Int64 Address) {
            byte[] Buffer = new byte[FLOAT];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, FLOAT, ref bytesRead);
            return BitConverter.ToSingle(Buffer, 0);
        }
    }
}
