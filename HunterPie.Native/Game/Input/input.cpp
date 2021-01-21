#pragma once
#include "../../Connection/Socket.h"
#include "input.h"
#include "../../libs/MinHook/MinHook.h"
#include <iostream>


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

void Game::Input::HunterPie_HandleKeyboardInput(sMhKeyboard* keyboard)
{
    originalHandleInput(keyboard);

    std::mutex& inputQueueMtx = Server::getInstance()->inputQueueMutex;

    if (inputQueueMtx.try_lock())
    {
        std::queue<input*>& sharedQueue = Server::getInstance()->inputInjectionToQueue;

        while (!sharedQueue.empty())
        {
            input* toQueue = sharedQueue.front();
            inputInjectionQueue.push(toQueue);
            sharedQueue.pop();
            std::cout << "Queued input " << toQueue->injectionId << std::endl;
        }
        inputQueueMtx.unlock();
    }

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
        }
    }
}
