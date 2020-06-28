using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Core;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {

        private static Settings _Instance;
        public static Settings Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Settings();
                }
                return _Instance;
            }
        }

        public Settings() => InitializeComponent();

        public void UninstallKeyboardHook() => _Instance?.SettingsBox.UnhookEvents();

        static public void Destroy()
        {
            if (_Instance == null) return;
            _Instance.UninstallKeyboardHook();
            _Instance.SettingsBox = null;
            _Instance = null;
        }

        static public void RefreshSettingsUI()
        {
            if (_Instance == null) return;
            var settings = UserSettings.PlayerConfig;
            var settingsUI = _Instance.SettingsBox;
            settingsUI.fullGamePath = settings.HunterPie.Launch.GamePath;
            settingsUI.fullLaunchArgs = settings.HunterPie.Launch.LaunchArgs;

            // HunterPie
            settingsUI.switchEnableAutoUpdate.IsEnabled = settings.HunterPie.Update.Enabled;
            settingsUI.branchesCombobox.SelectedItem = _Instance.SettingsBox.branchesCombobox.Items.Contains(settings.HunterPie.Update.Branch) ? settings.HunterPie.Update.Branch : "master";
            settingsUI.ThemeFilesCombobox.SelectedItem = settings.HunterPie.Theme;
            settingsUI.selectPathBttn.Content = settings.HunterPie.Launch.GamePath == "" ? GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_GAMEPATH_UNSELECTED']") : settings.HunterPie.Launch.GamePath.Length > 15 ? "..." + settings.HunterPie.Launch.GamePath.Substring((settings.HunterPie.Launch.GamePath.Length / 2) - 10) : settings.HunterPie.Launch.GamePath;
            settingsUI.argsTextBox.Text = settings.HunterPie.Launch.LaunchArgs == "" ? GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_LAUNCHARGS_NOARGS']") : settings.HunterPie.Launch.LaunchArgs;
            settingsUI.switchEnableCloseWhenExit.IsEnabled = settings.HunterPie.Options.CloseWhenGameCloses;
            settingsUI.LanguageFilesCombobox.SelectedItem = settings.HunterPie.Language;
            settingsUI.switchEnableMinimizeToSystemTray.IsEnabled = settings.HunterPie.MinimizeToSystemTray;
            settingsUI.switchEnableStartMinimized.IsEnabled = settings.HunterPie.StartHunterPieMinimized;

            // Debug
            settingsUI.switchEnableDebugMessages.IsEnabled = settings.HunterPie.Debug.ShowDebugMessages;
            settingsUI.switchEnableUnknownStatuses.IsEnabled = settings.HunterPie.Debug.ShowUnknownStatuses;
            settingsUI.switchEnableLoadMonsterData.IsEnabled = settings.HunterPie.Debug.LoadCustomMonsterData;

            // Rich Presence
            settingsUI.switchEnableRichPresence.IsEnabled = settings.RichPresence.Enabled;
            settingsUI.switchShowMonsterHealth.IsEnabled = settings.RichPresence.ShowMonsterHealth;
            settingsUI.switchLetPeopleJoinSession.IsEnabled = settings.RichPresence.LetPeopleJoinSession;

            // Overlay
            settingsUI.switchEnableOverlay.IsEnabled = settings.Overlay.Enabled;
            settingsUI.DesiredFrameRateSlider.Value = settings.Overlay.DesiredAnimationFrameRate;
            settingsUI.DesiredScanPerSecond.Value = settings.Overlay.GameScanDelay;
            //settingsUI.DesignModeKeyCode.Content = KeyboardHookHelper.GetKeyboardKeyByID(settings.Overlay.ToggleDesignModeKey).ToString();
            settingsUI.ToggleDesignHotkey.HotKey = settings.Overlay.ToggleDesignKeybind;
            settingsUI.ToggleOverlayHotkey.HotKey = settings.Overlay.ToggleOverlayKeybind;
            settingsUI.switchHardwareAcceleration.IsEnabled = settings.Overlay.EnableHardwareAcceleration;
            settingsUI.switchHideWhenUnfocused.IsEnabled = settings.Overlay.HideWhenGameIsUnfocused;
            settingsUI.OverlayPosition.X = settings.Overlay.Position[0];
            settingsUI.OverlayPosition.Y = settings.Overlay.Position[1];

            // Monsters
            settingsUI.switchEnableMonsterComponent.IsEnabled = settings.Overlay.MonstersComponent.Enabled;
            settingsUI.HealthTextFormat.Text = settings.Overlay.MonstersComponent.HealthTextFormat;
            settingsUI.switchUseLockon.IsEnabled = settings.Overlay.MonstersComponent.UseLockonInsteadOfPin;
            settingsUI.MonsterShowModeSelection.SelectedIndex = settings.Overlay.MonstersComponent.ShowMonsterBarMode;
            settingsUI.ToggleMonsterBarModeHotKey.HotKey = settings.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey;
            settingsUI.MaxNumberOfPartsAtOnce.Value = settings.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce;
            settingsUI.MaxColumnsOfParts.Value = settings.Overlay.MonstersComponent.MaxPartColumns;
            settingsUI.MonsterBarDock.SelectedIndex = settings.Overlay.MonstersComponent.MonsterBarDock;
            settingsUI.MonstersPosition.X = settings.Overlay.MonstersComponent.Position[0];
            settingsUI.MonstersPosition.Y = settings.Overlay.MonstersComponent.Position[1];
            settingsUI.switchEnableParts.IsEnabled = settings.Overlay.MonstersComponent.EnableMonsterParts;
            settingsUI.PartsCustomizer.IsEnabled = settingsUI.switchEnableParts.IsEnabled;
            settingsUI.switchEnableAilments.IsEnabled = settings.Overlay.MonstersComponent.EnableMonsterAilments;
            settingsUI.switchEnableRemovableParts.IsEnabled = settings.Overlay.MonstersComponent.EnableRemovableParts;
            foreach (Custom_Controls.Switcher switcher in settingsUI.PartsCustomizer.Children)
            {
                if (settings.Overlay.MonstersComponent.EnabledPartGroups.Contains(switcher.Name.Replace("EnablePart", "").ToUpper()))
                {
                    switcher.IsEnabled = true;
                }
                else
                {
                    switcher.IsEnabled = false;
                }
            }
            settingsUI.HideSeconds.Value = settings.Overlay.MonstersComponent.SecondsToHideParts;
            settingsUI.switchEnableHideUnactiveParts.IsEnabled = settings.Overlay.MonstersComponent.HidePartsAfterSeconds;
            settingsUI.switchEnableMonsterWeakness.IsEnabled = settings.Overlay.MonstersComponent.ShowMonsterWeakness;
            settingsUI.MonsterComponentOpacity.Value = settings.Overlay.MonstersComponent.Opacity;


            // Primary Mantle
            settingsUI.switchEnablePrimaryMantle.IsEnabled = settings.Overlay.PrimaryMantle.Enabled;
            settingsUI.PrimaryMantlePosition.X = settings.Overlay.PrimaryMantle.Position[0];
            settingsUI.PrimaryMantlePosition.Y = settings.Overlay.PrimaryMantle.Position[1];
            settingsUI.PrimaryMantleColor.Color = settings.Overlay.PrimaryMantle.Color;
            settingsUI.PrimaryMantleComponentOpacity.Value = settings.Overlay.PrimaryMantle.Opacity;

            // Secondary Mantle
            settingsUI.switchEnableSecondaryMantle.IsEnabled = settings.Overlay.SecondaryMantle.Enabled;
            settingsUI.SecondaryMantlePosition.X = settings.Overlay.SecondaryMantle.Position[0];
            settingsUI.SecondaryMantlePosition.Y = settings.Overlay.SecondaryMantle.Position[1];
            settingsUI.SecondaryMantleColor.Color = settings.Overlay.SecondaryMantle.Color;
            settingsUI.SecondaryMantleComponentOpacity.Value = settings.Overlay.SecondaryMantle.Opacity;

            // Harvest Box
            settingsUI.switchEnableHarvestBox.IsEnabled = settings.Overlay.HarvestBoxComponent.Enabled;
            settingsUI.switchAlwaysShow.IsEnabled = settings.Overlay.HarvestBoxComponent.AlwaysShow;
            settingsUI.switchShowSteamTracker.IsEnabled = settings.Overlay.HarvestBoxComponent.ShowSteamTracker;
            settingsUI.switchShowArgosyTracker.IsEnabled = settings.Overlay.HarvestBoxComponent.ShowArgosyTracker;
            settingsUI.switchShowTailraidersTracker.IsEnabled = settings.Overlay.HarvestBoxComponent.ShowTailraidersTracker;
            settingsUI.switchHarvestCompactMode.IsEnabled = settings.Overlay.HarvestBoxComponent.CompactMode;
            settingsUI.HarvestBoxPosition.X = settings.Overlay.HarvestBoxComponent.Position[0];
            settingsUI.HarvestBoxPosition.Y = settings.Overlay.HarvestBoxComponent.Position[1];
            settingsUI.HarvestBoxBackgroundOpacity.Value = settings.Overlay.HarvestBoxComponent.BackgroundOpacity;
            settingsUI.HarvestBoxComponentOpacity.Value = settings.Overlay.HarvestBoxComponent.Opacity;

            // DPS Meter
            settingsUI.switchEnableDPSMeter.IsEnabled = settings.Overlay.DPSMeter.Enabled;
            settingsUI.switchEnableTotalDamage.IsEnabled = settings.Overlay.DPSMeter.ShowTotalDamage;
            settingsUI.switchEnableDPSWheneverPossible.IsEnabled = settings.Overlay.DPSMeter.ShowDPSWheneverPossible;
            settingsUI.switchEnableTimer.IsEnabled = settings.Overlay.DPSMeter.ShowTimer;
            settingsUI.switchEnableTimerInExpeditions.IsEnabled = settings.Overlay.DPSMeter.ShowTimerInExpeditions;
            settingsUI.switchEnableOnlyTimer.IsEnabled = settings.Overlay.DPSMeter.ShowOnlyTimer;
            settingsUI.switchEnableOnlyMyself.IsEnabled = settings.Overlay.DPSMeter.ShowOnlyMyself;
            settingsUI.DamageMeterPosition.X = settings.Overlay.DPSMeter.Position[0];
            settingsUI.DamageMeterPosition.Y = settings.Overlay.DPSMeter.Position[1];
            settingsUI.FirstPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[0].Color;
            settingsUI.SecondPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[1].Color;
            settingsUI.ThirdPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[2].Color;
            settingsUI.FourthPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[3].Color;
            settingsUI.DamageBackgroundOpacity.Value = settings.Overlay.DPSMeter.BackgroundOpacity;
            settingsUI.DamageComponentOpacity.Value = settings.Overlay.DPSMeter.Opacity;

            // Classes Widget
            settingsUI.switchDualBladesHelper.IsEnabled = settings.Overlay.ClassesWidget.DualBladesHelper.Enabled;
            settingsUI.switchLongswordHelper.IsEnabled = settings.Overlay.ClassesWidget.LongSwordHelper.Enabled;
            settingsUI.switchHammerHelper.IsEnabled = settings.Overlay.ClassesWidget.HammerHelper.Enabled;
            settingsUI.switchLanceHelper.IsEnabled = settings.Overlay.ClassesWidget.LanceHelper.Enabled;
            settingsUI.switchGunLanceHelper.IsEnabled = settings.Overlay.ClassesWidget.GunLanceHelper.Enabled;
            settingsUI.switchSwitchAxeHelper.IsEnabled = settings.Overlay.ClassesWidget.SwitchAxeHelper.Enabled;
            settingsUI.switchChargeBladeHelper.IsEnabled = settings.Overlay.ClassesWidget.ChargeBladeHelper.Enabled;
            settingsUI.switchInsectGlaiveHelper.IsEnabled = settings.Overlay.ClassesWidget.InsectGlaiveHelper.Enabled;
            settingsUI.switchBowHelper.IsEnabled = settings.Overlay.ClassesWidget.BowHelper.Enabled;


        }

        private void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = UserSettings.PlayerConfig;
            var settingsUI = _Instance.SettingsBox;
            // HunterPie
            settings.HunterPie.Update.Enabled = settingsUI.switchEnableAutoUpdate.IsEnabled;
            settings.HunterPie.Update.Branch = (string)settingsUI.branchesCombobox.SelectedItem;
            settings.HunterPie.Theme = (string)settingsUI.ThemeFilesCombobox.SelectedItem;
            settings.HunterPie.Launch.GamePath = settingsUI.fullGamePath;
            settings.HunterPie.Launch.LaunchArgs = settingsUI.fullLaunchArgs == GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_LAUNCHARGS_NOARGS']") ? "" : settingsUI.fullLaunchArgs;
            settings.HunterPie.Options.CloseWhenGameCloses = settingsUI.switchEnableCloseWhenExit.IsEnabled;
            settings.HunterPie.Language = (string)settingsUI.LanguageFilesCombobox.SelectedItem;
            settings.HunterPie.MinimizeToSystemTray = settingsUI.switchEnableMinimizeToSystemTray.IsEnabled;
            settings.HunterPie.StartHunterPieMinimized = settingsUI.switchEnableStartMinimized.IsEnabled;

            // Debug
            settings.HunterPie.Debug.ShowDebugMessages = settingsUI.switchEnableDebugMessages.IsEnabled;
            settings.HunterPie.Debug.ShowUnknownStatuses = settingsUI.switchEnableUnknownStatuses.IsEnabled;
            settings.HunterPie.Debug.LoadCustomMonsterData = settingsUI.switchEnableLoadMonsterData.IsEnabled;

            // Rich Presence
            settings.RichPresence.Enabled = settingsUI.switchEnableRichPresence.IsEnabled;
            settings.RichPresence.ShowMonsterHealth = settingsUI.switchShowMonsterHealth.IsEnabled;
            settings.RichPresence.LetPeopleJoinSession = settingsUI.switchLetPeopleJoinSession.IsEnabled;

            // Overlay
            settings.Overlay.Enabled = settingsUI.switchEnableOverlay.IsEnabled;
            settings.Overlay.DesiredAnimationFrameRate = (int)settingsUI.DesiredFrameRateSlider.Value;
            settings.Overlay.GameScanDelay = (int)settingsUI.DesiredScanPerSecond.Value;
            //settings.Overlay.ToggleDesignModeKey = (int)settingsUI.KeyChoosen;
            settings.Overlay.ToggleDesignKeybind = settingsUI.ToggleDesignHotkey.HotKey;
            settings.Overlay.ToggleOverlayKeybind = settingsUI.ToggleOverlayHotkey.HotKey;
            settings.Overlay.EnableHardwareAcceleration = settingsUI.switchHardwareAcceleration.IsEnabled;
            settings.Overlay.HideWhenGameIsUnfocused = settingsUI.switchHideWhenUnfocused.IsEnabled;
            settings.Overlay.Position[0] = settingsUI.OverlayPosition.X;
            settings.Overlay.Position[1] = settingsUI.OverlayPosition.Y;

            // Monsters
            settings.Overlay.MonstersComponent.Enabled = settingsUI.switchEnableMonsterComponent.IsEnabled;
            settings.Overlay.MonstersComponent.HealthTextFormat = settingsUI.HealthTextFormat.Text;
            settings.Overlay.MonstersComponent.UseLockonInsteadOfPin = settingsUI.switchUseLockon.IsEnabled;
            settings.Overlay.MonstersComponent.ShowMonsterBarMode = (byte)settingsUI.MonsterShowModeSelection.SelectedIndex;
            settings.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey = settingsUI.ToggleMonsterBarModeHotKey.HotKey;
            settings.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce = (int)settingsUI.MaxNumberOfPartsAtOnce.Value;
            settings.Overlay.MonstersComponent.MaxPartColumns = (int)settingsUI.MaxColumnsOfParts.Value;
            settings.Overlay.MonstersComponent.MonsterBarDock = (byte)settingsUI.MonsterBarDock.SelectedIndex;
            settings.Overlay.MonstersComponent.Position[0] = settingsUI.MonstersPosition.X;
            settings.Overlay.MonstersComponent.Position[1] = settingsUI.MonstersPosition.Y;
            settings.Overlay.MonstersComponent.EnableMonsterParts = settingsUI.switchEnableParts.IsEnabled;
            settings.Overlay.MonstersComponent.EnableRemovableParts = settingsUI.switchEnableRemovableParts.IsEnabled;
            settings.Overlay.MonstersComponent.EnableMonsterAilments = settingsUI.switchEnableAilments.IsEnabled;
            List<string> EnabledParts = new List<string>();
            foreach (Custom_Controls.Switcher switcher in settingsUI.PartsCustomizer.Children)
            {
                if (switcher.IsEnabled)
                    EnabledParts.Add(switcher.Name.Replace("EnablePart", "").ToUpper());
            }
            settings.Overlay.MonstersComponent.EnabledPartGroups = EnabledParts.ToArray();
            settings.Overlay.MonstersComponent.HidePartsAfterSeconds = settingsUI.switchEnableHideUnactiveParts.IsEnabled;
            settings.Overlay.MonstersComponent.SecondsToHideParts = Math.Min(Math.Max((int)settingsUI.HideSeconds.Value, 0), 10000);
            settings.Overlay.MonstersComponent.ShowMonsterWeakness = settingsUI.switchEnableMonsterWeakness.IsEnabled;
            settings.Overlay.MonstersComponent.Opacity = (float)settingsUI.MonsterComponentOpacity.Value;

            // Primary Mantle
            settings.Overlay.PrimaryMantle.Enabled = settingsUI.switchEnablePrimaryMantle.IsEnabled;
            settings.Overlay.PrimaryMantle.Position[0] = settingsUI.PrimaryMantlePosition.X;
            settings.Overlay.PrimaryMantle.Position[1] = settingsUI.PrimaryMantlePosition.Y;
            settings.Overlay.PrimaryMantle.Color = settingsUI.PrimaryMantleColor.Color;
            settings.Overlay.PrimaryMantle.Opacity = (float)settingsUI.PrimaryMantleComponentOpacity.Value;

            // Secondary Mantle
            settings.Overlay.SecondaryMantle.Enabled = settingsUI.switchEnableSecondaryMantle.IsEnabled;
            settings.Overlay.SecondaryMantle.Position[0] = settingsUI.SecondaryMantlePosition.X;
            settings.Overlay.SecondaryMantle.Position[1] = settingsUI.SecondaryMantlePosition.Y;
            settings.Overlay.SecondaryMantle.Color = settingsUI.SecondaryMantleColor.Color;
            settings.Overlay.SecondaryMantle.Opacity = (float)settingsUI.SecondaryMantleComponentOpacity.Value;

            // Harvest Box
            settings.Overlay.HarvestBoxComponent.Enabled = settingsUI.switchEnableHarvestBox.IsEnabled;
            settings.Overlay.HarvestBoxComponent.AlwaysShow = settingsUI.switchAlwaysShow.IsEnabled;
            settings.Overlay.HarvestBoxComponent.ShowSteamTracker = settingsUI.switchShowSteamTracker.IsEnabled;
            settings.Overlay.HarvestBoxComponent.ShowArgosyTracker = settingsUI.switchShowArgosyTracker.IsEnabled;
            settings.Overlay.HarvestBoxComponent.ShowTailraidersTracker = settingsUI.switchShowTailraidersTracker.IsEnabled;
            settings.Overlay.HarvestBoxComponent.CompactMode = settingsUI.switchHarvestCompactMode.IsEnabled;
            settings.Overlay.HarvestBoxComponent.Position[0] = settingsUI.HarvestBoxPosition.X;
            settings.Overlay.HarvestBoxComponent.Position[1] = settingsUI.HarvestBoxPosition.Y;
            settings.Overlay.HarvestBoxComponent.BackgroundOpacity = (float)settingsUI.HarvestBoxBackgroundOpacity.Value;
            settings.Overlay.HarvestBoxComponent.Opacity = (float)settingsUI.HarvestBoxComponentOpacity.Value;


            // DPS Meter
            settings.Overlay.DPSMeter.Enabled = settingsUI.switchEnableDPSMeter.IsEnabled;
            settings.Overlay.DPSMeter.ShowTotalDamage = settingsUI.switchEnableTotalDamage.IsEnabled;
            settings.Overlay.DPSMeter.ShowDPSWheneverPossible = settingsUI.switchEnableDPSWheneverPossible.IsEnabled;
            settings.Overlay.DPSMeter.ShowTimer = settingsUI.switchEnableTimer.IsEnabled;
            settings.Overlay.DPSMeter.ShowTimerInExpeditions = settingsUI.switchEnableTimerInExpeditions.IsEnabled;
            settings.Overlay.DPSMeter.ShowOnlyTimer = settingsUI.switchEnableOnlyTimer.IsEnabled;
            settings.Overlay.DPSMeter.ShowOnlyMyself = settingsUI.switchEnableOnlyMyself.IsEnabled;
            settings.Overlay.DPSMeter.Position[0] = settingsUI.DamageMeterPosition.X;
            settings.Overlay.DPSMeter.Position[1] = settingsUI.DamageMeterPosition.Y;
            settings.Overlay.DPSMeter.PartyMembers[0].Color = settingsUI.FirstPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[1].Color = settingsUI.SecondPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[2].Color = settingsUI.ThirdPlayerColor.Color;
            settings.Overlay.DPSMeter.PartyMembers[3].Color = settingsUI.FourthPlayerColor.Color;
            settings.Overlay.DPSMeter.BackgroundOpacity = (float)settingsUI.DamageBackgroundOpacity.Value;
            settings.Overlay.DPSMeter.Opacity = (float)settingsUI.DamageComponentOpacity.Value;

            // Classes Widget
            settings.Overlay.ClassesWidget.DualBladesHelper.Enabled = settingsUI.switchDualBladesHelper.IsEnabled;
            settings.Overlay.ClassesWidget.LongSwordHelper.Enabled = settingsUI.switchLongswordHelper.IsEnabled;
            settings.Overlay.ClassesWidget.HammerHelper.Enabled = settingsUI.switchHammerHelper.IsEnabled;
            settings.Overlay.ClassesWidget.LanceHelper.Enabled = settingsUI.switchLanceHelper.IsEnabled;
            settings.Overlay.ClassesWidget.GunLanceHelper.Enabled = settingsUI.switchGunLanceHelper.IsEnabled;
            settings.Overlay.ClassesWidget.SwitchAxeHelper.Enabled = settingsUI.switchSwitchAxeHelper.IsEnabled;
            settings.Overlay.ClassesWidget.ChargeBladeHelper.Enabled = settingsUI.switchChargeBladeHelper.IsEnabled;
            settings.Overlay.ClassesWidget.InsectGlaiveHelper.Enabled = settingsUI.switchInsectGlaiveHelper.IsEnabled;
            settings.Overlay.ClassesWidget.BowHelper.Enabled = settingsUI.switchBowHelper.IsEnabled;

            // Abnormality bars
            int i = 0;
            foreach (Custom_Controls.BuffBarSettingControl abnormBar in settingsUI.BuffTrays.Children)
            {
                settings.Overlay.AbnormalitiesWidget.BarPresets[i].Name = abnormBar.PresetName;
                settings.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled = abnormBar.Enabled;
                i++;
            }

            // and then save settings
            UserSettings.SaveNewConfig();
        }
    }
}
