using HunterPie.Native.Connection.Packets;
using HunterPie.Native.Connection;

namespace HunterPie.Core.Native
{
    public class Chat
    {
        const int MaxLength = 256;

        /// <summary>
        /// Sends a string to Monster Hunter: World
        /// </summary>
        /// <param name="message">Text to be sent (Max: 256 characters)</param>
        public static async void Say(string message)
        {
            if (message is null)
                return;

            if (message.Length >= MaxLength)
                message = message.Substring(0, MaxLength - 1);

            message += "\x00";

            C_SEND_CHAT pkt = new C_SEND_CHAT
            {
                header = new Header { opcode = OPCODE.SendChatMessage, version = 1 },
                message = message
            };
            await Client.ToServer(pkt);
        }

        public static async void SystemMessage(string message, float unk1, uint unk2, byte unk3)
        {
            if (message is null)
                return;

            if (message.Length >= MaxLength)
                message = message.Substring(0, MaxLength - 1);

            message += "\x00";

            C_SEND_SYSTEM_CHAT pkt = new C_SEND_SYSTEM_CHAT
            {
                header = new Header { opcode = OPCODE.SendSystemMessage, version = 1},
                message = message,
                unk1 = unk1,
                unk2 = unk2,
                unk3 = unk3
            };

            await Client.ToServer(pkt);
        }
    }
}
