#define WIN32_LEAN_AND_MEAN

#pragma once
#include "Packets/Definitions.h"
#include <WinSock2.h>
#include <ws2tcpip.h>
#pragma comment (lib, "Ws2_32.lib")

#define DEFAULT_BUFFER_SIZE 1024

namespace Connection
{
    class Server
    {
    public:
        static Server* getInstance();

        void sendData(void* data, int size);

        bool initialize();

    private:
        Server();
        Server(Server const&);
        Server& operator=(Server const&);

        void receivePackets(char buffer[DEFAULT_BUFFER_SIZE]);

        bool isInitialized = false;

        static Server* _instance;
        SOCKET client;
    };
}
