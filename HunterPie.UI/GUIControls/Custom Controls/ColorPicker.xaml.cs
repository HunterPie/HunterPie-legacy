using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {

        public string NameText
        {
            get => (string)GetValue(NameTextProperty);
            set => SetValue(NameTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof(string), typeof(ColorPicker));

        private string _Color { get; set; }
        public string Color
        {
            get => _Color;
            set { SetColor(value); _Color = value; }
        }

        public ColorPicker() => InitializeComponent();

        private void SetColor(string HexColor)
        {
            Color clr = (Color)ColorConverter.ConvertFromString(HexColor);
            ColorCircle.Fill = new SolidColorBrush(clr);
            ColorHex.Text = clr.ToString();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var ColorPickDialog = new System.Windows.Forms.ColorDialog())
            {
                var result = ColorPickDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string PickedColor = RGBToHex(ColorPickDialog.Color.R, ColorPickDialog.Color.G, ColorPickDialog.Color.B, ColorPickDialog.Color.A);
                    Color = PickedColor;
                }

            }
        }

        private string RGBToHex(byte R, byte G, byte B, byte A)
        {
            string ColorHex = $"#{A:X2}{R:X2}{G:X2}{B:X2}";
            return ColorHex;
        }
    }
}
