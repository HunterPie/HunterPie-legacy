using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class HuntingHorn : Job
    {
        #region Private properties
        private sHuntingHornSong[] songs;

        private byte[] notes = new byte[4];
        private long firstNoteIndex = 0;
        private int[] songQueue = new int[3];
        private long lastSongIndex = 0;
        private int[] songIndexesQueue = new int[3];
        private int[] songIdsQueue = new int[3];
        private long songIdFirstIndex = 0;
        private long playCurrentAt = 0;
        private long playStartAt = 0;
        
        // Colors
        private NoteColorId firstNote;
        private NoteColorId secondNote;
        private NoteColorId thirdNote;
        #endregion

        #region Public properties
        public sHuntingHornSong[] Songs
        {
            get => songs;
            private set
            {
                if (!IsSongArrayEqual(value))
                {
                    songs = value;
                    // TODO: Event
                }
            }
        }

        public byte[] Notes => OrganizeQueue<byte>(notes, firstNoteIndex);
        public byte[] RawNotes => notes;
        public long NotesQueued { get; private set; }
        public long FirstNoteIndex
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
        

        public int[] SongQueue => OrganizeQueue<int>(songQueue, lastSongIndex);
        public int[] RawSongQueue => songQueue;
        public long SongsQueued { get; private set; }
        public long LastSongIndex
        {
            get => lastSongIndex;
            set
            {
                if (value != lastSongIndex)
                {
                    lastSongIndex = value;
                    // TODO: Event
                }
            }
        }

        public int[] SongIndexesQueue => OrganizeQueue<int>(songIndexesQueue, SongIndexesFirstIndex);
        public int[] RawSongIndexesQueue => songIndexesQueue;
        public long SongIndexesQueued { get; private set; }
        public long SongIndexesFirstIndex { get; private set; }

        public int[] SongIdsQueue => OrganizeQueue(songIdsQueue, songIdFirstIndex);
        public int[] RawSongIdsQueue => songIdsQueue;
        public long SongIdFirstIndex
        {
            get => songIdFirstIndex;
            private set
            {
                if (value != songIdFirstIndex)
                {
                    songIdFirstIndex = value;
                    // TODO: Event
                    
                }
            }
        }

        public NoteColorId FirstNoteColor
        {
            get => firstNote;
            private set
            {
                if (value != firstNote)
                {
                    firstNote = value;
                    // TODO: Event
                }
            }
        }
        public NoteColorId SecondNoteColor
        {
            get => secondNote;
            private set
            {
                if (value != secondNote)
                {
                    secondNote = value;
                    // TODO: Event
                }
            }
        }
        public NoteColorId ThirdNoteColor
        {
            get => thirdNote;
            private set
            {
                if (value != thirdNote)
                {
                    thirdNote = value;
                    // TODO: Event
                }
            }
        }
        #endregion

        public override int SafijiivaMaxHits => 5;

        #region Events
        public delegate void HuntingHornEvents(object source, EventArgs args);
        public delegate void HuntingHornNoteEvents(object source, HuntingHornNoteEventArgs args);
        public delegate void HuntingHornSongEvents(object source, HuntingHornSongEventArgs args);

        public event HuntingHornEvents OnSongsListUpdate;
        public event HuntingHornEvents OnNoteColorUpdate;

        public event HuntingHornNoteEvents OnNoteQueueUpdate;
        public event HuntingHornSongEvents OnSongQueueUpdate;
        #endregion


        public void UpdateInformation(sHuntingHornMechanics mechanics, sHuntingHornSong[] availableSongs)
        {
            
            Songs = availableSongs;

            notes = mechanics.Notes;
            NotesQueued = mechanics.Notes_Length;
            firstNoteIndex = mechanics.FirstNoteIndex;
            
            songIndexesQueue = mechanics.SongIndexes;
            SongIndexesQueued = mechanics.SongIndexes_Length;
            SongIndexesFirstIndex = mechanics.SongIndexFirstIndex;

            songIdsQueue = mechanics.SongIds;
            playStartAt = mechanics.PlayStartAt;
            playCurrentAt = mechanics.PlayCurrentAt;
            SongIdFirstIndex = mechanics.SongIdFirstIndex;

            songQueue = mechanics.Songs;
            SongsQueued = mechanics.Songs_Length;
            lastSongIndex = mechanics.LastSongIndex;

            FirstNoteColor = mechanics.FirstNote;
            SecondNoteColor = mechanics.SecondNote;
            ThirdNoteColor = mechanics.ThirdNote;
        }

        /// <summary>
        /// Compares whether the new Songs list is equal to the old songs list to avoid triggering events
        /// for no reason
        /// </summary>
        /// <param name="newSongsList">sHuntingHornSong array with the new songs</param>
        /// <returns>True if they're equal, false otherwise.</returns>
        private bool IsSongArrayEqual(sHuntingHornSong[] newSongsList)
        {
            if (Songs == null)
            {
                return false;
            }

            // 0x18 is the sizeof(sHuntingHornSong)
            byte[] newSongsBuffer = new byte[0x18 * 10];
            byte[] oldSongsBuffer = new byte[0x18 * 10];

            Buffer.BlockCopy(newSongsList, 0, newSongsBuffer, 0, newSongsBuffer.Length);
            Buffer.BlockCopy(songs, 0, oldSongsBuffer, 0, oldSongsBuffer.Length);

            for (int i = 0; i < newSongsBuffer.Length; i++)
            {
                if (oldSongsBuffer[i] != newSongsBuffer[i])
                {
                    return false;
                }
            }

            return true;
        }

        private T[] OrganizeQueue<T>(T[] unorganizedArray, long startAt)
        {
            T[] organizedNotes = new T[4];

            for (int i = 0; i < 4; i++)
            {
                organizedNotes[i] = unorganizedArray[(i + startAt) % unorganizedArray.Length];
            }

            return organizedNotes;
        }
    }
}
