using System.Windows.Media;
using System.Windows;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using HunterPie.Logger;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class HuntingHorn : Job
    {
        #region Private properties
        private sHuntingHornSong[] songs;
        private long notesQueued = -1;
        private long firstNoteIndex = -1;
        private long lastSongIndex = 0;
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
                    DispatchEvents(OnSongsListUpdate);
                }
            }
        }

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
                    // TODO: OnHuntingHornPlay event                    
                }
            }
        }
        public long PlayStartAt
        {
            get => playStartAt;
            set
            {
                if (value != playStartAt)
                {
                    playStartAt = value;
                }
            }
        }
        public long PlayCurrentAt
        {
            get => playCurrentAt;
            set
            {
                if (value != playCurrentAt)
                {
                    playCurrentAt = value;
                    // TODO: Probably event
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
                    DispatchEvents(OnNoteColorUpdate);
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
                    DispatchEvents(OnNoteColorUpdate);
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

        public event HuntingHornEvents OnSongsListUpdate;
        public event HuntingHornEvents OnNoteColorUpdate;

        public event HuntingHornNoteEvents OnNoteQueueUpdate;
        public event HuntingHornSongEvents OnSongQueueUpdate;

        protected virtual void DispatchEvents(HuntingHornEvents e) => e?.Invoke(this, new HuntingHornEventArgs(this));
        protected virtual void DispatchNoteEvents(HuntingHornNoteEvents e) => e?.Invoke(this, new HuntingHornNoteEventArgs(this));
        protected virtual void DispatchSongEvents(HuntingHornSongEvents e) => e?.Invoke(this, new HuntingHornSongEventArgs(this));
        #endregion
#if DEBUG // For testing purposes
        public HuntingHorn()
        {
            OnNoteQueueUpdate += HuntingHorn_OnNoteQueueUpdate;
            OnSongQueueUpdate += HuntingHorn_OnSongQueueUpdate;
        }
                
        private void HuntingHorn_OnSongQueueUpdate(object source, HuntingHornSongEventArgs args)
        {
            Debugger.Warn($"Songs queue: {args.SongQueue[0]} {args.SongQueue[1]} {args.SongQueue[2]}");
        }

        private void HuntingHorn_OnNoteQueueUpdate(object source, HuntingHornNoteEventArgs args)
        {
            Debugger.Log($"Notes: {args.Notes[0]} {args.Notes[1]} {args.Notes[2]} {args.Notes[3]}");
        }
#endif
        public void UpdateInformation(sHuntingHornMechanics mechanics, sHuntingHornSong[] availableSongs)
        {

            FirstNoteColor = mechanics.FirstNote;
            SecondNoteColor = mechanics.SecondNote;
            ThirdNoteColor = mechanics.ThirdNote;

            Songs = availableSongs;

            RawNotes = mechanics.Notes;
            NotesQueued = mechanics.Notes_Length;
            FirstNoteIndex = mechanics.FirstNoteIndex;
                                    
            RawSongIndexesQueue = mechanics.SongIndexes;
            SongIndexesQueued = mechanics.SongIndexes_Length;
            SongIndexesFirstIndex = mechanics.SongIndexFirstIndex;

            RawSongIdsQueue = mechanics.SongIds;
            PlayStartAt = mechanics.PlayStartAt;
            PlayCurrentAt = mechanics.PlayCurrentAt;
            SongIdFirstIndex = mechanics.SongIdFirstIndex;

            RawSongQueue = mechanics.Songs;
            SongsQueued = mechanics.Songs_Length;
            LastSongIndex = mechanics.LastSongIndex;

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

        public static Brush GetColorBasedOnColorId(NoteColorId colorId)
        {
            return Application.Current.FindResource($"NoteColor{(int)colorId}") as Brush;
        }
    }
}
