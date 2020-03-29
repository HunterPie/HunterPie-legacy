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

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for MinimalButton.xaml
    /// </summary>
    public partial class MinimalButton : UserControl {

        public Brush Color {
            get { return btnBackground.Fill; }
            set { btnBackground.Fill = value; }
        }
        public string Text {
            get { return btnText.Text; }
            set { btnText.Text = value; }
        }
        public ImageSource Icon {
            get { return btnIcon.Source; }
            set { btnIcon.Source = value; }
        }
        public string Link { get; set; }

        public MinimalButton() {
            InitializeComponent();
        }

        private void OnClick(object sender, MouseButtonEventArgs e) {
            if (Link == null) return;
            System.Diagnostics.Process.Start(Link);
        }
    }
}
