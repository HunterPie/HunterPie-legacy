// dllmain.cpp : Defines the entry point for the DLL application.
#pragma once
#include <iostream>
#include "Connection/Socket.h"
#include <thread>

using namespace Connection;

void CreateConsole()
{
    AllocConsole();
    FILE* dummy;
    freopen_s(&dummy, "CONOUT$", "w", stdout);
}

void LoadNativeDll()
{
    CreateConsole();
    std::cout << "Initialized console" << std::endl;

    std::thread([]()
        {
            bool result = Server::getInstance()->initialize();
            std::cout << "Result: " << result << std::endl;
        }).detach();

}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        LoadNativeDll();
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

