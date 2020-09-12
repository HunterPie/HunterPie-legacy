using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts.Components
{
    /// <summary>
    /// Interaction logic for SongComponent.xaml
    /// </summary>
    public partial class SongComponent : UserControl
    {

        public string SongName
        {
            get { return (string)GetValue(SongNameProperty); }
            set { SetValue(SongNameProperty, value); }
        }
        public static readonly DependencyProperty SongNameProperty =
            DependencyProperty.Register("SongName", typeof(string), typeof(SongComponent));

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
    }
}
