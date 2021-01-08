using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HunterPie.Logger;
using HunterPie.Memory;
using static HunterPie.Memory.WindowsHelper;
// ReSharper disable All

namespace HunterPie.Core.Input
{
    public static class VirtualInput
    {
        private struct VInput
        {
            public char vK;
            public bool isFrameBased;
            public ulong startedAt;
            public ulong endsAt;
        };

        private static readonly List<VInput> injectedInputs = new List<VInput>();
        private static bool isPatched = false;
        private static byte[] originalOps;
        private static byte[] originalForegroundOps;
        private static bool isInjectingForeground;
        private static bool isInjecting = false;
        private static CancellationTokenSource cToken;

        [DllImport("user32.dll")]
        private static extern int SendMessage(
            IntPtr hWnd,
            WMessages wMsg,
            char wParam,
            IntPtr lParam
            );

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(byte vKey);

        /// <summary>
        /// Sends a string to Monster Hunter: World
        /// </summary>
        /// <param name="characters">Text to be sent</param>
        public static void SendText(string characters)
        {
            if (characters is null)
                return;
            
            foreach (char c in characters)
                SendMessage(Kernel.WindowHandle, WMessages.WM_CHAR, c, IntPtr.Zero);
        }

        /// <summary>
        /// Sends input to the game and keeps it pressed for the specified time duration
        /// </summary>
        /// <param name="code">Virtual Key Code</param>
        /// <param name="duration">How long to keep the key pressed</param>
        /// <returns>True when the operation is completed</returns>
        public static async Task<bool> HoldInputAsync(char code, TimeSpan duration)
        {
            VInput input = new VInput
            {
                vK = code,
                isFrameBased = false,
                endsAt = (ulong)duration.TotalMilliseconds
            };
            // Adds our vk to the input list
            injectedInputs.Add(input);

            // Patch the input reset operations
            PatchInputReset();

            // Initialize our input injector to update the input array every 16ms
            InitializeInjector();

            await Task.Delay(duration).ContinueWith((_) =>
            {
                lock (injectedInputs)
                    injectedInputs.Remove(input);
            });

            return true;
        }

        public static async Task<bool> PressInputAsync(char code)
        {
            ulong frames = Game.ElapsedFrames;
            VInput input = new VInput
            {
                vK = code,
                isFrameBased = true,
                startedAt = frames,
                endsAt = frames + 1
            };
            
            injectedInputs.Add(input);
            PatchInputReset();

            InitializeInjector();

            await Task.Run(() =>
            {
                while (Game.ElapsedFrames < input.endsAt + 10)
                    Thread.Sleep((int)Game.RenderTime);
            });

            return true;
        }

        private static async Task InitializeInjector()
        {
            if (isInjecting)
                return;

            cToken = new CancellationTokenSource();
            
            await Task.Factory.StartNew(() =>
            {
                byte[] keyboardStates = new byte[32];
                ulong frames = 0;
                List<VInput> removeQueue = new List<VInput>();

                long inputArr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("GAME_INPUT_OFFSET"),
                        Address.GetOffsets("GameInputArrayOffsets"));

                PatchIsWindowForeground();

                while (injectedInputs.Count > 0)
                {
                    if ((Game.ElapsedFrames - frames) < 2)
                        continue;

                    frames = Game.ElapsedFrames;

                    isInjecting = true;
                    
                    GetBitShiftedInputs(ref keyboardStates);

                    Kernel.Write(inputArr + 0x20, keyboardStates);

                    for (int i = 0; i < injectedInputs.Count; i++)
                    {
                        VInput input = injectedInputs[i];
                        if ((!input.isFrameBased) || (input.isFrameBased && (frames - input.startedAt) < 2))
                            InjectInput(input.vK, ref keyboardStates);
                        else
                            removeQueue.Add(input);

                    }

                    Kernel.Write(inputArr, keyboardStates);
                    
                    if (removeQueue.Count > 0)
                    {
                        foreach (VInput input in removeQueue)
                            injectedInputs.Remove(input);
                    }
                    
                    Thread.Sleep(1);
                    
                }

            }, cToken.Token);
            UnpatchIsWindowForeground();
            RestoreInputReset();
        }


        /// <summary>
        /// Injects a input in the inputs array
        /// </summary>
        /// <param name="code">Virtual key code</param>
        /// <param name="bInputs">byte array with the GetAsyncKeyState states</param>
        private static void InjectInput(char code, ref byte[] bInputs)
        {
            int idx = code / 8;
            byte bitValue = (byte)(1 << (code % 8));
            bInputs[idx] |= bitValue;
        }

        /// The way how MHW handles input is by having a 32 bytes (256 bits) array
        /// and OR'ing the keys to see if they're pressed or not.
        private static void GetBitShiftedInputs(ref byte[] bitStates)
        {
            for (byte vK = 0; vK < 0xFF; vK++)
            {
                bitStates[vK / 8] |= (byte)((byte)(GetAsyncKeyState(vK) & 0x8000) << (vK % 8));
            }
        }

        /// This is required for inputs that need the window to be focused, and should be patched only if the player
        /// HUD is actually open
        private static void PatchIsWindowForeground()
        {

            if (isInjectingForeground)
                return;

            // mov      [rax+000029AD],sil ; That's what controls whether the game window is focused
            short instructionsOffset = 0x120;
            short instructionsLength = 0x7;

            long foregroundFunAddress = Address.GetAddress("BASE") + Address.GetAddress("FUN_GAME_WINDOW");

            originalForegroundOps = Kernel.ReadStructure<byte>(
                foregroundFunAddress + instructionsOffset,
                instructionsLength
            );

            byte[] NOPs = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };

            if (Kernel.Write(foregroundFunAddress + instructionsOffset, NOPs))
            {
                isInjectingForeground = true;
                long isForegroundFlagPtr = Kernel.Read<long>(Address.GetAddress("BASE") + Address.GetAddress("GAME_WINDOW_INFO_OFFSET"));

                Kernel.Write<byte>(isForegroundFlagPtr + 0x29AD, 0x1);
            } else
            {
                isInjectingForeground = false;
                Debugger.Error("Failed to patch IsForegroundWindow");
            }
        }

        private static void UnpatchIsWindowForeground()
        {
            if (!isInjectingForeground)
                return;

            short instructionsOffset = 0x120;

            long foregroundFunAddress = Address.GetAddress("BASE") + Address.GetAddress("FUN_GAME_WINDOW");

            Kernel.Write(foregroundFunAddress + instructionsOffset, originalForegroundOps);
            long isForegroundFlagPtr = Kernel.Read<long>(Address.GetAddress("BASE") + Address.GetAddress("GAME_WINDOW_INFO_OFFSET"));

            Kernel.Write<byte>(isForegroundFlagPtr + 0x29AD, 0x0);
            isInjectingForeground = false;
        }

        /// Before handling the inputs, MHW also resets their values to default before calling GetAsyncKeyState
        /// This function simply patches it so our injected array don't get overwritten by the game's GetAsyncKeyState
        private static void PatchInputReset()
        {
            if (isPatched)
                return;

            long addr = Address.GetAddress("BASE") + Address.GetAddress("FUN_GAME_INPUT");
            originalOps = Kernel.ReadStructure<byte>(addr, 174);

            byte[] patchedOps = new byte[originalOps.Length];
            Array.Copy(originalOps, patchedOps, patchedOps.Length);

            Dictionary<short, short> offsetsToPatch = new Dictionary<short, short>()
            {
                // mov      [rcx+00000158],eax
                { 0x28, 0x6 },
                // mov      [rsi], edi
                { 0x2E, 0x2 },
                // mov      [rcx+0000015C],eax
                { 0x36, 0x6 },
                // mov      [rcx+0000013C],edi
                { 0x3C, 0x6 },
                // mov      [rcx+00000160],eax
                { 0x48, 0x6 },
                // mov      [rcx+00000140],edi
                { 0x4E, 0x6 },
                // mov      [rcx+00000164],eax
                { 0x5A, 0x6 },
                // mov      [rcx+00000144],edi
                { 0x60, 0x6 },
                // mov      [rcx+00000168],eax
                { 0x6C, 0x6 },
                // mov      [rcx+00000148],edi
                { 0x72, 0x6 },
                // mov      [rcx+0000016C],eax
                { 0x7E, 0x6 },
                // mov      [rcx+0000014C],edi
                { 0x84, 0x6 },
                // mov      [rcx+00000170],eax
                { 0x90, 0x6 },
                // mov      [rcx+00000150],edi
                { 0x96, 0x6 },
                // mov      [rcx+00000154],edi
                { 0xA2, 0x6 },
                // mov      [rcx+00000154],edi
                { 0xA8, 0x6 },
            };
            byte[] NOPs = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
            // Now we patch these addresses with NOPs:
            foreach (KeyValuePair<short, short> offset in offsetsToPatch)
                Array.ConstrainedCopy(NOPs, 0, patchedOps, offset.Key, offset.Value);

            Kernel.Write(addr, patchedOps);

            isPatched = true;
        }

        private static void RestoreInputReset()
        {
            if (injectedInputs.Count == 0 && isPatched)
            {
                long addr = Address.GetAddress("BASE") + Address.GetAddress("FUN_GAME_INPUT");
                Kernel.Write(addr, originalOps);

                isPatched = false;
                isInjecting = false;
            }
        }

        internal static void ForceUnPatch()
        {
            try
            {
                if (isInjectingForeground)
                    UnpatchIsWindowForeground();

                if (originalOps is null || !isPatched)
                    return;

                long addr = Address.GetAddress("BASE") + Address.GetAddress("FUN_GAME_INPUT");

                Kernel.Write(addr, originalOps);

                isPatched = false;

                cToken.Cancel();

                isInjecting = false;
            } catch(Exception err)
            {
                Debugger.Error(err);
            }
        }
    }
}
