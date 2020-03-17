using System.Windows.Controls;
using System.Windows.Media;

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for WeaknessDisplay.xaml
    /// </summary>
    public partial class WeaknessDisplay : UserControl {

        public ImageSource Icon {
            get { return WeaknessImage.Source; }
            set { WeaknessImage.Source = value; }
        }

        public WeaknessDisplay() {
            InitializeComponent();
        }
    }
}
