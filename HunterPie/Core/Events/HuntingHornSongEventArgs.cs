using System;
using HunterPie.Core.Definitions;
using HunterPie.Core.LPlayer.Jobs;

namespace HunterPie.Core.Events
{
    public class HuntingHornSongEventArgs : EventArgs
    {
        /// <summary>
        /// Available Hunting Horn songs
        /// </summary>
        public sHuntingHornSong[] Songs { get; }

        /// <summary>
        /// Current to-be-played list of songs,
        /// their values are their buff Ids.
        ///
        /// This array is organized based on the <seealso cref="LastSongIndex"/>
        /// </summary>
        public int[] SongQueue { get; }

        /// <summary>
        /// Same as <see cref="SongQueue"/>, but it's in the same order as they appear in memory.
        /// </summary>
        public int[] RawSongQueue { get; }

        /// <summary>
        /// Number of songs in the <see cref="SongQueue"/>
        /// </summary>
        public long SongsQueued { get; }

        /// <summary>
        /// Index of the last song added to the queue.
        /// </summary>
        public long LastSongIndex { get; }

        /// <summary>
        /// Same as <see cref="SongQueue"/>, however, the values are indexes pointing to the song in the
        /// <see cref="Songs"/>.
        /// </summary>
        public int[] SongIndexesQueue { get; }

        /// <summary>
        /// Same as <see cref="SongIndexesQueue"/>, but in the order they appear in memory.
        /// </summary>
        public int[] RawSongIndexesQueue { get; }

        /// <summary>
        /// Number of songs queued in the <see cref="SongIndexesQueue"/>
        /// </summary>
        public long SongIndexesQueued { get; }

        /// <summary>
        /// Index of the first song in the <see cref="RawSongIndexesQueue"/>
        /// </summary>
        public long SongIndexesFirstIndex { get; }

        /// <summary>
        /// Whether the player is casting buffs based on their player Action Id
        /// </summary>
        public bool IsCastingSongs { get; }

        /// <summary>
        /// Whether the song cast was interrupted by either a monster hitting the player
        /// </summary>
        public bool IsCastingInterrupted { get; }

        public HuntingHornSongEventArgs(HuntingHorn huntingHorn)
        {
            Songs = huntingHorn.Songs;
            SongQueue = huntingHorn.SongQueue;
            RawSongQueue = huntingHorn.RawSongQueue;
            SongsQueued = huntingHorn.SongsQueued;
            LastSongIndex = huntingHorn.LastSongIndex;
            SongIndexesQueue = huntingHorn.SongIndexesQueue;
            RawSongIndexesQueue = huntingHorn.RawSongIndexesQueue;
            SongIndexesQueued = huntingHorn.SongIndexesQueued;
            SongIndexesFirstIndex = huntingHorn.SongIndexesFirstIndex;
            IsCastingSongs = huntingHorn.IsCastingBuffs;
            IsCastingInterrupted = huntingHorn.IsCastingInterrupted;
        }

    }
}
