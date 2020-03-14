using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for CNotification.xaml
    /// </summary>
    public partial class CNotification : UserControl {


        private DispatcherTimer VisibilityTimer;
        public ImageSource NIcon {
            get { return NotificationIcon.Source; }
            set { NotificationIcon.Source = value; }
        }
        public string NText {
            get { return NotificationText.Text; }
            set { NotificationText.Text = value; }
        }
        
        public CNotification() {
            InitializeComponent();
        }

        public void ShowNotification(string text=null, ImageSource icon=null) {
            if (this.Visibility == System.Windows.Visibility.Visible) return;
            this.Visibility = System.Windows.Visibility.Visible;
            VisibilityTimer = new DispatcherTimer() {
                Interval = new TimeSpan(0, 0, 2)
            };
            VisibilityTimer.Tick += ChangeVisibility;
            VisibilityTimer.Start();
        }

        private void ChangeVisibility(object source, EventArgs e) {
            VisibilityTimer.Stop();
            this.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
