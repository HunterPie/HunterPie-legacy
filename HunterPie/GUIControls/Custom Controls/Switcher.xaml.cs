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

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Switcher));

        public Visibility RestartVisibility
        {
            get => (Visibility)GetValue(RestartVisibilityProperty);
            set => SetValue(RestartVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for RestartVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RestartVisibilityProperty =
            DependencyProperty.Register("RestartVisibility", typeof(Visibility), typeof(Switcher));

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Switcher));


        public Switcher() => InitializeComponent();

        private void OnClick(object sender, MouseButtonEventArgs e) => IsEnabled = !IsEnabled;

    }
}
