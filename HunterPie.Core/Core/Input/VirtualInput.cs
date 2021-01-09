using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HunterPie.Core.Definitions;
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
            public bool Tap;
            public ulong startedAt;
            public ulong endsAt;
        };

        private static readonly List<VInput> injectedInputs = new List<VInput>();
        private static bool isPatched = false;
        private static byte[] originalOps;
        private static bool isInjecting = false;
        private static bool ignoreKeyboardInput;

        private static char confirmKey
        {
            get
            {
                sKeyConfig key = PlayerKeyboard.Get(MenuControls.Confirm);
                return (char)Math.Max(key.MainKey, key.SubKey);
            }
        }

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
        /// <param name="vK">Virtual Key Code</param>
        /// <param name="duration">How long to keep the key pressed</param>
        /// <returns>True when the operation is completed</returns>
        public static async Task<bool> HoldInputAsync(char vK, TimeSpan duration)
        {
            VInput input = new VInput
            {
                vK = vK,
                isFrameBased = false,
                endsAt = (ulong)duration.TotalMilliseconds
            };

            InjectRawInput(input);

            await Task.Delay(duration).ContinueWith((_) =>
            {
                lock (injectedInputs)
                    injectedInputs.Remove(input);
            });

            return true;
        }

        /// <summary>
        /// Injects input into the game for 1 frame
        /// </summary>
        /// <param name="vK">Key code</param>
        /// <returns>True when the operation is done</returns>
        public static async Task<bool> PressInputAsync(char vK)
        {
            ulong frames = Game.ElapsedFrames;
            
            VInput input = new VInput
            {
                vK = vK,
                isFrameBased = true,
                startedAt = frames,
                endsAt = frames + 1
            };

            return await InjectRawInput(input);
        }

        /// <summary>
        /// Injects input into the game for 2 frames
        /// </summary>
        /// <param name="vK">Key code</param>
        /// <returns>True when the operation is done</returns>
        public static async Task<bool> TapInputAsync(char vK)
        {
            ulong frames = Game.ElapsedFrames;
            
            VInput input = new VInput
            {
                vK = vK,
                isFrameBased = true,
                startedAt = frames,
                Tap = true,
                endsAt = frames + 2
            };

            return await InjectRawInput(input);
        }


        /// <summary>
        /// Sends a confirmation to the game to accept dialogs.
        /// </summary>
        /// <returns>True when the operation is done</returns>
        public static async Task<bool> SendConfirm()
        {
            ulong frames = Game.ElapsedFrames;
            VInput confirmInput = new VInput()
            {
                vK = confirmKey,
                isFrameBased = true,
                startedAt = frames,
                Tap = true,
                endsAt = frames + 2
            };

            return await InjectRawInput(confirmInput);
        }

        /// <summary>
        /// Sets the Ignore real keyboard input flag, when set to False, HunterPie will not care about
        /// the real keyboard input, only the injected ones. This is useful when Alt+Tabbing out of the game
        /// and letting HunterPie do the job in-game.
        /// </summary>
        /// <param name="newState">New state for the flag</param>
        public static void SetIgnoreKeyboard(bool newState)
        {
            ignoreKeyboardInput = newState;
        }

        private static async Task<bool> InjectRawInput(VInput input)
        {
            
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
                GetBitShiftedInputs(ref keyboardStates);

                byte[] lastKeyboardState = new byte[32];

                ulong frames = 0;

                List<VInput> removeQueue = new List<VInput>();

                long inputArr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("GAME_INPUT_OFFSET"),
                        Address.GetOffsets("GameInputArrayOffsets"));
                
                while (injectedInputs.Any())
                {
                    if ((Game.ElapsedFrames - frames) < 1)
                        continue;

                    isInjecting = true;
                    
                    // Confirmation keys take 2 frames to be processed
                    if ((Game.ElapsedFrames - frames) < 2)
                    {
                        foreach (VInput input in injectedInputs.Where(i => i.Tap))
                        if (IsKeyDown(ref keyboardStates, input.vK))
                            RemoveInput(input.vK, ref keyboardStates);
                    }

                    Array.Copy(keyboardStates, lastKeyboardState, keyboardStates.Length);

                    Kernel.Write(inputArr + 0x20, lastKeyboardState);

                    frames = Game.ElapsedFrames;

                    GetBitShiftedInputs(ref keyboardStates);

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
            RestoreInputReset();
        }

        /// <summary>
        /// Injects a input in the inputs array
        /// </summary>
        /// <param name="vK">Virtual key code</param>
        /// <param name="bInputs">byte array with the GetAsyncKeyState states</param>
        private static void InjectInput(char vK, ref byte[] bInputs)
        {
            int idx = vK / 8;
            byte bitValue = (byte)(1 << (vK % 8));
            bInputs[idx] |= bitValue;
        }

        /// The way how MHW handles input is by having a 32 bytes (256 bits) array
        /// and OR'ing the keys to see if they're pressed or not.
        private static void GetBitShiftedInputs(ref byte[] bInputs)
        {
            if (ignoreKeyboardInput)
            {
                for (int i = 0; i < bInputs.Length; i++)
                    bInputs[i] = 0;

                return;
            }

            for (byte vK = 0; vK < 0xFF; vK++)
            {
                bInputs[vK / 8] |= (byte)((byte)(GetAsyncKeyState(vK) & 0x8000) << (vK % 8));
            }
        }

        private static void RemoveInput(char vK, ref byte[] bInputs)
        {
            bInputs[vK / 8] &= (byte)(~(1 << (byte)(vK % 8)));
        }

        private static bool IsKeyDown(ref byte[] bInputs, char vK)
        {
            byte extracted = bInputs[vK / 8];
            return (extracted &= (byte)(1 << (vK % 8))) != 0;
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
