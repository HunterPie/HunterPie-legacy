using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using HunterPie.Core.Events;
using HunterPie.Core.LPlayer.Jobs;
using HunterPie.GUI.Widgets.ClassWidget.Parts.Components;
using HunterPie.Logger;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for HuntingHornControl.xaml
    /// </summary>
    public partial class HuntingHornControl : ClassControl
    {

        HuntingHorn Context { get; set; }
        Brush[] cachedBrushes = new Brush[3];

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
            OnNoteColorUpdate(this, new HuntingHornEventArgs(Context));
            OnNoteQueueUpdate(this, new HuntingHornNoteEventArgs(Context));
        }

        private void HookEvents()
        {
            Context.OnNoteQueueUpdate += OnNoteQueueUpdate;
            Context.OnNoteColorUpdate += OnNoteColorUpdate;
        }

        private void OnNoteColorUpdate(object source, HuntingHornEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                cachedBrushes[0] = HuntingHorn.GetColorBasedOnColorId(args.FirstNoteColor);
                cachedBrushes[1] = HuntingHorn.GetColorBasedOnColorId(args.SecondNoteColor);
                cachedBrushes[2] = HuntingHorn.GetColorBasedOnColorId(args.ThirdNoteColor);
            }));
        }

        public override void UnhookEvents()
        {

        }

        private void OnNoteQueueUpdate(object source, HuntingHornNoteEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                // If the number of notes in the visual sheet is lower than the in-game sheet,
                // we have to add all the notes.
                if (Sheet.Children.Count < args.NotesQueued)
                {
                    for (int i = Sheet.Children.Count; i < args.NotesQueued; i++)
                    {
                        byte noteId = args.Notes[i];

                        NoteComponent note = new NoteComponent()
                        {
                            NoteId = noteId,
                            Color = noteId == 4 ? null : cachedBrushes[noteId - 1]
                        };
                        Sheet.Children.Add(note);

                    }
                }
                else
                {
                    byte lastNoteId = args.Notes[args.NotesQueued - 1];
                    
                    NoteComponent note = new NoteComponent()
                    {
                        NoteId = lastNoteId,
                        Color = lastNoteId == 4 ? null : cachedBrushes[lastNoteId - 1]
                    };
                    Sheet.Children.Add(note);
                    Sheet.Children.RemoveAt(0);
                }
            }));
            
        }
    }
}
