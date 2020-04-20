using System.Windows.Controls;
using HunterPie.GUI.Widgets.Abnormality_Widget;

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for BuffBarSettingControl.xaml
    /// </summary>
    public partial class BuffBarSettingControl : UserControl {
        AbnormalityTraySettings TraySettingsWindow;
        public string PresetName {
            get { return this.BarPresetName.Text; }
            set {
                this.BarPresetName.Text = value;
            }
        }
        public bool Enabled {
            get { return this.SwitchEnableBar.IsEnabled; }
            set {
                this.SwitchEnableBar.IsEnabled = value;
            }
        }
        public int TrayIndex;

        public BuffBarSettingControl() {
            InitializeComponent();
        }

        private void OnBuffTraySettingClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (TraySettingsWindow == null || TraySettingsWindow.IsClosed) {
                TraySettingsWindow = new AbnormalityTraySettings(trayIndex: TrayIndex);
            }
            TraySettingsWindow.Show();
        }
    }
}
