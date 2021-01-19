using System;
using System.Runtime.InteropServices;

namespace HunterPie.Native.Connection.Packets
{
    public class PacketParser
    {
        public static byte[] Serialize<T>(T data) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr mAlloc = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(data, mAlloc, true);

            byte[] buffer = new byte[size];
            Marshal.Copy(mAlloc, buffer, 0, buffer.Length);

            Marshal.FreeHGlobal(mAlloc);
            return buffer;
        }

        public static T Deserialize<T>(byte[] buffer) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            IntPtr mAlloc = Marshal.AllocHGlobal(size);

            Marshal.Copy(buffer, 0, mAlloc, size);

            T deserialized = Marshal.PtrToStructure<T>(mAlloc);

            Marshal.FreeHGlobal(mAlloc);

            return deserialized;
        }
    }
}
