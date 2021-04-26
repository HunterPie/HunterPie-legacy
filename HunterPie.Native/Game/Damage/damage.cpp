#pragma once
#include "damage.h"
#include "../Helpers.h"
#include "../../libs/MinHook/MinHook.h"
#include "../../Connection/Logger.h"
#include "../../Connection/Socket.h"
#include "../../Connection/Packets/address_map.h"

using namespace Game::Damage;
using namespace Connection::Packets;

uintptr_t DrawDamageOnScreen = (uintptr_t)nullptr;

void Game::Damage::HunterPie_DrawDamageOnScreen(
    void* target,
    unsigned int index,
    char unk3,
    int damage,
    float* unk4,
    int unk5,
    unsigned int isCrit,
    int unk6,
    int isTenderized,
    char unk7)
{

    S_DEAL_DAMAGE packet
    {
        OPCODE::SendDamage,
        1,
        (uintptr_t)target,
        index,
        damage,
        isCrit != 0,
        isTenderized != 0
    };


    Connection::Server::getInstance()->sendData(
        &packet,
        sizeof(packet)
    );

    // Calls original function
    originalDrawDamageOnScreen(
        target,
        index,
        unk3,
        damage,
        unk4,
        unk5,
        isCrit,
        unk6,
        isTenderized,
        unk7
    );
}

void Game::Damage::InitializeHooks()
{
    MH_CreateHook(
        (fnDrawDamageOnScreen)DrawDamageOnScreen,
        &HunterPie_DrawDamageOnScreen,
        reinterpret_cast<LPVOID*>(&originalDrawDamageOnScreen)
    );

}

bool Game::Damage::LoadAddress(uintptr_t ptrs[128])
{
    DrawDamageOnScreen = ptrs[FUN_DRAW_DAMAGE];
    return true;
}
