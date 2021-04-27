#pragma once
#include "model.h"

namespace Connection
{
    namespace Packets
    {
        typedef struct input
        {
            char inputArray[32];
            int nFrames;
            unsigned int injectionId;
            bool ignoreOriginalInputs;
        };

        typedef struct C_CONNECT : I_PACKET
        {
            uintptr_t addresses[128];
        };

        typedef struct S_CONNECT : I_PACKET
        {
            bool success;
        };

        typedef struct C_DISCONNECT : I_PACKET
        {
            char message[256];
            float unk1;
            unsigned int unk2;
            unsigned char unk3;
        };

        typedef struct S_DISCONNECT : I_PACKET {};

        typedef struct C_ENABLE_HOOKS : I_PACKET {};
        typedef struct C_DISABLE_HOOKS : I_PACKET {};

        typedef struct C_QUEUE_INPUT : I_PACKET
        {
            input inputs;
        };

        typedef struct S_QUEUE_INPUT : I_PACKET
        {
            unsigned int inputId;
        };

        typedef struct C_SEND_CHAT : I_PACKET
        {
            char message[256];
        };

        typedef struct C_SEND_SYSTEM_CHAT : I_PACKET
        {
            char message[256];
            float unk1;
            unsigned int unk2;
            unsigned char unk3;
        };

        typedef struct S_DEAL_DAMAGE : I_PACKET
        {
            uintptr_t target;
            int damage;
            bool isCrit;
            bool isTenderized;
            int attackId;
        };

    }
}
