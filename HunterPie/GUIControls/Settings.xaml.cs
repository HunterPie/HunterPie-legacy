using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Core;

namespace HunterPie.GUIControls {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl {

        public static Settings _Instance;
        public static Settings Instance {
            get {
                if (_Instance == null) {
                    _Instance = new Settings();
                }
                return _Instance;
            }
        }

        public Settings() {
            InitializeComponent();
        }

        static public void RefreshSettingsUI() {
            var settings = UserSettings.PlayerConfig;
            var settingsUI = _Instance.SettingsBox;
            settingsUI.fullGamePath = settings.HunterPie.Launch.GamePath;
            settingsUI.fullLaunchArgs = settings.HunterPie.Launch.LaunchArgs;

            // HunterPie
            settingsUI.enableAutoUpdate.IsChecked = settings.HunterPie.Update.Enabled;
            settingsUI.branchesCombobox.SelectedItem = _Instance.SettingsBox.branchesCombobox.Items.Contains(settings.HunterPie.Update.Branch) ? settings.HunterPie.Update.Branch : "master";
            settingsUI.selectPathBttn.Content = settings.HunterPie.Launch.GamePath == "" ? "Select path" : settings.HunterPie.Launch.GamePath.Length > 15 ? "..." + settings.HunterPie.Launch.GamePath.Substring((settings.HunterPie.Launch.GamePath.Length / 2) - 10) : settings.HunterPie.Launch.GamePath;
            settingsUI.argsTextBox.Text = settings.HunterPie.Launch.LaunchArgs == "" ? "No arguments" : settings.HunterPie.Launch.LaunchArgs;
            settingsUI.enableCloseWhenExit.IsChecked = settings.HunterPie.Options.CloseWhenGameCloses;

            // Rich Presence
            settingsUI.enableRichPresence.IsChecked = settings.RichPresence.Enabled;
            
            // Overlay
            settingsUI.enableOverlay.IsChecked = settings.Overlay.Enabled;
            settingsUI.positionOverlayX.Text = settings.Overlay.Position[0].ToString();
            settingsUI.positionOverlayY.Text = settings.Overlay.Position[1].ToString();
            // Monsters
            settingsUI.enableMonsterComponent.IsChecked = settings.Overlay.MonstersComponent.Enabled;
            settingsUI.positionMonsterCompX.Text = settings.Overlay.MonstersComponent.Position[0].ToString();
            settingsUI.positionMonsterCompY.Text = settings.Overlay.MonstersComponent.Position[1].ToString();

            // Primary Mantle
            settingsUI.enablePrimaryMantle.IsChecked = settings.Overlay.PrimaryMantle.Enabled;
            settingsUI.primMantlePosX.Text = settings.Overlay.PrimaryMantle.Position[0].ToString();
            settingsUI.primMantlePosY.Text = settings.Overlay.PrimaryMantle.Position[1].ToString();
            settingsUI.primMantleColor.Text = settings.Overlay.PrimaryMantle.Color;

            // Secondary Mantle
            settingsUI.enableSecondaryMantle.IsChecked = settings.Overlay.SecondaryMantle.Enabled;
            settingsUI.secMantlePosX.Text = settings.Overlay.SecondaryMantle.Position[0].ToString();
            settingsUI.secMantlePosY.Text = settings.Overlay.SecondaryMantle.Position[1].ToString();
            settingsUI.secMantleColor.Text = settings.Overlay.SecondaryMantle.Color;

            // Harvest Box
            settingsUI.enableHarvestBoxComponent.IsChecked = settings.Overlay.HarvestBoxComponent.Enabled;
            settingsUI.harvestBoxPosX.Text = settings.Overlay.HarvestBoxComponent.Position[0].ToString();
            settingsUI.harvestBoxPosY.Text = settings.Overlay.HarvestBoxComponent.Position[1].ToString();
        }

        private void saveSettings_Click(object sender, RoutedEventArgs e) {
            var settings = UserSettings.PlayerConfig;
            var settingsUI = _Instance.SettingsBox;
            // HunterPie
            settings.HunterPie.Update.Enabled = (bool)settingsUI.enableAutoUpdate.IsChecked;
            settings.HunterPie.Update.Branch = (string)settingsUI.branchesCombobox.SelectedItem;
            settings.HunterPie.Launch.GamePath = settingsUI.fullGamePath;
            settings.HunterPie.Launch.LaunchArgs = settingsUI.fullLaunchArgs == "No arguments" ? "" : settingsUI.fullLaunchArgs;
            settings.HunterPie.Options.CloseWhenGameCloses = (bool)settingsUI.enableCloseWhenExit.IsChecked;

            // Rich Presence
            settings.RichPresence.Enabled = (bool)settingsUI.enableRichPresence.IsChecked;

            // Overlay
            settings.Overlay.Enabled = (bool)settingsUI.enableOverlay.IsChecked;
            settings.Overlay.Position[0] = int.Parse(settingsUI.positionOverlayX.Text);
            settings.Overlay.Position[1] = int.Parse(settingsUI.positionOverlayY.Text);
            // Monsters
            settings.Overlay.MonstersComponent.Enabled = (bool)settingsUI.enableMonsterComponent.IsChecked;
            settings.Overlay.MonstersComponent.Position[0] = int.Parse(settingsUI.positionMonsterCompX.Text);
            settings.Overlay.MonstersComponent.Position[1] = int.Parse(settingsUI.positionMonsterCompY.Text);

            // Primary Mantle
            settings.Overlay.PrimaryMantle.Enabled = (bool)settingsUI.enablePrimaryMantle.IsChecked;
            settings.Overlay.PrimaryMantle.Position[0] = int.Parse(settingsUI.primMantlePosX.Text);
            settings.Overlay.PrimaryMantle.Position[1] = int.Parse(settingsUI.primMantlePosY.Text);
            settings.Overlay.PrimaryMantle.Color = settingsUI.primMantleColor.Text;

            // Secondary Mantle
            settings.Overlay.SecondaryMantle.Enabled = (bool)settingsUI.enableSecondaryMantle.IsChecked;
            settings.Overlay.SecondaryMantle.Position[0] = int.Parse(settingsUI.secMantlePosX.Text);
            settings.Overlay.SecondaryMantle.Position[1] = int.Parse(settingsUI.secMantlePosY.Text);
            settings.Overlay.SecondaryMantle.Color = settingsUI.secMantleColor.Text;

            // Harvest Box
            settings.Overlay.HarvestBoxComponent.Enabled = (bool)settingsUI.enableHarvestBoxComponent.IsChecked;
            settings.Overlay.HarvestBoxComponent.Position[0] = int.Parse(settingsUI.harvestBoxPosX.Text);
            settings.Overlay.HarvestBoxComponent.Position[1] = int.Parse(settingsUI.harvestBoxPosY.Text);

            // and then save settings
            UserSettings.SaveNewConfig();
        }
    }
}
