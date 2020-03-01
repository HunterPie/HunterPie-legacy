using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Input;
using HunterPie.Core;

namespace HunterPie.Core {
    public class KeyboardHookHelper {
        /*
         Deals with the keyboard
        */
        public static readonly int  WH_KEYBOARD_LL = 0xD;

        public enum KeyboardMessage {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105
        }

        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hmod, int dwThreadId);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    }
    public class KeyboardHook {
        [StructLayout(LayoutKind.Sequential)]
        internal struct KeyboardLowLevelHookStruct {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr dwExtraInfo;
        }
        KeyboardHookHelper.HookProc KeyboardProc;
        public event EventHandler<KeyboardInputEventArgs> OnKeyboardKeyPress;

        public IntPtr KeyboardHk { get; private set; } = IntPtr.Zero;

        public KeyboardHook() {
            KeyboardProc = LowLevelKeyboardProc;
        }
        
        private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam) {
            if (nCode >= 0) {
                var st = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
                OnKeyboardKeyPress?.Invoke(this, new KeyboardInputEventArgs(st.vkCode, (KeyboardHookHelper.KeyboardMessage)wParam));
            }
            return KeyboardHookHelper.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public void InstallHooks() {
            if (KeyboardHk == IntPtr.Zero) {
                KeyboardHk = KeyboardHookHelper.SetWindowsHookEx(KeyboardHookHelper.WH_KEYBOARD_LL, KeyboardProc, IntPtr.Zero, 0);
            }
        }

        public void UninstallHooks() {
            KeyboardHookHelper.UnhookWindowsHookEx(KeyboardHk);
        }
    }
}
