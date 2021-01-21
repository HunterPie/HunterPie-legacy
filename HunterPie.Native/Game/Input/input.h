#pragma once
#include <queue>

#include "../../Connection/Packets/Definitions.h"
#include "../Definitions.h"


namespace Game
{
    namespace Input
    {

        static void (*originalHandleInput)(long long keyboard);

        static unsigned int keyboardInputElapsedFrames = 0;
        void HunterPie_HandleKeyboardInput(long long keyboard);

        void InitializeHooks();

        static std::queue<Connection::Packets::input*> inputInjectionQueue;
    }
}
