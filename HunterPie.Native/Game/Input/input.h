#pragma once
#include <queue>

#include "../../Connection/Packets/Definitions.h"
#include "../Definitions.h"


namespace Game
{
    namespace Input
    {

        static void (*originalHandleInput)(sMhKeyboard* keyboard);

        static unsigned int keyboardInputElapsedFrames = 0;
        void HunterPie_HandleKeyboardInput(sMhKeyboard* keyboard);

        void InitializeHooks();

        static std::queue<Connection::Packets::input*> inputInjectionQueue;
    }
}
