using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterPie.Native.Connection.Packets;
using HunterPie.Native.Connection;

namespace HunterPie.Core.Native
{
    public class Chat
    {
        /// <summary>
        /// Sends a string to Monster Hunter: World
        /// </summary>
        /// <param name="characters">Text to be sent (Max: 40 characters)</param>
        public static async void Say(string characters)
        {
            if (characters is null)
                return;

            if (characters.Length >= 40)
                characters = characters.Substring(0, 39);

            characters += "\x00";

            C_SEND_CHAT pkt = new C_SEND_CHAT
            {
                header = new Header { opcode = OPCODE.SendChatMessage, version = 1 },
                message = characters
            };
            await Client.ToServer(pkt);
        }
    }
}
