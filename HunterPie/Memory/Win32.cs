using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HunterPie.Memory
{
    class Win32
    {
        IntPtr processHandle;

        public Win32(IntPtr pHandle)
        {
            processHandle = pHandle;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);

        public T Read<T>(long address) where T : struct
        {
            return Read<T>(address, 1)[0];
        }

        public T[] Read<T>(long address, int count) where T : struct
        {
            IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf<T>() * count);
            Win32.ReadProcessMemory(processHandle, (IntPtr)address, buffer, Marshal.SizeOf<T>() * count, out _);
            var structures = BufferToStructures<T>(buffer, count);
            Marshal.FreeHGlobal(buffer);
            return structures;
        }

        private static T[] BufferToStructures<T>(IntPtr handle, int count)
        {
            var results = new T[count];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Marshal.PtrToStructure<T>(IntPtr.Add(handle, i * Marshal.SizeOf<T>()));
            }

            return results;
        }
    }
}
