using System;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class HuntingHorn : Job
    {
        #region Private properties
        private byte[] notes = new byte[4];
        private int firstNoteIndex = 0;
        private int notesQueued = 0;

        private int[] songQueue = new int[3];
        private int lastSongIndex = 0;
        private int songsQueued = 0;

        private int[] songIndexesQueue = new int[3];
        private int songIndexesFirstIndex = 0;
        private int songIndexesQueued = 0;

        private int[] songIdsQueue = new int[3];
        private int songIdFirstIndex = 0;
        private int playCurrentAt = 0;
        private int playStartAt = 0;

        // Colors
        private NoteColorId firstNote;
        private NoteColorId secondNote;
        private NoteColorId thirdNote;
        #endregion

        #region Public properties
        public byte[] Notes => GetOrganizedNotes();

        public byte[] RawNotes => notes;
        public int FirstNoteIndex
        {
            get => firstNoteIndex;
            set
            {
                if (value != firstNoteIndex)
                {
                    firstNoteIndex = value;
                    // TODO: Event
                }
            }
        }
        public int NotesQueued
        {
            get => notesQueued;
            set
            {
                if (value != notesQueued)
                {
                    notesQueued = value;
                    // TODO: Event
                }
            }
        }
        #endregion


        public override int SafijiivaMaxHits => 5;

        #region Events
        public delegate void HuntingHornNoteEvents(object source, HuntingHornNoteEventArgs args);
        public delegate void HuntingHornSongEvents(object source, HuntingHornSongEventArgs args);
        #endregion

        
        private byte[] GetOrganizedNotes()
        {
            byte[] organizedNotes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                organizedNotes[i] = Notes[(i + FirstNoteIndex) % 4];
            }

            return organizedNotes;
        }
    }
}
