using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for MinimalSlider.xaml
    /// </summary>
    public partial class MinimalSlider : UserControl
    {

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(MinimalSlider));

        public double MinChange
        {
            get => (double)GetValue(MinChangeProperty);
            set => SetValue(MinChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinChangeProperty =
            DependencyProperty.Register("MinChange", typeof(double), typeof(MinimalSlider));

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(MinimalSlider));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(MinimalSlider));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MinimalSlider));

        public string ValueText
        {
            get => (string)GetValue(ValueTextProperty);
            set => SetValue(ValueTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValueText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(MinimalSlider));

        public bool IsInteger
        {
            get => (bool)GetValue(IsIntegerProperty);
            set => SetValue(IsIntegerProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValueText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsIntegerProperty =
            DependencyProperty.Register("IsInteger", typeof(bool), typeof(MinimalSlider));

        public MinimalSlider() => InitializeComponent();

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            // - ensure value in provided range
            // - if integer, floor

            var textBox = (TextBox)sender;
            if (double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
            {
                if (parsed < MinValue)
                {
                    Value = MinValue;
                }
                else if (parsed > MaxValue)
                {
                    Value = MaxValue;
                }
                else if (IsInteger)
                {
                    Value = (int)parsed;
                }
            }
            else
            {
                // value cannot be parsed: this should not occur because of TextChanged validation, keeping just in case
                Value = MinValue;
            }

            textBox.Text = Value.ToString(CultureInfo.InvariantCulture);
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // - don't allow dots for int values
            // - remove all non-numeric characters
            // - remove all dots after first one

            var tb = (TextBox)sender;
            var isDotAllowed = !IsInteger;
            var sel = tb.SelectionStart;
            tb.Text = string.Join("", tb.Text.Where(c =>
            {
                if (char.IsDigit(c)) return true;
                if (isDotAllowed && c == '.')
                {
                    isDotAllowed = false;
                    return true;
                }

                return false;
            }));
            tb.SelectionStart = sel;
            e.Handled = true;
        }
    }
}
