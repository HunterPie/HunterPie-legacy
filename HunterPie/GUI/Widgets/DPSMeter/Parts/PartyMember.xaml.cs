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

namespace HunterPie.GUI.Widgets.DPSMeter.Parts {
    /// <summary>
    /// Interaction logic for PartyMember.xaml
    /// </summary>
    public partial class PartyMember : UserControl {

        public object Color {
            get { return (object)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(object), typeof(Rectangle), new PropertyMetadata(0));

        public object Player {
            get { return (object)GetValue(PlayerProperty); }
            set { SetValue(PlayerProperty, value); }
        }
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(object), typeof(TextBlock), new PropertyMetadata(0));

        public object DPS {
            get { return (object)GetValue(DPSProperty); }
            set { SetValue(DPSProperty, value); }
        }
        public static readonly DependencyProperty DPSProperty = DependencyProperty.Register("DPS", typeof(object), typeof(TextBlock), new PropertyMetadata(0));

        public object DPSPercentage {
            get { return (object)GetValue(DPSPercentageProperty); }
            set { SetValue(DPSPercentageProperty, value); }
        }
        public static readonly DependencyProperty DPSPercentageProperty = DependencyProperty.Register("DPSPercentage", typeof(object), typeof(Rectangle), new PropertyMetadata(0));

        public object Class {
            get { return (object)GetValue(ClassProperty); }
            set { SetValue(ClassProperty, value); }
        }

    public static readonly DependencyProperty ClassProperty = DependencyProperty.Register("Class", typeof(object), typeof(Image), new PropertyMetadata(0));

        public PartyMember() { 
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
