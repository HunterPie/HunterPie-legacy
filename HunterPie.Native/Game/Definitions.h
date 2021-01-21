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
}
