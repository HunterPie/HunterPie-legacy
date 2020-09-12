using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts.Components
{
    /// <summary>
    /// Interaction logic for SongPredComponent.xaml
    /// </summary>
    public partial class SongPredComponent : UserControl
    {

        public string SongName
        {
            get { return (string)GetValue(SongNameProperty); }
            set { SetValue(SongNameProperty, value); }
        }
        public static readonly DependencyProperty SongNameProperty =
            DependencyProperty.Register("SongName", typeof(string), typeof(SongPredComponent));


        public SongPredComponent()
        {
            InitializeComponent();
        }

        public void UpdateNote(byte noteId, Brush color)
        {
            Note.NoteId = noteId;
            Note.Color = color;
        }
    }
}
