using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts.HuntingHorn.Components
{
    /// <summary>
    /// Interaction logic for NoteComponent.xaml
    /// </summary>
    public partial class NoteComponent : UserControl
    {

        private byte noteId { get; set; }
        public byte NoteId
        {
            get => noteId;
            set
            {
                if (value != noteId)
                {
                    string mask = null;
                    switch (value)
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            mask = $"pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/HuntingHornNote_{value}.png";
                            break;
                    }
                    NoteMask = mask;
                }
            }
        }

        private string NoteMask
        {
            get { return (string)GetValue(NoteMaskProperty); }
            set { SetValue(NoteMaskProperty, value); }
        }
        private static readonly DependencyProperty NoteMaskProperty =
            DependencyProperty.Register("NoteMask", typeof(string), typeof(NoteComponent));

        public string Color
        {
            get { return (string)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(string), typeof(NoteComponent));



        public NoteComponent()
        {
            InitializeComponent();
        }
    }
}
