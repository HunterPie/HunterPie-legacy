// dllmain.cpp : Defines the entry point for the DLL application.
#pragma once
#include "Connection/Socket.h"

using namespace Connection;


void LoadNativeDll()
{

    std::thread([]()
        {
            bool result = Server::getInstance()->initialize();
            
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
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

