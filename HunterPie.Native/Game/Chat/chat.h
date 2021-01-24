#pragma once
#include <stdint.h>

namespace Game
{
    namespace Chat
    {
        typedef struct uGUIChat
        {
            long long* vtable_ref;
            long long unkptrs[42]; // Most of them are pointers, but there's also some unk data there
            int chatIndex;
            int unk; // Probably either padding, or chatIndex is an uint64_t
            int isTextBarVisible;
            char space;
            char chatBuffer[256];
        };

        typedef struct sChat
        {
            long long* vtable_ref;
        };
        
        using fnSendSystemMsg = void (*)(sChat* _this, char* message, float unk, unsigned int unk1, unsigned char unk2);
        
        bool SendChatMessage(char message[256]);
        bool SendSystemMessage(char message[256], float unk, unsigned int unk1, unsigned char unk2);
        bool LoadAddress(uintptr_t ptrs[128]);
    }
}
