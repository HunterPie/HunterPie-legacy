using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for CNotification.xaml
    /// </summary>
    public partial class CNotification : UserControl
    {

        public ImageSource NIcon
        {
            get => (ImageSource)GetValue(NIconProperty);
            set => SetValue(NIconProperty, value);
        }

        // Using a DependencyProperty as the backing store for NIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NIconProperty =
            DependencyProperty.Register("NIcon", typeof(ImageSource), typeof(CNotification));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CNotification));

        public ImageSource FirstButtonImage
        {
            get => (ImageSource)GetValue(FirstButtonImageProperty);
            set => SetValue(FirstButtonImageProperty, value);
        }

        // Using a DependencyProperty as the backing store for FirstButtonImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstButtonImageProperty =
            DependencyProperty.Register("FirstButtonImage", typeof(ImageSource), typeof(CNotification));

        public ImageSource SecondButtonImage
        {
            get => (ImageSource)GetValue(SecondButtonImageProperty);
            set => SetValue(SecondButtonImageProperty, value);
        }

        // Using a DependencyProperty as the backing store for SecondButtonImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondButtonImageProperty =
            DependencyProperty.Register("SecondButtonImage", typeof(ImageSource), typeof(CNotification));

        public Visibility FirstButtonVisibility
        {
            get => (Visibility)GetValue(FirstButtonVisibilityProperty);
            set => SetValue(FirstButtonVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for FirstButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstButtonVisibilityProperty =
            DependencyProperty.Register("FirstButtonVisibility", typeof(Visibility), typeof(CNotification));

        public Visibility SecondButtonVisibility
        {
            get => (Visibility)GetValue(SecondButtonVisibilityProperty);
            set => SetValue(SecondButtonVisibilityProperty, value);
        }

        // Using a DependencyProperty as the backing store for SecondButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondButtonVisibilityProperty =
            DependencyProperty.Register("SecondButtonVisibility", typeof(Visibility), typeof(CNotification));

        public string FirstButtonText
        {
            get => (string)GetValue(FirstButtonTextProperty);
            set => SetValue(FirstButtonTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for  FirstButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstButtonTextProperty =
            DependencyProperty.Register("FirstButtonText", typeof(string), typeof(CNotification));

        public string SecondButtonText
        {
            get => (string)GetValue(SecondButtonTextProperty);
            set => SetValue(SecondButtonTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for SecondButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondButtonTextProperty =
            DependencyProperty.Register("SecondButtonText", typeof(string), typeof(CNotification));


        public int ShowTime { get; set; }
        public Action Callback1 { get; set; }
        public Action Callback2 { get; set; }
        private DispatcherTimer VisibilityTimer;

        public CNotification() => InitializeComponent();

        public void ShowNotification()
        {
            Visibility = Visibility.Visible;
            VisibilityTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, ShowTime)
            };
            VisibilityTimer.Tick += new EventHandler(Close);
            VisibilityTimer.Start();
        }

        private void Close(object source, EventArgs e)
        {
            VisibilityTimer?.Stop();
            VisibilityTimer = null;
            Visibility = Visibility.Collapsed;
            ((Panel)Parent).Children.Remove(this);

        }

        private void OnFirstButtonClick(object sender, RoutedEventArgs e) => Callback1();

        private void OnSecondButtonClick(object sender, RoutedEventArgs e) => Callback2();

        private void OnNotificationClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => Close(this, EventArgs.Empty);
    }
}
