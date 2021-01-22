#pragma once
#include "chat.h"
#include "../Helpers.h"
#include "../../Connection/Socket.h"

using namespace Game::Chat;

bool Game::Chat::SendChatMessage(char message[40])
{
    uGUIChat* chat = resolvePtrs<uGUIChat>(ChatBase, { 0x13FD0, 0x28F8 });
    bool* sendMessage = resolvePtrs<bool>(ChatBase, { 0x13FD0, 0x325E });

    LOG("%8X, %8X\n", chat, sendMessage);

    memcpy(chat->chatBuffer, message, 40);
    *sendMessage = true;

    return true;
}
