using HunterPie.Native.Connection.Packets;
using HunterPie.Native.Connection;
using System.Threading.Tasks;

namespace HunterPie.Core.Native
{
    public class Chat
    {
        const int MaxLength = 256;

        /// <summary>
        /// Sends a string to Monster Hunter: World
        /// </summary>
        /// <param name="message">Text to be sent (Max: 256 characters)</param>
        public static async Task<bool> Say(string message)
        {
            if (message is null)
                return false;

            if (message.Length >= MaxLength)
                message = message.Substring(0, MaxLength - 1);

            message += "\x00";

            C_SEND_CHAT pkt = new C_SEND_CHAT
            {
                header = new Header { opcode = OPCODE.SendChatMessage, version = 1 },
                message = message
            };
            return await Client.ToServer(pkt);
        }

        /// <summary>
        /// Sends a system message to the native Monster Hunter World chat
        /// </summary>
        /// <param name="message">Message to send (max: 256 characters)</param>
        /// <param name="unk1">unknown</param>
        /// <param name="unk2">unknown</param>
        /// <param name="isPurple">Whether this message background should be purple or blue</param>
        /// <returns>Awaitable task</returns>
        public static async Task<bool> SystemMessage(string message, float unk1, uint unk2, byte isPurple)
        {
            if (message is null)
                return false;

            if (message.Length >= MaxLength)
                message = message.Substring(0, MaxLength - 1);

            message += "\x00";

            C_SEND_SYSTEM_CHAT pkt = new C_SEND_SYSTEM_CHAT
            {
                header = new Header { opcode = OPCODE.SendSystemMessage, version = 1},
                message = message,
                unk1 = unk1,
                unk2 = unk2,
                unk3 = isPurple
            };

            return await Client.ToServer(pkt);
        }
    }
}
