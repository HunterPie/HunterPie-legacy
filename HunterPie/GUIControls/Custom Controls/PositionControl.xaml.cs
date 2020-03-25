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
using Regex = System.Text.RegularExpressions.Regex;

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl {
        public PositionControl() {
            InitializeComponent();
        }

        public int X {
            get {
                int Parsed;
                if (int.TryParse(PosX.Text, out Parsed)) {
                    return Parsed;
                } else { return 0; }
            }
            set {
                PosX.Text = value.ToString();
            }
        }
        public int Y {
            get {
                int Parsed;
                if (int.TryParse(PosY.Text, out Parsed)) {
                    return Parsed;
                } else { return 0; }
            }
            set {
                PosY.Text = value.ToString();
            }
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e) {
            Regex NumberRegex = new Regex("[^0-9.-]+");
            e.Handled = NumberRegex.IsMatch(e.Text);
        }
    }
}
