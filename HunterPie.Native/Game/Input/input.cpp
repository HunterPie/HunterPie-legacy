#pragma once
#include "../../Connection/Socket.h"
#include "input.h"
#include "../../libs/MinHook/MinHook.h"

using namespace Game::Input;
using namespace Connection::Packets;
using namespace Connection;

void Game::Input::InitializeHooks()
{
    // Creates the input function hook
    MH_CreateHook(Game::Input::HandleKeyboardInput,
        &HunterPie_HandleKeyboardInput,
        reinterpret_cast<LPVOID*>(&originalHandleInput));
}

void InjectInput(long long keyboardptr)
{
    sMhKeyboard* keyboard = reinterpret_cast<sMhKeyboard*>(keyboardptr);
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

void Game::Input::HunterPie_HandleKeyboardInput(long long keyboard)
{
    originalHandleInput(keyboard);

    std::mutex& inputQueueMtx = Server::getInstance()->inputQueueMutex;
    std::queue<input*>& sharedQueue = Server::getInstance()->inputInjectionToQueue;

    if (inputQueueMtx.try_lock())
    {
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
