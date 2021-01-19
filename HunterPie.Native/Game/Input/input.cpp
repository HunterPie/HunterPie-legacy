#include "../../pch.h"
#pragma once
#include "input.h"
#include "../../Connection/Socket.h"


using namespace Game::Input;
using namespace Connection::Packets;

void Game::Input::HunterPie_HandleKeyboardInput(sMhKeyboard* keyboard)
{
    originalHandleInput(keyboard);

    if (!inputInjectionToQueue.empty())
    {
        input* toQueue = inputInjectionToQueue.front();
        inputInjectionQueue.push(toQueue);
        inputInjectionToQueue.pop();
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

            free(inputInjectionQueue.front());
            inputInjectionQueue.pop();

            Connection::Server::getInstance()->sendData(&packet, sizeof(packet));
        }
    }
}

