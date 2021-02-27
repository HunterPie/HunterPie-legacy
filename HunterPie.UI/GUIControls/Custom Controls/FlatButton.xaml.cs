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
                    IsButtonEnabled = Command.CanExecute(CommandParameter);
                }
                SetValue(CommandProperty, value);
                IsButtonEnabled = value.CanExecute(CommandParameter);
                value.CanExecuteChanged += OnCanExecuteChanged;
            }
        }

        private void OnCanExecuteChanged(object sender, EventArgs e) => IsButtonEnabled = Command.CanExecute(CommandParameter);

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FlatButton), new PropertyMetadata(null, OnCommandChanged));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(FlatButton));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == nameof(Command) && e.NewValue is ICommand command)
            {
                d.SetValue(IsButtonEnabledProperty, command.CanExecute(d.GetValue(CommandParameterProperty)));
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

        public static readonly DependencyProperty IconBindingProperty = DependencyProperty.Register("IconBinding", typeof(ImageSource), typeof(FlatButton),
            new PropertyMetadata(default(ImageSource), IconChangedCallback));

        private static void IconChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ImageSource img)
            {
                ((FlatButton)d).btnIcon.Source = img;
            }
        }

        public ImageSource IconBinding
        {
            get { return (ImageSource)GetValue(IconBindingProperty); }
            set
            {
                SetValue(IconBindingProperty, value);
                this.btnIcon.Source = value;
            }
        }
    }
}
