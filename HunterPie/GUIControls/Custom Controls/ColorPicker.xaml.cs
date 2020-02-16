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

        public object NameText {
            get { return (object)GetValue(NameTextProperty); }
            set { SetValue(NameTextProperty, value); }
        }
        public static readonly DependencyProperty NameTextProperty = DependencyProperty.Register("NameText", typeof(object), typeof(TextBlock), new PropertyMetadata(0));

        private string _Color { get; set; }
        public string Color {
            get { return _Color; }
            set { SetColor(value); _Color = value; }
        }

        public ColorPicker() {
            InitializeComponent();
            this.DataContext = this;
        }

        private void SetColor(string HexColor) {
            this.ColorCircle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(HexColor));
            this.ColorHex.Text = HexColor;
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
