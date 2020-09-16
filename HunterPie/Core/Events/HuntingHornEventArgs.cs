using System;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using HunterPie.Core.Local.Jobs;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event arguments used by the Hunting Horn setup events
    /// </summary>
    public class HuntingHornEventArgs : EventArgs
    {
        /// <summary>
        /// Available songs for this Hunting Horn
        /// </summary>
        public sHuntingHornSong[] Songs { get; }

        /// <summary>
        /// First note color
        /// </summary>
        public NoteColorId FirstNoteColor { get; }

        /// <summary>
        /// Second note color
        /// </summary>
        public NoteColorId SecondNoteColor { get; }

        /// <summary>
        /// Third note color
        /// </summary>
        public NoteColorId ThirdNoteColor { get; }

        public HuntingHornEventArgs(HuntingHorn huntingHorn)
        {
            Songs = huntingHorn.Songs;
            FirstNoteColor = huntingHorn.FirstNoteColor;
            SecondNoteColor = huntingHorn.SecondNoteColor;
            ThirdNoteColor = huntingHorn.ThirdNoteColor;
        }

    }
}
