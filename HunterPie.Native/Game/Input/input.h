#pragma once
#include <queue>
#include <stdint.h>
#include "../../Connection/Packets/Definitions.h"

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

        using fnHandleKeyboard = void (*)(sMhKeyboard* keyboard);

        static void (*originalHandleInput)(sMhKeyboard* keyboard);

        static unsigned int keyboardInputElapsedFrames = 0;
        void HunterPie_HandleKeyboardInput(sMhKeyboard* keyboard);

        void InitializeHooks();
        bool LoadAddress(uintptr_t ptrs[128]);

        static std::queue<Connection::Packets::input*> inputInjectionQueue;
    }
}
