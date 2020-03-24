using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using HunterPie.Core;
using System;

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

        public void UninstallKeyboardHook() {
            _Instance?.SettingsBox.UnhookEvents();
        }

        static public void RefreshSettingsUI() {
            if (_Instance == null) return;
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
            settingsUI.switchEnableMinimizeToSystemTray.IsEnabled = settings.HunterPie.MinimizeToSystemTray;
            settingsUI.switchEnableStartMinimized.IsEnabled = settings.HunterPie.StartHunterPieMinimized;

            // Rich Presence
            settingsUI.switchEnableRichPresence.IsEnabled = settings.RichPresence.Enabled;
            settingsUI.switchShowMonsterHealth.IsEnabled = settings.RichPresence.ShowMonsterHealth;
            
            // Overlay
            settingsUI.switchEnableOverlay.IsEnabled = settings.Overlay.Enabled;
            settingsUI.DesiredFrameRateSlider.Value = settings.Overlay.DesiredAnimationFrameRate;
            settingsUI.DesignModeKeyCode.Content = KeyboardHookHelper.GetKeyboardKeyByID(settings.Overlay.ToggleDesignModeKey).ToString();
            settingsUI.ToggleOverlayHotKey.Content = settings.Overlay.ToggleOverlayKeybind;
            settingsUI.switchHardwareAcceleration.IsEnabled = settings.Overlay.EnableHardwareAcceleration;
            settingsUI.switchHideWhenUnfocused.IsEnabled = settings.Overlay.HideWhenGameIsUnfocused;
            settingsUI.positionOverlayX.Text = settings.Overlay.Position[0].ToString();
            settingsUI.positionOverlayY.Text = settings.Overlay.Position[1].ToString();

            // Monsters
            settingsUI.switchEnableMonsterComponent.IsEnabled = settings.Overlay.MonstersComponent.Enabled;
            settingsUI.MonsterShowModeSelection.SelectedIndex = settings.Overlay.MonstersComponent.ShowMonsterBarMode;
            settingsUI.ToggleMonsterBarModeHotKey.Content = settings.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey;
            settingsUI.MaxNumberOfPartsAtOnce.Text = settings.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce.ToString();
            settingsUI.MaxColumnsOfParts.Text = settings.Overlay.MonstersComponent.MaxPartColumns.ToString();
            settingsUI.MonsterBarDock.SelectedIndex = settings.Overlay.MonstersComponent.MonsterBarDock;
            settingsUI.positionMonsterCompX.Text = settings.Overlay.MonstersComponent.Position[0].ToString();
            settingsUI.positionMonsterCompY.Text = settings.Overlay.MonstersComponent.Position[1].ToString();
            settingsUI.switchEnableParts.IsEnabled = settings.Overlay.MonstersComponent.EnableMonsterParts;
            settingsUI.PartsCustomizer.IsEnabled = settingsUI.switchEnableParts.IsEnabled;
            settingsUI.switchEnableRemovableParts.IsEnabled = settings.Overlay.MonstersComponent.EnableRemovableParts;
            foreach (Custom_Controls.Switcher switcher in settingsUI.PartsCustomizer.Children) {
                if (settings.Overlay.MonstersComponent.EnabledPartGroups.Contains(switcher.Name.Replace("EnablePart", "").ToUpper())) {
                    switcher.IsEnabled = true;
                } else {
                    switcher.IsEnabled = false;
                }
            }
            settingsUI.HideSecondsTextbox.Text = settings.Overlay.MonstersComponent.SecondsToHideParts.ToString();
            settingsUI.switchEnableHideUnactiveParts.IsEnabled = settings.Overlay.MonstersComponent.HidePartsAfterSeconds;
            settingsUI.switchEnableMonsterWeakness.IsEnabled = settings.Overlay.MonstersComponent.ShowMonsterWeakness;

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
            settingsUI.switchAlwaysShow.IsEnabled = settings.Overlay.HarvestBoxComponent.AlwaysShow;
            settingsUI.harvestBoxPosX.Text = settings.Overlay.HarvestBoxComponent.Position[0].ToString();
            settingsUI.harvestBoxPosY.Text = settings.Overlay.HarvestBoxComponent.Position[1].ToString();

            // DPS Meter
            settingsUI.switchEnableDPSMeter.IsEnabled = settings.Overlay.DPSMeter.Enabled;
            settingsUI.switchEnableDPSWheneverPossible.IsEnabled = settings.Overlay.DPSMeter.ShowDPSWheneverPossible;
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
            settings.HunterPie.MinimizeToSystemTray = settingsUI.switchEnableMinimizeToSystemTray.IsEnabled;
            settings.HunterPie.StartHunterPieMinimized = settingsUI.switchEnableStartMinimized.IsEnabled;

            // Rich Presence
            settings.RichPresence.Enabled = settingsUI.switchEnableRichPresence.IsEnabled;
            settings.RichPresence.ShowMonsterHealth = settingsUI.switchShowMonsterHealth.IsEnabled;

            // Overlay
            settings.Overlay.Enabled = settingsUI.switchEnableOverlay.IsEnabled;
            settings.Overlay.DesiredAnimationFrameRate = (int)settingsUI.DesiredFrameRateSlider.Value;
            settings.Overlay.ToggleDesignModeKey = (int)settingsUI.KeyChoosen;
            settings.Overlay.ToggleOverlayKeybind = (string)settingsUI.ToggleOverlayHotKey.Content;
            settings.Overlay.EnableHardwareAcceleration = settingsUI.switchHardwareAcceleration.IsEnabled;
            settings.Overlay.HideWhenGameIsUnfocused = settingsUI.switchHideWhenUnfocused.IsEnabled;
            settings.Overlay.Position[0] = int.Parse(settingsUI.positionOverlayX.Text);
            settings.Overlay.Position[1] = int.Parse(settingsUI.positionOverlayY.Text);

            // Monsters
            settings.Overlay.MonstersComponent.Enabled = settingsUI.switchEnableMonsterComponent.IsEnabled;
            settings.Overlay.MonstersComponent.ShowMonsterBarMode = (byte)settingsUI.MonsterShowModeSelection.SelectedIndex;
            settings.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey = (string)settingsUI.ToggleMonsterBarModeHotKey.Content;
            settings.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce = int.Parse(settingsUI.MaxNumberOfPartsAtOnce.Text);
            settings.Overlay.MonstersComponent.MaxPartColumns = int.Parse(settingsUI.MaxColumnsOfParts.Text);
            settings.Overlay.MonstersComponent.MonsterBarDock = (byte)settingsUI.MonsterBarDock.SelectedIndex;
            settings.Overlay.MonstersComponent.Position[0] = int.Parse(settingsUI.positionMonsterCompX.Text);
            settings.Overlay.MonstersComponent.Position[1] = int.Parse(settingsUI.positionMonsterCompY.Text);
            settings.Overlay.MonstersComponent.EnableMonsterParts = settingsUI.switchEnableParts.IsEnabled;
            settings.Overlay.MonstersComponent.EnableRemovableParts = settingsUI.switchEnableRemovableParts.IsEnabled;
            List<string> EnabledParts = new List<string>();
            foreach (Custom_Controls.Switcher switcher in settingsUI.PartsCustomizer.Children) {
                if (switcher.IsEnabled)
                    EnabledParts.Add(switcher.Name.Replace("EnablePart", "").ToUpper());
            }
            settings.Overlay.MonstersComponent.EnabledPartGroups = EnabledParts.ToArray();
            settings.Overlay.MonstersComponent.HidePartsAfterSeconds = settingsUI.switchEnableHideUnactiveParts.IsEnabled;
            settings.Overlay.MonstersComponent.SecondsToHideParts = (int)Math.Min(Math.Max(long.Parse(settingsUI.HideSecondsTextbox.Text), 0), 10000);
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
            settings.Overlay.HarvestBoxComponent.AlwaysShow = settingsUI.switchAlwaysShow.IsEnabled;
            settings.Overlay.HarvestBoxComponent.Position[0] = int.Parse(settingsUI.harvestBoxPosX.Text);
            settings.Overlay.HarvestBoxComponent.Position[1] = int.Parse(settingsUI.harvestBoxPosY.Text);

            // DPS Meter
            settings.Overlay.DPSMeter.Enabled = settingsUI.switchEnableDPSMeter.IsEnabled;
            settings.Overlay.DPSMeter.ShowDPSWheneverPossible = settingsUI.switchEnableDPSWheneverPossible.IsEnabled;
            settings.Overlay.DPSMeter.Position[0] = int.Parse(settingsUI.DPSMeterPosX.Text);
            settings.Overlay.DPSMeter.Position[1] = int.Parse(settingsUI.DPSMeterPosY.Text);
            settings.Overlay.DPSMeter.PartyMembers[0].Color = settingsUI.FirstPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[1].Color = settingsUI.SecondPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[2].Color = settingsUI.ThirdPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[3].Color = settingsUI.FourthPlayerColor.Color;

            // Abnormality bars
            int i = 0;
            foreach (Custom_Controls.BuffBarSettingControl abnormBar in settingsUI.BuffTrays.Children) {
                settings.Overlay.AbnormalitiesWidget.BarPresets[i].Name = abnormBar.PresetName;
                settings.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled = abnormBar.Enabled;
                i++;
            }

            // and then save settings
            UserSettings.SaveNewConfig();
        }
    }
}
