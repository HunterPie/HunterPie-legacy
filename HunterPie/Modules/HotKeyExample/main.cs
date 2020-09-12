using System;
using HunterPie;
using HunterPie.Core;
using Debugger = HunterPie.Logger.Debugger;
using System.Windows;
using System.Windows.Interop;

namespace HunterPie.Plugins
{
    public class Example : IPlugin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Game Context { get; set; }

        IntPtr hWnd;
        HwndSource source;

        public void Initialize(Game context)
        {
            Name = "HotkeyExampleModule";
            Description = "A plugin example for HunterPie";

            Context = context;

            SetHotkey();
        }

        public void Unload()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                bool success = KeyboardHookHelper.UnregisterHotKey(hWnd, 999);

                // Make sure to remove the hook, so the next time you register a hotkey it won't
                // call the callback function multiple times
                source.RemoveHook(HwndHook);
                if (success)
                {
                    Debugger.Log("Successfully unregistered hotkey");
                } else
                {
                    Debugger.Error("failed to unregister");
                }
            }));
        }

        private void SetHotkey()
        {
            // We need the current window handle to register a global hotkey
            // HunterPie is multithreaded, so we should invoke to get the main window handle
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                hWnd = new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle();
                source = HwndSource.FromHwnd(hWnd);

                // This is our "callback"
                source.AddHook(HwndHook);

                // Key modifiers
                // 0x1 = Alt; 0x2 = Ctrl; 0x4 = Shift
                int Modifiers = 0x2 | 0x4;

                // Setting the key to P, you can find the keys here: https://github.com/Haato3o/HunterPie/blob/master/HunterPie/Core/KeyboardHook.cs
                KeyboardHookHelper.KeyboardKeys key = KeyboardHookHelper.KeyboardKeys.P;

                // Hotkeys also need an id, you can choose whatever you want, just make sure it's unique
                int hotkeyId = 999;

                // Now we register the hotkey
                bool success = KeyboardHookHelper.RegisterHotKey(hWnd, hotkeyId, Modifiers, (int)key);

                if (success)
                {
                    Debugger.Log("Registered hotkey!");
                } else
                {
                    Debugger.Error("Failed to register hotkey");
                }
            }));

        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        // Check if it's our hotkey id
                        case 999: 
                            Debugger.Log("Hotkey pressed!");
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
