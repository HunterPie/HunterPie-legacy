#pragma once

namespace Connection
{
    namespace Packets
    {
        enum OPCODE
        {
            None,
            Connect,
            Disconnect,
            EnableHooks,
            DisableHooks,
            QueueInput,
            SendChatMessage,
            SendSystemMessage,
            DealDamage,
            InterruptInput,
            LogMessage
        };

        typedef struct header
        {
            OPCODE opcode;
            unsigned int version;
        };

        typedef struct I_PACKET
        {
            header header;
        };
    }
}
