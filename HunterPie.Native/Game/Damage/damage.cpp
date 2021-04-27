#pragma once
#include "damage.h"
#include "../Helpers.h"
#include "../../Connection/Logger.h"
#include "../../Connection/Socket.h"
#include "../../Connection/Packets/address_map.h"
#include "../../libs/MinHook/MinHook.h"

using namespace Game::Damage;
using namespace Connection::Packets;

uintptr_t FunDealDamage = (uintptr_t)nullptr;

void Game::Damage::HunterPie_DealDamage(
    void* target,
    int damage,
    Vec3* position,
    int isTenderized,
    int isCrit,
    int unk0,
    int unk1,
    char unk2,
    int attackId)
{

    S_DEAL_DAMAGE packet
    {
        OPCODE::DealDamage,
        1,
        (uintptr_t)target,
        damage,
        isCrit != 0,
        isTenderized != 0,
        attackId
    };


    Connection::Server::getInstance()->sendData(
        &packet,
        sizeof(packet)
    );

    // Calls original function
    originalDealDamage(
        target,
        damage,
        position,
        isTenderized,
        isCrit,
        unk0,
        unk1,
        unk2,
        attackId
    );
}

void Game::Damage::InitializeHooks()
{
    MH_CreateHook(
        (fnDealDamage)FunDealDamage,
        &HunterPie_DealDamage,
        reinterpret_cast<LPVOID*>(&originalDealDamage)
    );

}

bool Game::Damage::LoadAddress(uintptr_t ptrs[128])
{
    FunDealDamage = ptrs[FUN_DEAL_DAMAGE];
    return true;
}
