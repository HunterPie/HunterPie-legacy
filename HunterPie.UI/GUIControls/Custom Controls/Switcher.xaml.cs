using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for Switcher.xaml
    /// </summary>
    public partial class Switcher : UserControl
    {

        public string Text
        {
            get => GetValue(TextProperty).ToString();
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Switcher));

        public Visibility RestartVisibility
        {
            get => (Visibility)GetValue(RestartVisibilityProperty);
            set => SetValue(RestartVisibilityProperty, value);
        }
        public static readonly DependencyProperty RestartVisibilityProperty =
            DependencyProperty.Register("RestartVisibility", typeof(Visibility), typeof(Switcher));

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }
        public static new readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Switcher));


        public Switcher() => InitializeComponent();

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            IsEnabled = !IsEnabled;
        }

    }
}
