#pragma once
#include "Socket.h"
#include <iostream>
#include <thread>
#include <chrono>

using namespace Connection;

Server* Server::_instance;

Server::Server() {}

bool Connection::Server::initialize()
{
    if (isInitialized)
        return true;

    const wchar_t* addr = L"127.0.0.1";
    const unsigned short port = 16969;

    WSADATA wsaData;
    WORD ver = MAKEWORD(2, 2);

    if (WSAStartup(ver, &wsaData) != 0)
    {
        std::cout << "[HunterPie.Native] Failed to initialize winsock" << std::endl;
        return false;
    }

    SOCKET ListenSocket;
    ListenSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (ListenSocket == INVALID_SOCKET)
    {
        std::cout << "[HunterPie.Native] INVALID_SOCKET" << std::endl;
        WSACleanup();
        return false;
    }

    SOCKADDR_IN addrServer;
    addrServer.sin_family = AF_INET;
    addrServer.sin_port = htons(port);
    InetPton(AF_INET, addr, &addrServer.sin_addr.s_addr);

    ZeroMemory(&addrServer.sin_zero, 8);

    if (bind(ListenSocket, (SOCKADDR*)&addrServer, sizeof(addrServer)) == SOCKET_ERROR)
    {
        std::cout << "Bind failed error: " << WSAGetLastError() << std::endl;
        closesocket(ListenSocket);
        WSACleanup();
        return false;
    }

    

    if (listen(ListenSocket, 5) == SOCKET_ERROR)
    {
        std::cout << "Failed to listen" << std::endl;
        closesocket(ListenSocket);
        WSACleanup();
        return false;
    }

    client = accept(ListenSocket, NULL, NULL);

    if (client == INVALID_SOCKET)
    {
        std::cout << "Failed to accept socket | Error: " << WSAGetLastError() << std::endl;
        closesocket(ListenSocket);
        WSACleanup();
        return false;
    }

    closesocket(ListenSocket);

    isInitialized = true;

    std::thread([this]()
        {
            using namespace std::chrono;
            char buffer[DEFAULT_BUFFER_SIZE];
            int recvSize;

            while (client != INVALID_SOCKET)
            {
                recvSize = recv(client, buffer, sizeof(buffer), 0);

                if (recvSize > 0)
                {
                    std::cout << "Received data" << std::endl;
                    receivePackets(buffer);
                    ZeroMemory(buffer, sizeof(buffer));
                }

                std::this_thread::sleep_for(16ms);
            }
        }).detach();

    std::cout << "Connection created" << std::endl;

    return true;
}

void Connection::Server::receivePackets(char buffer[DEFAULT_BUFFER_SIZE])
{
    using namespace Packets;

    I_PACKET packet = *reinterpret_cast<I_PACKET*>(buffer);

    if (packet.header.opcode == OPCODE::Connect)
    {
        std::cout << "Received a connect!" << std::endl;

        S_CONNECT packet{};
        packet.header.opcode = OPCODE::Connect;
        packet.header.version = 1;
        packet.success = true;

        sendData(&packet, sizeof(packet));
    }
}

Server* Connection::Server::getInstance()
{
    if (!_instance)
        return new Server();

    return _instance;
}

void Connection::Server::sendData(void* data, int size)
{
    send(client, (char*)data, size, 0);
}

Server& Connection::Server::operator=(Server const&)
{
    return *this;
}
