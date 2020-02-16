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
            settingsUI.switchEnableAutoUpdate.IsEnabled = settings.HunterPie.Update.Enabled;
            settingsUI.branchesCombobox.SelectedItem = _Instance.SettingsBox.branchesCombobox.Items.Contains(settings.HunterPie.Update.Branch) ? settings.HunterPie.Update.Branch : "master";
            settingsUI.selectPathBttn.Content = settings.HunterPie.Launch.GamePath == "" ? "Select path" : settings.HunterPie.Launch.GamePath.Length > 15 ? "..." + settings.HunterPie.Launch.GamePath.Substring((settings.HunterPie.Launch.GamePath.Length / 2) - 10) : settings.HunterPie.Launch.GamePath;
            settingsUI.argsTextBox.Text = settings.HunterPie.Launch.LaunchArgs == "" ? "No arguments" : settings.HunterPie.Launch.LaunchArgs;
            settingsUI.switchEnableCloseWhenExit.IsEnabled = settings.HunterPie.Options.CloseWhenGameCloses;
            settingsUI.LanguageFilesCombobox.SelectedItem = settings.HunterPie.Language;

            // Rich Presence
            settingsUI.switchEnableRichPresence.IsEnabled = settings.RichPresence.Enabled;
            
            // Overlay
            settingsUI.switchEnableOverlay.IsEnabled = settings.Overlay.Enabled;
            settingsUI.switchHideWhenUnfocused.IsEnabled = settings.Overlay.HideWhenGameIsUnfocused;
            settingsUI.positionOverlayX.Text = settings.Overlay.Position[0].ToString();
            settingsUI.positionOverlayY.Text = settings.Overlay.Position[1].ToString();
            // Monsters
            settingsUI.switchEnableMonsterComponent.IsEnabled = settings.Overlay.MonstersComponent.Enabled;
            settingsUI.positionMonsterCompX.Text = settings.Overlay.MonstersComponent.Position[0].ToString();
            settingsUI.positionMonsterCompY.Text = settings.Overlay.MonstersComponent.Position[1].ToString();
            settingsUI.switchEnableMonsterWeakness.IsEnabled = settings.Overlay.MonstersComponent.Enabled;

            // Primary Mantle
            settingsUI.switchEnablePrimaryMantle.IsEnabled = settings.Overlay.PrimaryMantle.Enabled;
            settingsUI.primMantlePosX.Text = settings.Overlay.PrimaryMantle.Position[0].ToString();
            settingsUI.primMantlePosY.Text = settings.Overlay.PrimaryMantle.Position[1].ToString();
            settingsUI.PrimaryMantleColor.Color = settings.Overlay.PrimaryMantle.Color;

            // Secondary Mantle
            settingsUI.switchEnableSecondaryMantle.IsEnabled = settings.Overlay.SecondaryMantle.Enabled;
            settingsUI.secMantlePosX.Text = settings.Overlay.SecondaryMantle.Position[0].ToString();
            settingsUI.secMantlePosY.Text = settings.Overlay.SecondaryMantle.Position[1].ToString();
            settingsUI.SecondaryMantleColor.Color = settings.Overlay.SecondaryMantle.Color;

            // Harvest Box
            settingsUI.switchEnableHarvestBox.IsEnabled = settings.Overlay.HarvestBoxComponent.Enabled;
            settingsUI.harvestBoxPosX.Text = settings.Overlay.HarvestBoxComponent.Position[0].ToString();
            settingsUI.harvestBoxPosY.Text = settings.Overlay.HarvestBoxComponent.Position[1].ToString();

            // DPS Meter
            settingsUI.switchEnableDPSMeter.IsEnabled = settings.Overlay.DPSMeter.Enabled;
            settingsUI.DPSMeterPosX.Text = settings.Overlay.DPSMeter.Position[0].ToString();
            settingsUI.DPSMeterPosY.Text = settings.Overlay.DPSMeter.Position[1].ToString();
            settingsUI.FirstPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[0].Color;
            settingsUI.SecondPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[1].Color;
            settingsUI.ThirdPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[2].Color;
            settingsUI.FourthPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[3].Color;
        }

        private void saveSettings_Click(object sender, RoutedEventArgs e) {
            var settings = UserSettings.PlayerConfig;
            var settingsUI = _Instance.SettingsBox;
            // HunterPie
            settings.HunterPie.Update.Enabled = settingsUI.switchEnableAutoUpdate.IsEnabled;
            settings.HunterPie.Update.Branch = (string)settingsUI.branchesCombobox.SelectedItem;
            settings.HunterPie.Launch.GamePath = settingsUI.fullGamePath;
            settings.HunterPie.Launch.LaunchArgs = settingsUI.fullLaunchArgs == "No arguments" ? "" : settingsUI.fullLaunchArgs;
            settings.HunterPie.Options.CloseWhenGameCloses = settingsUI.switchEnableCloseWhenExit.IsEnabled;
            settings.HunterPie.Language = (string)settingsUI.LanguageFilesCombobox.SelectedItem;

            // Rich Presence
            settings.RichPresence.Enabled = settingsUI.switchEnableRichPresence.IsEnabled;

            // Overlay
            settings.Overlay.Enabled = settingsUI.switchEnableOverlay.IsEnabled;
            settings.Overlay.HideWhenGameIsUnfocused = settingsUI.switchHideWhenUnfocused.IsEnabled;
            settings.Overlay.Position[0] = int.Parse(settingsUI.positionOverlayX.Text);
            settings.Overlay.Position[1] = int.Parse(settingsUI.positionOverlayY.Text);
            // Monsters
            settings.Overlay.MonstersComponent.Enabled = settingsUI.switchEnableMonsterComponent.IsEnabled;
            settings.Overlay.MonstersComponent.Position[0] = int.Parse(settingsUI.positionMonsterCompX.Text);
            settings.Overlay.MonstersComponent.Position[1] = int.Parse(settingsUI.positionMonsterCompY.Text);
            settings.Overlay.MonstersComponent.ShowMonsterWeakness = settingsUI.switchEnableMonsterWeakness.IsEnabled;

            // Primary Mantle
            settings.Overlay.PrimaryMantle.Enabled = settingsUI.switchEnablePrimaryMantle.IsEnabled;
            settings.Overlay.PrimaryMantle.Position[0] = int.Parse(settingsUI.primMantlePosX.Text);
            settings.Overlay.PrimaryMantle.Position[1] = int.Parse(settingsUI.primMantlePosY.Text);
            settings.Overlay.PrimaryMantle.Color = settingsUI.PrimaryMantleColor.Color;

            // Secondary Mantle
            settings.Overlay.SecondaryMantle.Enabled = settingsUI.switchEnableSecondaryMantle.IsEnabled;
            settings.Overlay.SecondaryMantle.Position[0] = int.Parse(settingsUI.secMantlePosX.Text);
            settings.Overlay.SecondaryMantle.Position[1] = int.Parse(settingsUI.secMantlePosY.Text);
            settings.Overlay.SecondaryMantle.Color = settingsUI.SecondaryMantleColor.Color;

            // Harvest Box
            settings.Overlay.HarvestBoxComponent.Enabled = settingsUI.switchEnableHarvestBox.IsEnabled;
            settings.Overlay.HarvestBoxComponent.Position[0] = int.Parse(settingsUI.harvestBoxPosX.Text);
            settings.Overlay.HarvestBoxComponent.Position[1] = int.Parse(settingsUI.harvestBoxPosY.Text);

            // DPS Meter
            settings.Overlay.DPSMeter.Enabled = settingsUI.switchEnableDPSMeter.IsEnabled;
            settings.Overlay.DPSMeter.Position[0] = int.Parse(settingsUI.DPSMeterPosX.Text);
            settings.Overlay.DPSMeter.Position[1] = int.Parse(settingsUI.DPSMeterPosY.Text);
            settings.Overlay.DPSMeter.PartyMembers[0].Color = settingsUI.FirstPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[1].Color = settingsUI.SecondPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[2].Color = settingsUI.ThirdPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[3].Color = settingsUI.FourthPlayerColor.Color;


            // and then save settings
            UserSettings.SaveNewConfig();
        }
    }
}
