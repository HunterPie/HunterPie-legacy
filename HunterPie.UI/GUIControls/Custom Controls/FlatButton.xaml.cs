using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for FlatButton.xaml
    /// </summary>
    public partial class FlatButton : UserControl
    {
        public FlatButton()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set
            {
                if (Command != null)
                {
                    IsButtonEnabled = Command.CanExecute(null);
                }
                SetValue(CommandProperty, value);
                IsButtonEnabled = value.CanExecute(null);
                value.CanExecuteChanged += OnCanExecuteChanged;
            }
        }

        private void OnCanExecuteChanged(object sender, EventArgs e) => IsButtonEnabled = Command.CanExecute(null);

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FlatButton), new PropertyMetadata(null, OnCommandChanged));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(Command) && e.NewValue is ICommand command)
            {
                d.SetValue(IsButtonEnabledProperty, command.CanExecute(null));
            }
        }

        public static readonly DependencyProperty IsButtonEnabledProperty = DependencyProperty.Register(
            "IsButtonEnabled", typeof(bool), typeof(FlatButton), new PropertyMetadata(true, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public bool IsButtonEnabled
        {
            get { return (bool)GetValue(IsButtonEnabledProperty); }
            set { SetValue(IsButtonEnabledProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(FlatButton), new PropertyMetadata(default(string)));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ImageSource Icon
        {
            get => btnIcon.Source;
            set
            {
                btnIcon.Source = value;
            }
        }
    }
}
