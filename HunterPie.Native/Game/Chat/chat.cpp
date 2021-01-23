#pragma once
#include "chat.h"
#include "../Helpers.h"
#include "../../Connection/Socket.h"

using namespace Game::Chat;

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
    Game::Chat::SendSystem(*sChatBase, message, unk, unk1, unk2);
    return true;
}
