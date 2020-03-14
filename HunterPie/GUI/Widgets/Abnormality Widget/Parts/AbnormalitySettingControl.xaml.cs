using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace HunterPie.GUI.Widgets.Abnormality_Widget.Parts {
    /// <summary>
    /// Interaction logic for AbnormalitySettingControl.xaml
    /// </summary>
    public partial class AbnormalitySettingControl : UserControl {
        private bool _IsEnabled { get; set; } = false;

        public new bool IsEnabled {
            get { return _IsEnabled; }
            set { _IsEnabled = value ; this.AbnormCheck.Visibility = value ? Visibility.Visible : Visibility.Hidden; }
        }
        public string InternalID;

        public AbnormalitySettingControl() {
            InitializeComponent();
        }

        public void SetAbnormalityInfo(ImageSource Icon, string Name, string InternalID, bool IsEnabled) {
            this.AbnormalityIcon.Source = Icon;
            this.InternalID = InternalID;
            this.ToolTip = Name;
            this.IsEnabled = IsEnabled;
            this.AbnormCheck.Visibility = IsEnabled ? Visibility.Visible : Visibility.Hidden;
        }

        private void OnMouseClick(object sender, MouseButtonEventArgs e) {
            IsEnabled = !IsEnabled;
        }
    }
}
