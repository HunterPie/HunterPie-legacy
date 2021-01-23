#pragma once

namespace Game
{
    namespace Input
    {
        typedef struct sMhKeyboard
        {
            long long vtable_ref;
            char data[0x130];
            char firstArray[32];
            char secondArray[32];
            char thirdArray[32];
            char fourthArray[32];
            char fifthArray[32];
            char sixthArray[32];
        };

        //static sMhKeyboard* keyboard = (sMhKeyboard*)0x1452258F8;

        static void (*HandleKeyboardInput)(long long keyboard) = (void(*)(long long))0x142303E30;
    }

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
            long long vtable_ref;
        };

        static long long* uGuiChatBase = (long long*)0x145224B80;
        static sChat** sChatBase = (sChat**)0x14506D340;
        static void (*SendSystem)(sChat* _this, char* message, float unk, unsigned int unk1, unsigned char unk2) = (void(*)(sChat*, char*, float, unsigned int, unsigned char))0x141A671F0;
    }
}
