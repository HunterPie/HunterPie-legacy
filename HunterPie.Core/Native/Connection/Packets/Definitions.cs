using System;
using System.Runtime.InteropServices;

namespace HunterPie.Native.Connection.Packets
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Header
    {
        public OPCODE opcode;
        public uint version;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct C_CONNECT
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public UIntPtr[] addresses;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct S_CONNECT
    {
        public Header header;
        [MarshalAs(UnmanagedType.Bool)]
        public bool success;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct C_DISCONNECT
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string message;
        public float unk1;
        public uint unk2;
        public byte unk3;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct S_DISCONNECT
    {
        public Header header;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct C_ENABLE_HOOKS
    {
        public Header header;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct C_DISABLE_HOOKS
    {
        public Header header;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct C_QUEUE_INPUT
    {
        public Header header;
        public input inputs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct S_QUEUE_INPUT
    {
        public Header header;
        public uint inputId;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct C_SEND_CHAT
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string message;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct C_SEND_SYSTEM_CHAT
    {
        public Header header;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string message;
        public float unk1;
        public uint unk2;
        public byte unk3;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct S_DEAL_DAMAGE
    {
        public Header header;
        public ulong target;
        public uint index;
        public int damage;
        [MarshalAs(UnmanagedType.Bool)]
        public bool isCrit;
        [MarshalAs(UnmanagedType.Bool)]
        public bool isTenderized;
    }

    #region Other Data structures

    [StructLayout(LayoutKind.Sequential)]
    public struct input
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] inputArray;
        public int nFrames;
        public uint injectionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool ignoreOriginalInputs;
    }

    #endregion
}
