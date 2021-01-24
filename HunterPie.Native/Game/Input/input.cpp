#pragma once
#include "input.h"
#include "../../Connection/Socket.h"
#include "../../libs/MinHook/MinHook.h"
#include "../../Connection/Packets/address_map.h"

using namespace Game::Input;
using namespace Connection::Packets;
using namespace Connection;

sMhKeyboard* keyboard = (sMhKeyboard*)nullptr;
uintptr_t HandleKeyboardInput = (uintptr_t)nullptr;

bool Game::Input::LoadAddress(uintptr_t ptrs[128])
{
    keyboard = (sMhKeyboard*)ptrs[GAME_INPUT_OFFSET];
    HandleKeyboardInput = ptrs[FUN_GAME_INPUT];
    return true;
}

void Game::Input::InitializeHooks()
{
    // Creates the input function hook
    MH_STATUS s = MH_CreateHook(((fnHandleKeyboard)HandleKeyboardInput),
        &HunterPie_HandleKeyboardInput,
        reinterpret_cast<LPVOID*>(&originalHandleInput));

    if (s != MH_OK)
        LOG("%s\n", MH_StatusToString(s));
}

void InjectInput(sMhKeyboard* keyboard)
{
    if (!inputInjectionQueue.empty())
    {
        input* currentInput = inputInjectionQueue.front();

        if (currentInput->ignoreOriginalInputs)
        {
            memcpy(keyboard->firstArray, currentInput->inputArray, sizeof(keyboard->firstArray));
            memcpy(keyboard->thirdArray, currentInput->inputArray, sizeof(keyboard->thirdArray));
        } else
        {
            for (int i = 0; i < sizeof(keyboard->firstArray); i++)
            {
                keyboard->firstArray[i] |= currentInput->inputArray[i];
                keyboard->thirdArray[i] |= currentInput->inputArray[i];
            }
        }

        keyboardInputElapsedFrames++;
        if (currentInput->nFrames <= keyboardInputElapsedFrames)
        {
            S_QUEUE_INPUT packet{};
            packet.header.opcode = OPCODE::QueueInput;
            packet.header.version = 1;
            packet.inputId = inputInjectionQueue.front()->injectionId;

            delete inputInjectionQueue.front();

            inputInjectionQueue.pop();

            Connection::Server::getInstance()->sendData(&packet, sizeof(packet));

            keyboardInputElapsedFrames = 0;
        }
    }
}

void Game::Input::HunterPie_HandleKeyboardInput(sMhKeyboard* keyboard)
{
    originalHandleInput(keyboard);

    std::mutex& inputQueueMtx = Server::getInstance()->inputQueueMutex;

    if (inputQueueMtx.try_lock())
    {

        std::queue<input*>& sharedQueue = Server::getInstance()->inputInjectionToQueue;

        if (!sharedQueue.empty())
        {
            input* toQueue = sharedQueue.front();
            inputInjectionQueue.push(toQueue);
            sharedQueue.pop();
        }

        inputQueueMtx.unlock();
    }

    InjectInput(keyboard);
}
