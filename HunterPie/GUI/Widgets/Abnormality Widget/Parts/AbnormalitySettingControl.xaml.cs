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

namespace HunterPie.GUI.Widgets.Abnormality_Widget.Parts {
    /// <summary>
    /// Interaction logic for AbnormalitySettingControl.xaml
    /// </summary>
    public partial class AbnormalitySettingControl : UserControl {

        public new bool IsEnabled;
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
            this.AbnormCheck.Visibility = IsEnabled ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
