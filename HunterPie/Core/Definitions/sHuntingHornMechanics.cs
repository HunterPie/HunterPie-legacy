using System.Runtime.InteropServices;
using HunterPie.Core.Enums;
// To make it easier to understand the structure
using size_t = System.Int64;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sHuntingHornMechanics
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x4)]
        public byte[] Notes;
        public int unkn0;
        public long FirstNoteIndex; // Index of the first note in the Notes array
        public size_t Notes_Length; // Length of the Notes array % 4

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3)]
        public int[] Songs;
        public int unkn1; // I think this is part of the array above to make sure it doesn't overflow, not sure though
        public long LastSongIndex; // Index of the last song added to the Songs array
        public size_t Songs_Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3)]
        public int[] SongIndexes;
        public int unkn2;
        public long SongIndexFirstIndex; // Index of the first song added to the SongIndexes array
        public size_t SongIndexes_Length;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3)]
        public int[] SongIds;
        public int unkn3;
        public long SongIdFirstIndex; // Index of the first song in the SongIds
        public long PlayCurrentAt; // Index of the song in the Songs array that's being currently played
        public long PlayStartAt; // Indicates which index the game will start at when playing the songs in the song queue

        public int unkn4;
        public int unkn5;
        public int unkn6;

        // Note colors
        public NoteColorId FirstNote;
        public NoteColorId SecondNote;
        public NoteColorId ThirdNote;
    }
}
