using System;
using System.Threading.Tasks;
using HunterPie.Memory;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;
using static HunterPie.Memory.WindowsHelper;
// ReSharper disable All

namespace HunterPie.Core.Input
{
    public static class VirtualInput
    {
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

        public static async Task PressKey(char key)
        {
            C_QUEUE_INPUT packet = new C_QUEUE_INPUT()
            {
                header = new Header() { opcode = OPCODE.QueueInput, version = 1},
                inputs = new input()
                {
                    inputArray = CalculateInputs(new []{ key }),
                    nFrames = 1,
                    injectionId = 69,
                    ignoreOriginalInputs = true
                }
            };

            await Client.ToServer(packet);
        }

        private static char[] CalculateInputs(char[] keys)
        {
            char[] bits = new char[32];
            string uwu = "";
            foreach (char key in keys)
            {
                bits[key / 8] |= (char)(1 << (key % 8));
            }
            foreach (char b in bits)
                uwu += $"{(int)b:X02} ";

            Logger.Debugger.Log(uwu);
            return bits;
        }

    }
}
