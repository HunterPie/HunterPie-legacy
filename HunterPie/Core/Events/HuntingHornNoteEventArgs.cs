using System;
using HunterPie.Core.Definitions;
using HunterPie.Core.Jobs;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event arguments used by the Hunting Horn note events
    /// </summary>
    public class HuntingHornNoteEventArgs : EventArgs
    {
        /// <summary>
        /// The last 4 notes the player has played, unorganized and in the same order as they appear in memory.
        /// </summary>
        public byte[] RawNotes { get; }

        /// <summary>
        /// The last 4 notes the player has played, this is organized in the same order as they appear in the HUD
        /// </summary>
        public byte[] Notes { get; }

        /// <summary>
        /// The index of the first note, this is required if you're using the <b>RawNotes</b>.
        /// </summary>
        public long FirstNoteIndex { get; }

        /// <summary>
        /// The amount of valid notes currently in <b>RawNotes</b> array.
        /// </summary>
        public long NotesQueued { get; }

        /// <summary>
        /// Possible songs that can be played with the current Notes
        /// </summary>
        public sHuntingHornSong[] Candidates { get; }

        public HuntingHornNoteEventArgs(HuntingHorn huntinghorn)
        {
            RawNotes = huntinghorn.RawNotes;
            Notes = huntinghorn.Notes;
            FirstNoteIndex = huntinghorn.FirstNoteIndex;
            NotesQueued = huntinghorn.NotesQueued;
            Candidates = huntinghorn.SongCandidates;
        }

    }
}
