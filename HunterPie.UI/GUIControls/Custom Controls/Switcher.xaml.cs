using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HunterPie.UI.Infrastructure;

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

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }
        public static new readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Switcher));


        public ICommand ToggleCommand
        {
            get => (ICommand)GetValue(ToggleCommandProperty);
            set => SetValue(ToggleCommandProperty, value);
        }
        public static readonly DependencyProperty ToggleCommandProperty =
            DependencyProperty.Register(nameof(ToggleCommand), typeof(ICommand), typeof(Switcher));


        public Switcher()
        {
            InitializeComponent();
            ToggleCommand = new RelayCommand(_ => true, _ => IsEnabled = !IsEnabled);
        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (ToggleCommand.CanExecute(IsEnabled))
            {
                ToggleCommand.Execute(!IsEnabled);
            }
        }

    }
}
