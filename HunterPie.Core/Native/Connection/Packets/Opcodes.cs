﻿namespace HunterPie.Native.Connection.Packets
{
    public enum OPCODE : int
    {
        None,
        Connect,
        Disconnect,
        EnableHooks,
        DisableHooks,
        QueueInput,
        SendChatMessage,
        SendSystemMessage,
        DealDamage,
        InterruptInput,
        LogMessage
    }
}
