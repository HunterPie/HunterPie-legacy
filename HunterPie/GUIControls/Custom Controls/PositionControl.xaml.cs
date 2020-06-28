using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Regex = System.Text.RegularExpressions.Regex;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl
    {
        public PositionControl() => InitializeComponent();

        public int X
        {
            get
            {
                int Parsed;
                if (int.TryParse(PosX.Text, out Parsed))
                {
                    return Parsed;
                }
                else { return 0; }
            }
            set => PosX.Text = value.ToString();
        }
        public int Y
        {
            get
            {
                int Parsed;
                if (int.TryParse(PosY.Text, out Parsed))
                {
                    return Parsed;
                }
                else { return 0; }
            }
            set => PosY.Text = value.ToString();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex NumberRegex = new Regex("[^0-9.-]+");
            e.Handled = NumberRegex.IsMatch(e.Text);
        }

        private void LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox s = sender as TextBox;
            if (string.IsNullOrEmpty(s.Text))
            {
                s.Text = "0";
            }
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox s = sender as TextBox;
            if (s.Text == "0")
            {
                s.Text = null;
            }
        }
    }
}
