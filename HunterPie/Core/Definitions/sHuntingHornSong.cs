using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sHuntingHornSong
    {
        public int Id;
        public int unkn1;
        public int NotesLength; // The amount of notes this song requires
        public int unkn2; // Always FF FF FF FF?
        public int BuffId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Notes;
    }
}
