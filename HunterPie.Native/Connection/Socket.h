
#define DEFAULT_BUFFER_SIZE 8192
#include <WinSock2.h>

#include "Packets/Definitions.h"

#include <mutex>
#include <queue>

#include <ws2tcpip.h>
#include "Logger.h"
#pragma comment (lib, "ws2_32.lib")

namespace Connection
{
    class Server
    {
    public:
        std::queue<Packets::input*> inputInjectionToQueue;
        std::mutex inputQueueMutex;

        static Server* getInstance();

        void sendData(void* data, int size);

        bool initialize();
        
    private:
        Server();
        Server(Server const&);
        Server& operator=(Server const&);

        void receivePackets(char buffer[DEFAULT_BUFFER_SIZE]);

        void enableHooks();
        void disableHooks();
        void disconnectNative();

        bool isInitialized = false;
        bool hooksEnabled = false;

        static Server* _instance;
        SOCKET client;
    };
}
