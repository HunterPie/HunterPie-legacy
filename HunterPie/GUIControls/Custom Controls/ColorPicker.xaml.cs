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
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl {

        public string NameText {
            get { return (string)GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof(string), typeof(ColorPicker));
        
        private string _Color { get; set; }
        public string Color {
            get { return _Color; }
            set { SetColor(value); _Color = value; }
        }

        public ColorPicker() {
            InitializeComponent();
        }

        private void SetColor(string HexColor) {
            Color clr = (Color)ColorConverter.ConvertFromString(HexColor);
            this.ColorCircle.Fill = new SolidColorBrush(clr);
            this.ColorHex.Text = clr.ToString();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            using (var ColorPickDialog = new System.Windows.Forms.ColorDialog()) {
                var result = ColorPickDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    string PickedColor = RGBToHex(ColorPickDialog.Color.R, ColorPickDialog.Color.G, ColorPickDialog.Color.B, ColorPickDialog.Color.A);
                    this.Color = PickedColor;
                }
                
            }
        }
        
        private string RGBToHex(byte R, byte G, byte B, byte A) {
            string ColorHex = $"#{A:X2}{R:X2}{G:X2}{B:X2}";
            return ColorHex;
        }
    }
}
