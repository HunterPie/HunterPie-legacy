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
            char hunterpiepath[256];
        };

        typedef struct S_CONNECT : I_PACKET
        {
            bool success;
        };

        typedef struct C_DISCONNECT : I_PACKET {};

        typedef struct S_DISCONNECT : I_PACKET {};

        typedef struct C_QUEUE_INPUT : I_PACKET
        {
            input inputs;
        };

        typedef struct S_QUEUE_INPUT : I_PACKET
        {
            unsigned int inputId;
        };
    }
}
