#pragma once
#include "chat.h"
#include "../Helpers.h"
#include "../../Connection/Logger.h"
#include "../../Connection/Packets/address_map.h"

using namespace Game::Chat;

sChat** sChatBase = (sChat**)nullptr;
long long* uGuiChatBase = (long long*)nullptr;
uintptr_t SendSystemMsg = (uintptr_t)nullptr;

bool Game::Chat::LoadAddress(uintptr_t ptrs[128])
{
    sChatBase = (sChat**)ptrs[GAME_CHAT_OFFSET];
    uGuiChatBase = (long long*)ptrs[GAME_HUD_INFO_OFFSET];
    SendSystemMsg = ptrs[FUN_CHAT_SYSTEM];
    return true;
}

bool Game::Chat::SendChatMessage(char message[256])
{
    uGUIChat* chat = resolvePtrs<uGUIChat>(uGuiChatBase, { 0x13FD0, 0x28F8 });
    bool* sendMessage = resolvePtrs<bool>(uGuiChatBase, { 0x13FD0, 0x325E });

    LOG("%8X, %8X\n", chat, sendMessage);

    memcpy(chat->chatBuffer, message, 256);
    *sendMessage = true;

    return true;
}

bool Game::Chat::SendSystemMessage(char message[256], float unk, unsigned int unk1, unsigned char unk2)
{
    ((fnSendSystemMsg)SendSystemMsg)(*sChatBase, message, unk, unk1, unk2);
    return true;
}
