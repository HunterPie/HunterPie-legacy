using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for MinimalButton.xaml
    /// </summary>
    public partial class MinimalButton : UserControl
    {



        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MinimalButton));



        public string Link
        {
            get => (string)GetValue(LinkProperty);
            set => SetValue(LinkProperty, value);
        }

        // Using a DependencyProperty as the backing store for Link.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Link", typeof(string), typeof(MinimalButton));



        public Brush Color
        {
            get => btnBackground.Fill;
            set => btnBackground.Fill = value;
        }
        public ImageSource Icon
        {
            get => btnIcon.Source;
            set => btnIcon.Source = value;
        }

        public MinimalButton() => InitializeComponent();

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (Link == null) return;
            System.Diagnostics.Process.Start(Link);
        }
    }
}
