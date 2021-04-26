#pragma once
#include <stdint.h>

namespace Game
{
    namespace Damage
    {
        // Function that draws damage on screen
        // doesn't get called when damage indicator is disabled,
        // but it's required to calculate damage in expeditions
        using fnDrawDamageOnScreen = void(*)(
            void* target,
            unsigned int index,
            char unk3,
            int damage,
            float* unk4,
            int unk5,
            unsigned int isCrit,
            int unk6,
            int isTenderized,
            char unk7
        );

        static void (*originalDrawDamageOnScreen)(
            void* target,
            unsigned int index,
            char unk3,
            int damage,
            float* unk4,
            int unk5,
            unsigned int isCrit,
            int unk6,
            int isTenderized,
            char unk7
        );

        void HunterPie_DrawDamageOnScreen(void* target,
            unsigned int index,
            char unk3,
            int damage,
            float* unk4,
            int unk5,
            unsigned int isCrit,
            int unk6,
            int isTenderized,
            char unk7
        );

        void InitializeHooks();
        bool LoadAddress(uintptr_t ptrs[128]);
    }
}
