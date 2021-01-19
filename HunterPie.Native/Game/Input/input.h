#pragma once
#include "../../libs/MinHook/MinHook.h"
#include "../Definitions.h"
#include <queue>

namespace Game
{
    namespace Input
    {
        struct input
        {
            char inputArray[32];
            int nFrames;
            unsigned int injectionId;
            bool ignoreOriginalInputs;
        };

        static void (*originalHandleInput)(sMhKeyboard* keyboard);

        static unsigned int keyboardInputElapsedFrames = 0;
        void HunterPie_HandleKeyboardInput(sMhKeyboard* keyboard);

        static std::queue<input*> inputInjectionToQueue;
        static std::queue<input*> inputInjectionQueue;
    }
}
