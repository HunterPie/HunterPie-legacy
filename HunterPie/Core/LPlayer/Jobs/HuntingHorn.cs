using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using System.Linq;
using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class HuntingHorn : Job
    {

        // Action Ids for when the player cast buffs from the queue
        public static readonly int[] SongCastActionIds = { 77, 81, 82, 83, 84, 85, 86, 87, 88, 91, 92, 94, 95, 96, 97, 102 };
        public static readonly int[] DoubleSongCastActionIds = { 91, 92, 94, 95, 96, 97, 102 };
        #region Private properties
        private sHuntingHornSong[] songs = new sHuntingHornSong[0];
        private long notesQueued = -1;
        private long firstNoteIndex = -1;
        private long lastSongIndex = 0;
        private long songIdFirstIndex = 0;
        private long playCurrentAt = 0;
        private bool isCastingBuffs;
        private bool isDoubleCastingBuffs;
        private bool isCastingInterrupted;

        // Colors
        private NoteColorId firstNote = NoteColorId.None;
        private NoteColorId secondNote = NoteColorId.None;
        private NoteColorId thirdNote = NoteColorId.None;
        #endregion

        #region Public properties
        public sHuntingHornSong[] Songs => songs;
        public sHuntingHornSong[] SongCandidates => FindSongCandidates();
        
        public byte[] Notes => OrganizeQueue<byte>(RawNotes, FirstNoteIndex, NotesQueued);
        public byte[] RawNotes { get; private set; } = new byte[4];
        public long NotesQueued
        {
            get => notesQueued;
            set
            {
                if (value != notesQueued)
                {
                    notesQueued = value;
                    if (FirstNoteIndex >= 0)
                    {
                        DispatchNoteEvents(OnNoteQueueUpdate);
                    }
                }
            }
        }
        public long FirstNoteIndex
        {
            get => firstNoteIndex;
            set
            {
                if (value != firstNoteIndex)
                {
                    firstNoteIndex = value;
                    if (NotesQueued >= 0)
                    {
                        DispatchNoteEvents(OnNoteQueueUpdate);
                    }
                }
            }
        }
        
        public int[] SongQueue => OrganizeQueue<int>(RawSongQueue, LastSongIndex, SongsQueued);
        public int[] RawSongQueue { get; private set; } = new int[3];
        public long SongsQueued { get; private set; }
        public long LastSongIndex
        {
            get => lastSongIndex;
            set
            {
                if (value != lastSongIndex)
                {
                    lastSongIndex = value;
                    DispatchSongEvents(OnSongQueueUpdate);
                }
            }
        }

        public int[] SongIndexesQueue => OrganizeQueue<int>(RawSongIndexesQueue, SongIndexesFirstIndex, SongIndexesQueued);
        public int[] RawSongIndexesQueue { get; private set; } = new int[3];
        public long SongIndexesQueued { get; private set; }
        public long SongIndexesFirstIndex { get; private set; }

        public int[] SongIdsQueue => OrganizeQueue(RawSongIdsQueue, SongIdFirstIndex, 3);
        public int[] RawSongIdsQueue { get; private set; } = new int[3];
        public long SongIdFirstIndex
        {
            get => songIdFirstIndex;
            private set
            {
                if (value != songIdFirstIndex)
                {
                    songIdFirstIndex = value;
                }
            }
        }
        public byte PlayStartAt { get; private set; }
        public long PlayLastAt { get; private set; }
        public long PlayCurrentAt
        {
            get => playCurrentAt;
            set
            {
                if (value != playCurrentAt)
                {
                    PlayLastAt = playCurrentAt;
                    playCurrentAt = value;
                    DispatchSongCastEvents(OnSongsCast);
                }
            }
        }
        public bool IsCastingBuffs
        {
            get => isCastingBuffs;
            private set
            {
                if (value != isCastingBuffs)
                {
                    isCastingBuffs = value;
                    DispatchSongCastEvents(OnSongsCast);
                }
            }
        }
        public bool IsDoubleCastingBuffs
        {
            get => isDoubleCastingBuffs;
            private set
            {
                if (value != isDoubleCastingBuffs)
                {
                    isDoubleCastingBuffs = value;
                    DispatchSongCastEvents(OnSongsCast);
                }
            }
        }
        public bool IsCastingInterrupted
        {
            get => isCastingInterrupted;
            set
            {
                if (value != isCastingInterrupted)
                {
                    isCastingInterrupted = value;
                    DispatchSongCastEvents(OnSongsCast);
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
                    if (secondNote != NoteColorId.None)
                    {
                        DispatchEvents(OnNoteColorUpdate);
                    }
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
                    if (thirdNote != NoteColorId.None)
                    {
                        DispatchEvents(OnNoteColorUpdate);
                    }
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
                    DispatchEvents(OnNoteColorUpdate);
                }
            }
        }
        #endregion

        public override int SafijiivaMaxHits => 5;

        #region Events
        public delegate void HuntingHornEvents(object source, HuntingHornEventArgs args);
        public delegate void HuntingHornNoteEvents(object source, HuntingHornNoteEventArgs args);
        public delegate void HuntingHornSongEvents(object source, HuntingHornSongEventArgs args);
        public delegate void HuntingHornSongCastEvents(object source, HuntingHornSongCastEventArgs args);

        public event HuntingHornEvents OnSongsListUpdate;
        public event HuntingHornEvents OnNoteColorUpdate;

        public event HuntingHornNoteEvents OnNoteQueueUpdate;
        public event HuntingHornSongEvents OnSongQueueUpdate;
        public event HuntingHornSongCastEvents OnSongsCast;

        protected virtual void DispatchEvents(HuntingHornEvents e) => e?.Invoke(this, new HuntingHornEventArgs(this));
        protected virtual void DispatchNoteEvents(HuntingHornNoteEvents e) => e?.Invoke(this, new HuntingHornNoteEventArgs(this));
        protected virtual void DispatchSongEvents(HuntingHornSongEvents e) => e?.Invoke(this, new HuntingHornSongEventArgs(this));
        protected virtual void DispatchSongCastEvents(HuntingHornSongCastEvents e) => e?.Invoke(this, new HuntingHornSongCastEventArgs(this));
        #endregion

        public void UpdateInformation(sHuntingHornMechanics mechanics, sHuntingHornSong[] availableSongs, int playerActionId)
        {
            // Depending on HunterPie's polling rate, we read invalid values on login
            if (mechanics.Notes_Length > 4 || mechanics.FirstNoteIndex > 4)
            {
                return;
            }

            songs = availableSongs;

            FirstNoteColor = mechanics.FirstNote;
            SecondNoteColor = mechanics.SecondNote;
            ThirdNoteColor = mechanics.ThirdNote;

            RawNotes = mechanics.Notes;
            NotesQueued = mechanics.Notes_Length;
            FirstNoteIndex = mechanics.FirstNoteIndex;

            RawSongQueue = mechanics.Songs;
            SongsQueued = mechanics.Songs_Length;

            RawSongIndexesQueue = mechanics.SongIndexes;
            SongIndexesQueued = mechanics.SongIndexes_Length;
            SongIndexesFirstIndex = mechanics.SongIndexFirstIndex;

            RawSongIdsQueue = mechanics.SongIds;
            PlayStartAt = mechanics.PlayStartAt;
            SongIdFirstIndex = mechanics.SongIdFirstIndex;
            PlayCurrentAt = mechanics.PlayCurrentAt;
                        
            LastSongIndex = mechanics.LastSongIndex;

            IsCastingInterrupted = IsCastingBuffs && !SongCastActionIds.Contains(playerActionId) && !DoubleSongCastActionIds.Contains(playerActionId) && playerActionId != 90;
            IsCastingBuffs = SongCastActionIds.Contains(playerActionId);
            IsDoubleCastingBuffs = DoubleSongCastActionIds.Contains(playerActionId);
        }

        /// <summary>
        /// Compares whether the new Songs list is equal to the old songs list to avoid triggering events
        /// for no reason
        /// </summary>
        /// <param name="newSongsList">sHuntingHornSong array with the new songs</param>
        /// <returns>True if they're equal, false otherwise.</returns>
        private bool IsSongArrayEqual(sHuntingHornSong[] newSongsList)
        {
            
            if (Songs.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < Songs.Length; i++)
            {
                if (Songs[i].Equals(newSongsList[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private T[] OrganizeQueue<T>(T[] unorganizedArray, long startAt, long length)
        {
            T[] organizedNotes = new T[unorganizedArray.Length];

            if (startAt < 0 || length < 0)
            {
                return organizedNotes;
            }

            for (int i = 0; i < unorganizedArray.Length; i++)
            {
                if (i >= length)
                {
                    break;
                } else
                {
                    organizedNotes[i] = unorganizedArray[(i + startAt) % unorganizedArray.Length];
                }
            }

            return organizedNotes;
        }

        /// <summary>
        /// Searches the Songs array for possible songs that start with the same note combination
        /// the user currently has in Notes
        /// </summary>
        /// <returns>Array with sHuntingHornSong</returns>
        private sHuntingHornSong[] FindSongCandidates()
        {
            List<sHuntingHornSong> candidates = new List<sHuntingHornSong>();
            if (NotesQueued == 0 || NotesQueued == -1 || FirstNoteIndex == -1)
            {
                return candidates.ToArray();
            }

            foreach (sHuntingHornSong song in Songs)
            {
                bool matches = false;
                // Now we look for notes that have the same starting notes
                for (long i = song.NotesLength - 1; i > 0; i--)
                {
                    if (song.Id < 0 || NotesQueued < song.NotesLength - 1)
                    {
                        continue;
                    }

                    if (song.Notes[i - 1] == Notes[(NotesQueued - song.NotesLength) + i])
                    {
                        matches = true;
                    } else
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    candidates.Add(song);
                }
                
            }
            return candidates.ToArray();
        }

        public static Brush GetColorBasedOnColorId(NoteColorId colorId)
        {
            return Application.Current.FindResource($"NoteColor{(int)colorId}") as Brush;
        }
    }
}
