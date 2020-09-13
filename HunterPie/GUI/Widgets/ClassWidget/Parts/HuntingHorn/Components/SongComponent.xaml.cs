using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System;
using HunterPie.Logger;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts.Components
{
    /// <summary>
    /// Interaction logic for SongComponent.xaml
    /// </summary>
    public partial class SongComponent : UserControl, IDisposable
    {

        public string SongName
        {
            get { return (string)GetValue(SongNameProperty); }
            set { SetValue(SongNameProperty, value); }
        }
        public static readonly DependencyProperty SongNameProperty =
            DependencyProperty.Register("SongName", typeof(string), typeof(SongComponent));

        public bool Destroy
        {
            get { return (bool)GetValue(DestroyProperty); }
            set { SetValue(DestroyProperty, value); }
        }
        public static readonly DependencyProperty DestroyProperty =
            DependencyProperty.Register("Destroy", typeof(bool), typeof(SongComponent));

        public bool IsCasted
        {
            get { return (bool)GetValue(IsCastedProperty); }
            set { SetValue(IsCastedProperty, value); }
        }
        public static readonly DependencyProperty IsCastedProperty =
            DependencyProperty.Register("IsCasted", typeof(bool), typeof(SongComponent));


        public SongComponent()
        {
            InitializeComponent();
        }

        public void SetSong(byte[] notes, Brush[] brushes)
        {
            foreach (byte note in notes)
            {
                if (note == 0)
                {
                    continue;
                }

                NoteComponent noteDisplay = new NoteComponent()
                {
                    NoteId = note,
                    Width = 15,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                if (note != 4)
                {
                    noteDisplay.Color = brushes[note - 1];
                }
                NotesPanel.Children.Add(noteDisplay);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Destroy = true;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private void OnSongRemoveAnimationComplete(object sender, EventArgs e)
        {
            ((Panel)Parent).Children.Remove(this);
        }
    }
}
