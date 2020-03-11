using System.Windows.Controls;

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for BuffBarSettingControl.xaml
    /// </summary>
    public partial class BuffBarSettingControl : UserControl {
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

        public BuffBarSettingControl() {
            InitializeComponent();
        }
    }
}
