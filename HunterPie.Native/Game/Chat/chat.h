#pragma once
#include "../Definitions.h"

namespace Game
{
    namespace Chat
    {
        bool SendChatMessage(char message[256]);
        bool SendSystemMessage(char message[256], float unk, unsigned int unk1, unsigned char unk2);
    }
}
