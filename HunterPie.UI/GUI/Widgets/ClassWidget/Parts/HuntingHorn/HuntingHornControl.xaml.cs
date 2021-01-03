using System;
using System.Windows.Media;
using System.Windows.Threading;
using HunterPie.Core.Events;
using HunterPie.Core.Jobs;
using HunterPie.GUI.Widgets.ClassWidget.Parts.Components;
using HunterPie.Core.Definitions;
using HunterPie.Core;
using System.Collections.Generic;
using System.Linq;
using HunterPie.Core.Native;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for HuntingHornControl.xaml
    /// </summary>
    public partial class HuntingHornControl : ClassControl
    {

        HuntingHorn Context { get; set; }

        readonly Brush[] cachedBrushes = new Brush[3];
        readonly List<int> castOrder = new List<int>();

        public HuntingHornControl()
        {
            InitializeComponent();
        }

        public void SetContext(HuntingHorn context)
        {
            Context = context;
            SetupComponents();
            HookEvents();
            
        }

        private void SetupComponents()
        {
            // In case someone opens HunterPie after the game we need to trigger the
            // events again for this.
            OnNoteColorUpdate(this, new HuntingHornEventArgs(Context));
            OnNoteQueueUpdate(this, new HuntingHornNoteEventArgs(Context));
            OnSongQueueUpdate(this, new HuntingHornSongEventArgs(Context));
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        private void HookEvents()
        {
            Context.OnNoteColorUpdate += OnNoteColorUpdate;
            Context.OnNoteQueueUpdate += OnNoteQueueUpdate;
            Context.OnSongQueueUpdate += OnSongQueueUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
            Context.OnSongsCast += OnSongsCast;
        }

        public override void UnhookEvents()
        {
            Context.OnNoteColorUpdate -= OnNoteColorUpdate;
            Context.OnNoteQueueUpdate -= OnNoteQueueUpdate;
            Context.OnSongQueueUpdate -= OnSongQueueUpdate;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context.OnSongsCast -= OnSongsCast;
            SongQueue.Children.Clear();
            Sheet.Children.Clear();
            PredictionSheet.Children.Clear();
            castOrder.Clear();
        }

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                HasSafiBuff = args.SafijiivaRegenCounter != -1;
                SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
            }));
        }

        private void OnSongsCast(object source, HuntingHornSongCastEventArgs args)
        {
            
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (args.IsCastingBuffs && !args.IsDoubleCasting)
                {
                    
                    if (SongQueue.Children.Count < args.PlayStartAt + 1)
                    {
                        return;
                    }

                    // When the player is interrupted by monsters/taking damage
                    // This was hell to fix, please save me
                    if (args.IsCastingInterrupted)
                    {
                        foreach (SongComponent castedSong in SongQueue.Children)
                        {
                            if (castedSong.IsCasted)
                            {
                                castedSong.IsCasted = false;
                            }
                        }
                        SongQueue.Children.Clear();
                        castOrder.Clear();
                        ReconstructQueue();
                        return;
                    }
                    for (int i = args.PlayStartAt; i < SongQueue.Children.Count; i++)
                    {
                        SongComponent song = (SongComponent)SongQueue.Children[i];
                        if (!song.IsCasted)
                        {
                            castOrder.Add(i);
                            song.IsCasted = true;
                            break;
                        }
                    }
                } else if (args.IsCastingBuffs && args.IsDoubleCasting)
                {
                    int castedSongs = SongQueue.Children.Cast<SongComponent>().Where(e => e.IsCasted).Count();
                    if (castedSongs > args.PlayCurrentAt && args.PlayCurrentAt > 0)
                    {
                        ((SongComponent)SongQueue.Children[castOrder.Last()]).IsCasted = false;
                    }
                    foreach (SongComponent castedSong in SongQueue.Children)
                    {
                        if (castedSong.IsCasted)
                        {
                            castedSong.IsDoubleCasted = true;
                        }
                    }
                    castOrder.Clear();
                }
                else 
                {
                    foreach (SongComponent song in SongQueue.Children)
                    {
                        if (song.IsCasted)
                        {
                            song.Dispose();
                        }
                    }
                    
                }
                
            }));
        }

        private void OnSongQueueUpdate(object source, HuntingHornSongEventArgs args)
        {
            if (args.IsCastingSongs)
            {
                return;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (SongQueue.Children.Count == args.SongsQueued)
                {
                    int index = args.RawSongIndexesQueue[args.LastSongIndex];

                    if (args.Songs.Length < index + 1 || index < 0)
                    {
                        return;
                    }

                    sHuntingHornSong song = args.Songs[index];

                    SongComponent songComponent = new SongComponent()
                    {
                        SongName = GMD.GetMusicSkillNameById(song.BuffId)//GStrings.GetAbnormalityByID("HUNTINGHORN", song.BuffId, 0)
                    };
                    songComponent.SetSong(song.Notes, cachedBrushes);
                    SongQueue.Children.Insert(0, songComponent);

                    SongQueue.Children.RemoveAt(SongQueue.Children.Count - 1);

                } else
                {
                    bool insert = SongQueue.Children.Count > 0;
                    // Add remaning songs to the queue based on the SongQueue length
                    for (int i = 0; SongQueue.Children.Count < args.SongsQueued; i++)
                    {
                        int index = args.SongIndexesQueue[i];
                        sHuntingHornSong song = args.Songs[index];

                        SongComponent songComponent = new SongComponent()
                        {
                            SongName = GMD.GetMusicSkillNameById(song.BuffId)
                        };
                        songComponent.SetSong(song.Notes, cachedBrushes);

                        if (insert)
                        {
                            SongQueue.Children.Insert(0, songComponent);
                        } else
                        {
                            SongQueue.Children.Add(songComponent);
                        }
                    }
                }
            }));
        }

        private void ReconstructQueue()
        {
            for (int i = 0; SongQueue.Children.Count < Context.SongsQueued; i++)
            {
                int index = Context.SongIndexesQueue[i];
                sHuntingHornSong song = Context.Songs[index];

                SongComponent songComponent = new SongComponent()
                {
                    SongName = GMD.GetMusicSkillNameById(song.BuffId)
                };
                songComponent.SetSong(song.Notes, cachedBrushes);

                SongQueue.Children.Add(songComponent);
            }
        }

        private void OnNoteColorUpdate(object source, HuntingHornEventArgs args)
        {
            if (args.FirstNoteColor == Core.Enums.NoteColorId.None ||
                args.SecondNoteColor == Core.Enums.NoteColorId.None ||
                args.ThirdNoteColor == Core.Enums.NoteColorId.None)
            {
                return;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                cachedBrushes[0] = HuntingHorn.GetColorBasedOnColorId(args.FirstNoteColor);
                cachedBrushes[1] = HuntingHorn.GetColorBasedOnColorId(args.SecondNoteColor);
                cachedBrushes[2] = HuntingHorn.GetColorBasedOnColorId(args.ThirdNoteColor);
            }));
        }

        private void OnNoteQueueUpdate(object source, HuntingHornNoteEventArgs args)
        {
            if (args.FirstNoteIndex < 0 || args.NotesQueued < 0)
            {
                return;
            }
            
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (args.NotesQueued == 0 && Sheet.Children.Count >= 0)
                {
                    Sheet.Children.Clear();
                    PredictionSheet.Children.Clear();
                    return;
                }

                // If the number of notes in the visual sheet is lower than the in-game sheet,
                // we have to add all the notes.
                if (Sheet.Children.Count < args.NotesQueued)
                {
                    for (int i = Sheet.Children.Count; i < args.NotesQueued; i++)
                    {
                        byte noteId = args.Notes[i];

                        // Skip empty notes
                        if (noteId == 0)
                        {
                            continue;
                        }

                        NoteComponent note = new NoteComponent()
                        {
                            NoteId = noteId,
                            Height = noteId == 4 ? 25 : 33,
                            Width = 23
                        };
                        note.Color = noteId == 4 ? null : cachedBrushes[noteId - 1];

                        Sheet.Children.Add(note);
                    }
                }
                else
                {
                    byte lastNoteId = args.Notes[args.NotesQueued - 1];
                    
                    NoteComponent note = new NoteComponent()
                    {
                        NoteId = lastNoteId,
                        Height = lastNoteId == 4 ? 25 : 33,
                        Width = 23
                    };
                    note.Color = lastNoteId == 4 ? null : cachedBrushes[lastNoteId - 1];
                    Sheet.Children.Add(note);
                    ((NoteComponent)Sheet.Children[0]).Destroy = true;
                }
                UpdatePredictedSong(args.Candidates);
            }));   
        }

        private void UpdatePredictedSong(sHuntingHornSong[] predictions)
        {
            PredictionSheet.Children.Clear();
            foreach (sHuntingHornSong song in predictions)
            {
                SongPredComponent predDisplay = new SongPredComponent()
                {
                    SongName = GMD.GetMusicSkillNameById(song.BuffId)
                };
                predDisplay.UpdateNote(song.Notes[song.NotesLength - 1], cachedBrushes[song.Notes[song.NotesLength - 1] - 1]);
                PredictionSheet.Children.Add(predDisplay);
            }
        }
    }
}
