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

    S_LOG_MESSAGE packet{};
    packet.header.opcode = OPCODE::LogMessage;
    packet.header.version = 1;

    if (s != MH_OK) {
        LOG("Game::Input::InitializeHooks %s\n", MH_StatusToString(s));

        sprintf_s(packet.message
            , sizeof(packet.message)
            , "Error on Input hook %s", MH_StatusToString(s)
        );
    }
    else
    {
        sprintf_s(packet.message
            , sizeof(packet.message)
            , "Input Hook initialized successfully"
        );        
    }
    Connection::Server::getInstance()->sendData(&packet, sizeof(packet));
}

void InjectInput(sMhKeyboard* keyboard)
{
    if (!inputInjectionQueue.empty())
    {
        input* currentInput = inputInjectionQueue.front();

        if (currentInput->nFrames != 0)
        {
            if (currentInput->ignoreOriginalInputs)
            {
                memcpy(keyboard->firstArray, currentInput->inputArray, sizeof(keyboard->firstArray));
                memcpy(keyboard->thirdArray, currentInput->inputArray, sizeof(keyboard->thirdArray));
            }
            else
            {
                for (int i = 0; i < sizeof(keyboard->firstArray); i++)
                {
                    keyboard->firstArray[i] |= currentInput->inputArray[i];
                    keyboard->thirdArray[i] |= currentInput->inputArray[i];
                }
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

            inputInjectionQueue.pop_front();

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
        std::queue<C_INTERRUPT_INPUT*>& interruptQueue = Server::getInstance()->inputInterruptQueue;

        if (!sharedQueue.empty())
        {
            input* toQueue = sharedQueue.front();
            inputInjectionQueue.push_front(toQueue);
            sharedQueue.pop();
        }
        
        while (!interruptQueue.empty())
        {
            switch (const auto pkt = interruptQueue.front(); pkt->type)
            {
                case last: {
                    if (!inputInjectionQueue.empty()) {
                        input* front = inputInjectionQueue.front();
                        front->nFrames = 0;
                        LOG("   itrp last - ok\n");
                    }
                    else
                    {
                        LOG("   itrp last - empty\n");
                    }
                    break;
                }

                case by_id: {
                    bool ok = false;

                    for (auto input : inputInjectionQueue)
                    {
                        if (input->injectionId == pkt->inputId)
                        {
                            ok = true;
                            input->nFrames = 0;
                            break;
                        }
                    }
                    LOG("   itrp by id %s\n", ok ? "OK" : "not found");
                    break;
                }

                case clear:
                    for (auto input : inputInjectionQueue)
                    {
                        input->nFrames = 0;
                    }
                    LOG("   itrp all %llu\n", inputInjectionQueue.size());
                    break;
            }

            interruptQueue.pop();                
        }

        inputQueueMtx.unlock();
    }

    InjectInput(keyboard);
}
