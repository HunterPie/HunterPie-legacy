#pragma once
#include <stdint.h>
#include "../Game.h"

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

        using fnDealDamage = void(*)(
            void* target,
            int damage,
            Vec3* position,
            int isTenderized,
            int isCrit,
            int unk0,
            int unk1,
            char unk2,
            int attackId
        );

        static void (*originalDealDamage)(
            void* target,
            int damage,
            Vec3* position,
            int isTenderized,
            int isCrit,
            int unk0,
            int unk1,
            char unk2,
            int attackId
        );

        void HunterPie_DealDamage(
            void* target,
            int damage,
            Vec3* position,
            int isTenderized,
            int isCrit,
            int unk0,
            int unk1,
            char unk2,
            int attackId
        );

        void InitializeHooks();
        bool LoadAddress(uintptr_t ptrs[128]);
    }
}
