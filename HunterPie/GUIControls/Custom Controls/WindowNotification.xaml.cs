using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for WindowNotification.xaml
    /// </summary>
    public partial class WindowNotification : UserControl {



        public ImageSource Icon {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(WindowNotification));



        public bool IsAccepted {
            get { return (bool)GetValue(IsAcceptedProperty); }
            set { SetValue(IsAcceptedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAccepted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAcceptedProperty =
            DependencyProperty.Register("IsAccepted", typeof(bool), typeof(WindowNotification));

        public string Link {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Link.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Link", typeof(string), typeof(WindowNotification));

        public delegate void ConfirmationEvents(object source, EventArgs args);
        public event ConfirmationEvents OnAccept;
        public event ConfirmationEvents OnReject;

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(WindowNotification));



        public WindowNotification() {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e) {
            OnAccept?.Invoke(this, EventArgs.Empty);
            IsAccepted = true;
            
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e) {
            this.Visibility = Visibility.Collapsed;
            IsAccepted = false;
            OnReject?.Invoke(this, EventArgs.Empty);
        }

        private void TextBox_MouseButtonDown(object sender, MouseButtonEventArgs e) {
            e.Handled = false;
            Clipboard.SetText(Link);
            Box.SelectAll();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.Visibility = Visibility.Collapsed;
            this.IsAccepted = false;
        }
    }
}
