using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using HunterPie.Logger;
using HunterPie.Memory;
using static HunterPie.Memory.WindowsHelper;

namespace HunterPie.Core.Input
{
    public static class VirtualInput
    {
        private struct vINPUT
        {
            public char vK;
            public bool isFrameBased;
            public ulong startedAt;
            public ulong endsAt;
        };

        private static readonly List<vINPUT> injectedInputs = new List<vINPUT>();
        private static bool isPatched = false;
        private static byte[] originalOps;
        private static bool isInjecting = false;
        private static CancellationTokenSource cToken;
        private static bool isFrameCounting = false;

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
            vINPUT input = new vINPUT
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
            vINPUT input = new vINPUT
            {
                vK = code,
                isFrameBased = true,
                startedAt = frames,
                // Inputs are processed after 2 frames, but we're gonna use 4 for safety
                endsAt = frames + 4
            };
            injectedInputs.Add(input);

            PatchInputReset();

            InitializeInjector();

            await Task.Run(() =>
            {
                while (Game.ElapsedFrames < input.endsAt)
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
                List<vINPUT> removeQueue = new List<vINPUT>();
                while (injectedInputs.Count > 0)
                {
                    isInjecting = true;
                    byte[] keyboardStates = GetBitShiftedInputs();

                    foreach (vINPUT input in injectedInputs)
                    {
                        if ((!input.isFrameBased) || (input.isFrameBased && Game.ElapsedFrames < input.endsAt))
                            InjectInput(input.vK, ref keyboardStates);
                        else
                            removeQueue.Add(input);
                            
                    }

                    long inputArr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("GAME_INPUT_OFFSET"),
                        Address.GetOffsets("GameInputArrayOffsets"));

                    Kernel.Write(inputArr, keyboardStates);

                    if (removeQueue.Count > 0)
                    {
                        foreach (vINPUT input in removeQueue)
                            injectedInputs.Remove(input);
                        removeQueue.Clear();
                    }

                    Thread.Sleep((int)Game.RenderTime);
                }

            }, cToken.Token);
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
        private static byte[] GetBitShiftedInputs()
        {
            byte[] bitStates = new byte[32];
            for (byte vK = 0; vK < 0xFF; vK++)
            {
                bitStates[vK / 8] |= (byte)((byte)(GetAsyncKeyState(vK) & 0x8000) << (vK % 8));
            }
            return bitStates;
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
                // mov      [rsi], edi
                { 0x2E, 0x2 },
                // mov      [rcx+0000013C],edi
                { 0x3C, 0x6 },
                // mov      [rcx+00000140],edi
                { 0x4E, 0x6 },
                // mov      [rcx+00000144],edi
                { 0x60, 0x6 },
                // mov      [rcx+00000148],edi
                { 0x72, 0x6 },
                // mov      [rcx+0000014C],edi
                { 0x84, 0x6 },
                // mov      [rcx+00000150],edi
                { 0x96, 0x6 },
                // mov      [rcx+00000154],edi
                { 0xA8, 0x6 },
            };
            byte[] NOPs = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };
            // Now we patch these addresses with NOPs:
            foreach (var offset in offsetsToPatch)
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
