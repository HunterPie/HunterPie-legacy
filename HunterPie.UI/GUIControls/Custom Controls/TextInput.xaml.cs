using System.Windows;
using System.Windows.Controls;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for TextInput.xaml
    /// </summary>
    public partial class TextInput : UserControl
    {
        public TextInput()
        {
            InitializeComponent();
            TextBox.GotFocus += TextBoxFocusChanged;
            TextBox.LostFocus += TextBoxFocusChanged;
        }

        private void TextBoxFocusChanged(object sender, RoutedEventArgs e)
        {
            this.WatermarkTextBox.Visibility = WatermarkVisible
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark", typeof(string), typeof(TextInput), new PropertyMetadata(default(string)));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }


        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(TextInput), new PropertyMetadata(default(bool)));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private bool WatermarkVisible => !TextBox.IsFocused && string.IsNullOrEmpty(Text);
    }
}
