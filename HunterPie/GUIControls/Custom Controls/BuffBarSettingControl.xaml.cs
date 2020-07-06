using System.Linq;
using System.Windows.Controls;
using HunterPie.GUI.Widgets.Abnormality_Widget;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for BuffBarSettingControl.xaml
    /// </summary>
    public partial class BuffBarSettingControl : UserControl
    {

        public string PresetName
        {
            get => BarPresetName.Text;
            set => BarPresetName.Text = value;
        }
        public bool Enabled
        {
            get => SwitchEnableBar.IsEnabled;
            set => SwitchEnableBar.IsEnabled = value;
        }
        public int TrayIndex;

        public BuffBarSettingControl() => InitializeComponent();

        private void OnBuffTraySettingClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            bool SettingsWindowIsOpen = App.Current.Windows.Cast<System.Windows.Window>()
                .Where(w => w.Title == "Abnormality Tray Settings")
                .Count() > 0;

            if (SettingsWindowIsOpen) return;

            AbnormalityTraySettings traySettingsWindow = new AbnormalityTraySettings(TrayIndex);
            traySettingsWindow.Show();
        }

    }
}
