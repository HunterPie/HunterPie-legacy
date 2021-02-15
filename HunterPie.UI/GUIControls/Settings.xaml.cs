using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Core;
using HunterPie.Core.Enums;
using HunterPie.Logger;
using HunterPie.Settings;

namespace HunterPie.GUIControls
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        private readonly ObservableCollection<ISettingsTab> settingBlocks;

        public static Settings Instance
        {
            get; private set;
        }

        public static Settings Instantiate(ObservableCollection<ISettingsTab> settingBlocks)
        {
            if (Instance != null)
            {
                throw new Exception("Instantiate shouldn't be created multiple times.");
            }
            return Instance = new Settings(settingBlocks);
        }

        private Settings(ObservableCollection<ISettingsTab> settingBlocks)
        {
            InitializeComponent();
            this.settingBlocks = settingBlocks;
            this.settingBlocks.CollectionChanged += SettingBlocksOnCollectionChanged;
        }

        private void SettingBlocksOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (var block in e.NewItems.Cast<ISettingsTab>())
                        {
                            SettingsBox.AddSettingsBlock(block);
                        }
                    }

                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var block in e.OldItems.Cast<ISettingsTab>())
                        {
                            SettingsBox.RemoveSettingsBlock(block);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Error(ex.ToString());
                }
            });
        }

        public void UninstallKeyboardHook() => Instance?.SettingsBox.UnhookEvents();

        internal static void Destroy()
        {
            if (Instance == null) return;
            Instance.UninstallKeyboardHook();
            Instance.SettingsBox = null;
            Instance = null;
        }

        static public void RefreshSettingsUI()
        {
            if (Instance == null)
                return;

            var settings = ConfigManager.Settings;
            var settingsUI = Instance.SettingsBox;

            // HunterPie
            settingsUI.switchEnableAutoUpdate.IsEnabled = settings.HunterPie.Update.Enabled;
            settingsUI.switchEnableNative.IsEnabled = settings.HunterPie.EnableNativeFunctions;
            settingsUI.branchesCombobox.SelectedItem = Instance.SettingsBox.branchesCombobox.Items.Contains(settings.HunterPie.Update.Branch) ? settings.HunterPie.Update.Branch : "master";
            settingsUI.ThemeFilesCombobox.SelectedItem = settings.HunterPie.Theme;
            settingsUI.GamePathFileSelect.SelectedPath = settings.HunterPie.Launch.GamePath;
            settingsUI.gameLaunchArgsTb.Text = settings.HunterPie.Launch.LaunchArgs;
            settingsUI.switchEnableCloseWhenExit.IsEnabled = settings.HunterPie.Options.CloseWhenGameCloses;
            settingsUI.LanguageFilesCombobox.SelectedItem = settings.HunterPie.Language;
            settingsUI.switchEnableMinimizeToSystemTray.IsEnabled = settings.HunterPie.MinimizeToSystemTray;
            settingsUI.switchEnableStartMinimized.IsEnabled = settings.HunterPie.StartHunterPieMinimized;
            settingsUI.comboPluginUpdateProxy.SelectedIndex = (int)settings.HunterPie.PluginProxyMode;
            settingsUI.switchEnableAutoMinimize.IsEnabled = settings.HunterPie.MinimizeAfterGameStart;

            // Debug
            settingsUI.switchEnableDebugMessages.IsEnabled = settings.HunterPie.Debug.ShowDebugMessages;
            settingsUI.switchEnableUnknownStatuses.IsEnabled = settings.HunterPie.Debug.ShowUnknownStatuses;
            settingsUI.switchEnableLoadMonsterData.IsEnabled = settings.HunterPie.Debug.LoadCustomMonsterData;
            settingsUI.switchEnableSendLogsToDev.IsEnabled = settings.HunterPie.Debug.SendCrashFileToDev;

            // Rich Presence
            settingsUI.switchEnableRichPresence.IsEnabled = settings.RichPresence.Enabled;
            settingsUI.switchShowMonsterHealth.IsEnabled = settings.RichPresence.ShowMonsterHealth;
            settingsUI.switchLetPeopleJoinSession.IsEnabled = settings.RichPresence.LetPeopleJoinSession;

            // Overlay
            settingsUI.switchInitializeOverlay.IsEnabled = settings.Overlay.Initialize;
            settingsUI.switchEnableOverlay.IsEnabled = settings.Overlay.Enabled;
            settingsUI.DesiredFrameRateSlider.Value = settings.Overlay.DesiredAnimationFrameRate;
            settingsUI.DesiredScanPerSecond.Value = settings.Overlay.GameScanDelay;
            settingsUI.switchAliasing.IsEnabled = settings.Overlay.EnableAntiAliasing;
            settingsUI.switchForceDirectX11Fullscreen.IsEnabled = settings.Overlay.EnableForceDirectX11Fullscreen;
            settingsUI.ToggleDesignHotkey.HotKey = settings.Overlay.ToggleDesignKeybind;
            settingsUI.ToggleOverlayHotkey.HotKey = settings.Overlay.ToggleOverlayKeybind;
            settingsUI.switchHardwareAcceleration.IsEnabled = settings.Overlay.EnableHardwareAcceleration;
            settingsUI.switchHideWhenUnfocused.IsEnabled = settings.Overlay.HideWhenGameIsUnfocused;
            settingsUI.OverlayPosition.X = settings.Overlay.Position[0];
            settingsUI.OverlayPosition.Y = settings.Overlay.Position[1];

            // Player
            settingsUI.switchInitializePlayerWidget.IsEnabled = settings.Overlay.PlayerHealthComponent.Initialize;
            settingsUI.switchEnablePlayerWidget.IsEnabled = settings.Overlay.PlayerHealthComponent.Enabled;
            settingsUI.switchEnabledPlayerMinimalisticMode.IsEnabled = settings.Overlay.PlayerHealthComponent.MinimalisticMode;
            settingsUI.switchEnablePlayerStreamerMode.IsEnabled = settings.Overlay.PlayerHealthComponent.StreamerMode;
            settingsUI.PlayerNameTextFormat.Text = settings.Overlay.PlayerHealthComponent.NameTextFormat;
            settingsUI.switchEnableHideInVillages.IsEnabled = settings.Overlay.PlayerHealthComponent.HideHealthInVillages;
            settingsUI.PlayerComponentOpacity.Value = settings.Overlay.PlayerHealthComponent.Opacity;
            settingsUI.PlayerWidgetPosition.X = settings.Overlay.PlayerHealthComponent.Position[0];
            settingsUI.PlayerWidgetPosition.Y = settings.Overlay.PlayerHealthComponent.Position[1];

            // Monsters
            settingsUI.switchInitializeMonsterWidget.IsEnabled = settings.Overlay.MonstersComponent.Initialize;
            settingsUI.switchEnableMonsterComponent.IsEnabled = settings.Overlay.MonstersComponent.Enabled;
            settingsUI.switchEnableMonsterStreamerMode.IsEnabled = settings.Overlay.MonstersComponent.StreamerMode;
            settingsUI.switchEnableMonsterAction.IsEnabled = settings.Overlay.MonstersComponent.ShowMonsterActionName;
            settingsUI.switchSortParts.IsEnabled = settings.Overlay.MonstersComponent.EnableSortParts;
            settingsUI.switchEnableHideUnactiveAilments.IsEnabled = settings.Overlay.MonstersComponent.HideAilmentsAfterSeconds;
            settingsUI.switchShowOnlyPartsThatCanBeBroken.IsEnabled = settings.Overlay.MonstersComponent.EnableOnlyPartsThatCanBeBroken;
            settingsUI.switchShowOnlyPartsThatArentBroken.IsEnabled = settings.Overlay.MonstersComponent.HidePartsThatHaveAlreadyBeenBroken;
            settingsUI.HealthTextFormat.Text = settings.Overlay.MonstersComponent.HealthTextFormat;
            settingsUI.PartHealthTextFormat.Text = settings.Overlay.MonstersComponent.PartTextFormat;
            settingsUI.switchEnableHealthHiding.IsEnabled = settings.Overlay.MonstersComponent.HideHealthInformation;
            settingsUI.AilmentBuildUpTextFormat.Text = settings.Overlay.MonstersComponent.AilmentBuildupTextFormat;
            settingsUI.AilmentTimerTextFormat.Text = settings.Overlay.MonstersComponent.AilmentTimerTextFormat;
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
            settingsUI.AilmentsCustomizer.IsEnabled = settingsUI.switchEnableAilments.IsEnabled;
            foreach (Custom_Controls.Switcher switcher in settingsUI.AilmentsCustomizer.Children)
            {
                if (settings.Overlay.MonstersComponent.EnabledAilmentGroups.Contains(switcher.Name.Replace("EnableAilment_", "")))
                {
                    switcher.IsEnabled = true;
                }
                else
                {
                    switcher.IsEnabled = false;
                }
            }

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
            settingsUI.switchEnableAilmentsColor.IsEnabled = settings.Overlay.MonstersComponent.EnableAilmentsBarColor;

            // Primary Mantle
            settingsUI.switchInitializePrimaryMantleWidget.IsEnabled = settings.Overlay.PrimaryMantle.Initialize;
            settingsUI.switchEnablePrimaryMantle.IsEnabled = settings.Overlay.PrimaryMantle.Enabled;
            settingsUI.switchEnablePMantleStreamerMode.IsEnabled = settings.Overlay.PrimaryMantle.StreamerMode;
            settingsUI.switchPrimaryMantleCompactMode.IsEnabled = settings.Overlay.PrimaryMantle.CompactMode;
            settingsUI.PrimaryMantlePosition.X = settings.Overlay.PrimaryMantle.Position[0];
            settingsUI.PrimaryMantlePosition.Y = settings.Overlay.PrimaryMantle.Position[1];
            settingsUI.PrimaryMantleColor.Color = settings.Overlay.PrimaryMantle.Color;
            settingsUI.PrimaryMantleComponentOpacity.Value = settings.Overlay.PrimaryMantle.Opacity;

            // Secondary Mantle
            settingsUI.switchInitializeSecondaryMantleWidget.IsEnabled = settings.Overlay.SecondaryMantle.Initialize;
            settingsUI.switchEnableSecondaryMantle.IsEnabled = settings.Overlay.SecondaryMantle.Enabled;
            settingsUI.switchEnableSMantleStreamerMode.IsEnabled = settings.Overlay.SecondaryMantle.StreamerMode;
            settingsUI.switchSecondaryMantleCompactMode.IsEnabled = settings.Overlay.SecondaryMantle.CompactMode;
            settingsUI.SecondaryMantlePosition.X = settings.Overlay.SecondaryMantle.Position[0];
            settingsUI.SecondaryMantlePosition.Y = settings.Overlay.SecondaryMantle.Position[1];
            settingsUI.SecondaryMantleColor.Color = settings.Overlay.SecondaryMantle.Color;
            settingsUI.SecondaryMantleComponentOpacity.Value = settings.Overlay.SecondaryMantle.Opacity;

            // Harvest Box
            settingsUI.switchInitializeHarvestWidget.IsEnabled = settings.Overlay.HarvestBoxComponent.Initialize;
            settingsUI.switchEnableHarvestBox.IsEnabled = settings.Overlay.HarvestBoxComponent.Enabled;
            settingsUI.switchEnableHarvestStreamerMode.IsEnabled = settings.Overlay.HarvestBoxComponent.StreamerMode;
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
            settingsUI.switchInitializeMeterWidget.IsEnabled = settings.Overlay.DPSMeter.Initialize;
            settingsUI.switchEnableDPSMeter.IsEnabled = settings.Overlay.DPSMeter.Enabled;
            settingsUI.switchEnableDPSMeterStreamerMode.IsEnabled = settings.Overlay.DPSMeter.StreamerMode;
            settingsUI.switchEnableTotalDamage.IsEnabled = settings.Overlay.DPSMeter.ShowTotalDamage;
            settingsUI.switchEnableDPSWheneverPossible.IsEnabled = settings.Overlay.DPSMeter.ShowDPSWheneverPossible;
            settingsUI.switchEnableTimer.IsEnabled = settings.Overlay.DPSMeter.ShowTimer;
            settingsUI.switchEnableTimerInExpeditions.IsEnabled = settings.Overlay.DPSMeter.ShowTimerInExpeditions;
            settingsUI.switchEnableOnlyTimer.IsEnabled = settings.Overlay.DPSMeter.ShowOnlyTimer;
            settingsUI.switchEnableOnlyMyself.IsEnabled = settings.Overlay.DPSMeter.ShowOnlyMyself;
            settingsUI.switchEnableDamagePlot.IsEnabled = settings.Overlay.DPSMeter.EnableDamagePlot;
            settingsUI.comboDamagePlotMode.SelectedIndex = (int)settings.Overlay.DPSMeter.DamagePlotMode;
            settingsUI.DamagePlotPollInterval.Value = settings.Overlay.DPSMeter.DamagePlotPollInterval;
            settingsUI.DamageMeterPosition.X = settings.Overlay.DPSMeter.Position[0];
            settingsUI.DamageMeterPosition.Y = settings.Overlay.DPSMeter.Position[1];
            settingsUI.FirstPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[0].Color;
            settingsUI.SecondPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[1].Color;
            settingsUI.ThirdPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[2].Color;
            settingsUI.FourthPlayerColor.Color = settings.Overlay.DPSMeter.PartyMembers[3].Color;
            settingsUI.DamageBackgroundOpacity.Value = settings.Overlay.DPSMeter.BackgroundOpacity;
            settingsUI.DamageComponentOpacity.Value = settings.Overlay.DPSMeter.Opacity;

            // Classes Widget
            settingsUI.switchInitializeClassHelper.IsEnabled = settings.Overlay.ClassesWidget.Initialize;
            settingsUI.switchEnableClassStreamerMode.IsEnabled = settings.Overlay.ClassesWidget.StreamerMode;
            settingsUI.switchGreatswordHelper.IsEnabled = settings.Overlay.ClassesWidget.GreatswordHelper.Enabled;
            settingsUI.switchDualBladesHelper.IsEnabled = settings.Overlay.ClassesWidget.DualBladesHelper.Enabled;
            settingsUI.switchLongswordHelper.IsEnabled = settings.Overlay.ClassesWidget.LongSwordHelper.Enabled;
            settingsUI.switchHammerHelper.IsEnabled = settings.Overlay.ClassesWidget.HammerHelper.Enabled;
            settingsUI.switchHuntingHornHelper.IsEnabled = settings.Overlay.ClassesWidget.HuntingHornHelper.Enabled;
            settingsUI.switchLanceHelper.IsEnabled = settings.Overlay.ClassesWidget.LanceHelper.Enabled;
            settingsUI.switchGunLanceHelper.IsEnabled = settings.Overlay.ClassesWidget.GunLanceHelper.Enabled;
            settingsUI.switchSwitchAxeHelper.IsEnabled = settings.Overlay.ClassesWidget.SwitchAxeHelper.Enabled;
            settingsUI.switchChargeBladeHelper.IsEnabled = settings.Overlay.ClassesWidget.ChargeBladeHelper.Enabled;
            settingsUI.switchInsectGlaiveHelper.IsEnabled = settings.Overlay.ClassesWidget.InsectGlaiveHelper.Enabled;
            settingsUI.switchBowHelper.IsEnabled = settings.Overlay.ClassesWidget.BowHelper.Enabled;
            settingsUI.switchHBGHelper.IsEnabled = settings.Overlay.ClassesWidget.HeavyBowgunHelper.Enabled;
            settingsUI.switchLBGHelper.IsEnabled = settings.Overlay.ClassesWidget.LightBowgunHelper.Enabled;


        }

        private async void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            var settings = ConfigManager.Settings;
            var settingsUI = Instance.SettingsBox;
            // HunterPie
            settings.HunterPie.Update.Enabled = settingsUI.switchEnableAutoUpdate.IsEnabled;
            settings.HunterPie.EnableNativeFunctions = settingsUI.switchEnableNative.IsEnabled;
            settings.HunterPie.Update.Branch = (string)settingsUI.branchesCombobox.SelectedItem;
            settings.HunterPie.Theme = (string)settingsUI.ThemeFilesCombobox.SelectedItem;
            settings.HunterPie.Launch.GamePath = settingsUI.GamePathFileSelect.SelectedPath;
            settings.HunterPie.Launch.LaunchArgs = settingsUI.gameLaunchArgsTb.Text;
            settings.HunterPie.Options.CloseWhenGameCloses = settingsUI.switchEnableCloseWhenExit.IsEnabled;
            settings.HunterPie.Language = (string)settingsUI.LanguageFilesCombobox.SelectedItem;
            settings.HunterPie.MinimizeToSystemTray = settingsUI.switchEnableMinimizeToSystemTray.IsEnabled;
            settings.HunterPie.StartHunterPieMinimized = settingsUI.switchEnableStartMinimized.IsEnabled;
            settings.HunterPie.PluginProxyMode = (PluginProxyMode)settingsUI.comboPluginUpdateProxy.SelectedIndex;
            settings.HunterPie.MinimizeAfterGameStart = settingsUI.switchEnableAutoMinimize.IsEnabled;

            // Debug
            settings.HunterPie.Debug.ShowDebugMessages = settingsUI.switchEnableDebugMessages.IsEnabled;
            settings.HunterPie.Debug.ShowUnknownStatuses = settingsUI.switchEnableUnknownStatuses.IsEnabled;
            settings.HunterPie.Debug.LoadCustomMonsterData = settingsUI.switchEnableLoadMonsterData.IsEnabled;
            settings.HunterPie.Debug.SendCrashFileToDev = settingsUI.switchEnableSendLogsToDev.IsEnabled;

            // Rich Presence
            settings.RichPresence.Enabled = settingsUI.switchEnableRichPresence.IsEnabled;
            settings.RichPresence.ShowMonsterHealth = settingsUI.switchShowMonsterHealth.IsEnabled;
            settings.RichPresence.LetPeopleJoinSession = settingsUI.switchLetPeopleJoinSession.IsEnabled;

            // Overlay
            settings.Overlay.Initialize = settingsUI.switchInitializeOverlay.IsEnabled;
            settings.Overlay.Enabled = settingsUI.switchEnableOverlay.IsEnabled;
            settings.Overlay.DesiredAnimationFrameRate = (int)settingsUI.DesiredFrameRateSlider.Value;
            settings.Overlay.GameScanDelay = (int)settingsUI.DesiredScanPerSecond.Value;
            settings.Overlay.EnableAntiAliasing = settingsUI.switchAliasing.IsEnabled;
            settings.Overlay.EnableForceDirectX11Fullscreen = settingsUI.switchForceDirectX11Fullscreen.IsEnabled;
            settings.Overlay.ToggleDesignKeybind = settingsUI.ToggleDesignHotkey.HotKey;
            settings.Overlay.ToggleOverlayKeybind = settingsUI.ToggleOverlayHotkey.HotKey;
            settings.Overlay.EnableHardwareAcceleration = settingsUI.switchHardwareAcceleration.IsEnabled;
            settings.Overlay.HideWhenGameIsUnfocused = settingsUI.switchHideWhenUnfocused.IsEnabled;
            settings.Overlay.Position[0] = settingsUI.OverlayPosition.X;
            settings.Overlay.Position[1] = settingsUI.OverlayPosition.Y;

            // Player
            settings.Overlay.PlayerHealthComponent.Initialize = settingsUI.switchInitializePlayerWidget.IsEnabled;
            settings.Overlay.PlayerHealthComponent.Enabled = settingsUI.switchEnablePlayerWidget.IsEnabled;
            settings.Overlay.PlayerHealthComponent.MinimalisticMode = settingsUI.switchEnabledPlayerMinimalisticMode.IsEnabled;
            settings.Overlay.PlayerHealthComponent.StreamerMode = settingsUI.switchEnablePlayerStreamerMode.IsEnabled;
            settings.Overlay.PlayerHealthComponent.NameTextFormat = settingsUI.PlayerNameTextFormat.Text;
            settings.Overlay.PlayerHealthComponent.HideHealthInVillages = settingsUI.switchEnableHideInVillages.IsEnabled;
            settings.Overlay.PlayerHealthComponent.Opacity = (float)settingsUI.PlayerComponentOpacity.Value;
            settings.Overlay.PlayerHealthComponent.Position[0] = settingsUI.PlayerWidgetPosition.X;
            settings.Overlay.PlayerHealthComponent.Position[1] = settingsUI.PlayerWidgetPosition.Y;

            // Monsters
            settings.Overlay.MonstersComponent.Initialize = settingsUI.switchInitializeMonsterWidget.IsEnabled;
            settings.Overlay.MonstersComponent.Enabled = settingsUI.switchEnableMonsterComponent.IsEnabled;
            settings.Overlay.MonstersComponent.StreamerMode = settingsUI.switchEnableMonsterStreamerMode.IsEnabled;
            settings.Overlay.MonstersComponent.ShowMonsterActionName = settingsUI.switchEnableMonsterAction.IsEnabled;
            settings.Overlay.MonstersComponent.EnableSortParts = settingsUI.switchSortParts.IsEnabled;
            settings.Overlay.MonstersComponent.HideAilmentsAfterSeconds = settingsUI.switchEnableHideUnactiveAilments.IsEnabled;
            settings.Overlay.MonstersComponent.EnableOnlyPartsThatCanBeBroken = settingsUI.switchShowOnlyPartsThatCanBeBroken.IsEnabled;
            settings.Overlay.MonstersComponent.HidePartsThatHaveAlreadyBeenBroken = settingsUI.switchShowOnlyPartsThatArentBroken.IsEnabled;
            settings.Overlay.MonstersComponent.HealthTextFormat = settingsUI.HealthTextFormat.Text;
            settings.Overlay.MonstersComponent.PartTextFormat = settingsUI.PartHealthTextFormat.Text;
            settings.Overlay.MonstersComponent.HideHealthInformation = settingsUI.switchEnableHealthHiding.IsEnabled;
            settings.Overlay.MonstersComponent.AilmentBuildupTextFormat = settingsUI.AilmentBuildUpTextFormat.Text;
            settings.Overlay.MonstersComponent.AilmentTimerTextFormat = settingsUI.AilmentTimerTextFormat.Text;
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
            List<string> EnabledAilments = new List<string>();
            foreach (Custom_Controls.Switcher switcher in settingsUI.AilmentsCustomizer.Children)
            {
                if (switcher.IsEnabled)
                    EnabledAilments.Add(switcher.Name.Replace("EnableAilment_", ""));
            }
            settings.Overlay.MonstersComponent.EnableMonsterAilments = settingsUI.switchEnableAilments.IsEnabled;
            settings.Overlay.MonstersComponent.EnabledAilmentGroups = EnabledAilments.ToArray();
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
            settings.Overlay.MonstersComponent.EnableAilmentsBarColor = settingsUI.switchEnableAilmentsColor.IsEnabled;

            // Primary Mantle
            settings.Overlay.PrimaryMantle.Initialize = settingsUI.switchInitializePrimaryMantleWidget.IsEnabled;
            settings.Overlay.PrimaryMantle.Enabled = settingsUI.switchEnablePrimaryMantle.IsEnabled;
            settings.Overlay.PrimaryMantle.StreamerMode = settingsUI.switchEnablePMantleStreamerMode.IsEnabled;
            settings.Overlay.PrimaryMantle.CompactMode = settingsUI.switchPrimaryMantleCompactMode.IsEnabled;
            settings.Overlay.PrimaryMantle.Position[0] = settingsUI.PrimaryMantlePosition.X;
            settings.Overlay.PrimaryMantle.Position[1] = settingsUI.PrimaryMantlePosition.Y;
            settings.Overlay.PrimaryMantle.Color = settingsUI.PrimaryMantleColor.Color;
            settings.Overlay.PrimaryMantle.Opacity = (float)settingsUI.PrimaryMantleComponentOpacity.Value;

            // Secondary Mantle
            settings.Overlay.SecondaryMantle.Initialize = settingsUI.switchInitializeSecondaryMantleWidget.IsEnabled;
            settings.Overlay.SecondaryMantle.Enabled = settingsUI.switchEnableSecondaryMantle.IsEnabled;
            settings.Overlay.SecondaryMantle.StreamerMode = settingsUI.switchEnableSMantleStreamerMode.IsEnabled;
            settings.Overlay.SecondaryMantle.CompactMode = settingsUI.switchSecondaryMantleCompactMode.IsEnabled;
            settings.Overlay.SecondaryMantle.Position[0] = settingsUI.SecondaryMantlePosition.X;
            settings.Overlay.SecondaryMantle.Position[1] = settingsUI.SecondaryMantlePosition.Y;
            settings.Overlay.SecondaryMantle.Color = settingsUI.SecondaryMantleColor.Color;
            settings.Overlay.SecondaryMantle.Opacity = (float)settingsUI.SecondaryMantleComponentOpacity.Value;

            // Harvest Box
            settings.Overlay.HarvestBoxComponent.Initialize = settingsUI.switchInitializeHarvestWidget.IsEnabled;
            settings.Overlay.HarvestBoxComponent.Enabled = settingsUI.switchEnableHarvestBox.IsEnabled;
            settings.Overlay.HarvestBoxComponent.StreamerMode = settingsUI.switchEnableHarvestStreamerMode.IsEnabled;
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
            settings.Overlay.DPSMeter.Initialize = settingsUI.switchInitializeMeterWidget.IsEnabled;
            settings.Overlay.DPSMeter.Enabled = settingsUI.switchEnableDPSMeter.IsEnabled;
            settings.Overlay.DPSMeter.StreamerMode = settingsUI.switchEnableDPSMeterStreamerMode.IsEnabled;
            settings.Overlay.DPSMeter.ShowTotalDamage = settingsUI.switchEnableTotalDamage.IsEnabled;
            settings.Overlay.DPSMeter.ShowDPSWheneverPossible = settingsUI.switchEnableDPSWheneverPossible.IsEnabled;
            settings.Overlay.DPSMeter.ShowTimer = settingsUI.switchEnableTimer.IsEnabled;
            settings.Overlay.DPSMeter.ShowTimerInExpeditions = settingsUI.switchEnableTimerInExpeditions.IsEnabled;
            settings.Overlay.DPSMeter.EnableDamagePlot = settingsUI.switchEnableDamagePlot.IsEnabled;
            settings.Overlay.DPSMeter.DamagePlotMode = (DamagePlotMode)settingsUI.comboDamagePlotMode.SelectedIndex;
            settings.Overlay.DPSMeter.DamagePlotPollInterval = (int)settingsUI.DamagePlotPollInterval.Value;
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
            settings.Overlay.ClassesWidget.Initialize = settingsUI.switchInitializeClassHelper.IsEnabled;
            settings.Overlay.ClassesWidget.Enabled = settingsUI.switchEnableClassStreamerMode.IsEnabled;
            settings.Overlay.ClassesWidget.GreatswordHelper.Enabled = settingsUI.switchGreatswordHelper.IsEnabled;
            settings.Overlay.ClassesWidget.DualBladesHelper.Enabled = settingsUI.switchDualBladesHelper.IsEnabled;
            settings.Overlay.ClassesWidget.LongSwordHelper.Enabled = settingsUI.switchLongswordHelper.IsEnabled;
            settings.Overlay.ClassesWidget.HammerHelper.Enabled = settingsUI.switchHammerHelper.IsEnabled;
            settings.Overlay.ClassesWidget.HuntingHornHelper.Enabled = settingsUI.switchHuntingHornHelper.IsEnabled;
            settings.Overlay.ClassesWidget.LanceHelper.Enabled = settingsUI.switchLanceHelper.IsEnabled;
            settings.Overlay.ClassesWidget.GunLanceHelper.Enabled = settingsUI.switchGunLanceHelper.IsEnabled;
            settings.Overlay.ClassesWidget.SwitchAxeHelper.Enabled = settingsUI.switchSwitchAxeHelper.IsEnabled;
            settings.Overlay.ClassesWidget.ChargeBladeHelper.Enabled = settingsUI.switchChargeBladeHelper.IsEnabled;
            settings.Overlay.ClassesWidget.InsectGlaiveHelper.Enabled = settingsUI.switchInsectGlaiveHelper.IsEnabled;
            settings.Overlay.ClassesWidget.BowHelper.Enabled = settingsUI.switchBowHelper.IsEnabled;
            settings.Overlay.ClassesWidget.HeavyBowgunHelper.Enabled = settingsUI.switchHBGHelper.IsEnabled;
            settings.Overlay.ClassesWidget.LightBowgunHelper.Enabled = settingsUI.switchLBGHelper.IsEnabled;

            // Abnormality bars
            int i = 0;
            foreach (Custom_Controls.BuffBarSettingControl abnormBar in settingsUI.BuffTrays.Children)
            {
                settings.Overlay.AbnormalitiesWidget.BarPresets[i].Name = abnormBar.PresetName;
                settings.Overlay.AbnormalitiesWidget.BarPresets[i].Enabled = abnormBar.Enabled;
                i++;
            }

            // save data from plugin tabs
            // TODO: notify user
            var hasErrors = settingsUI.Save();

            // and then save settings
            await ConfigManager.TrySaveSettingsAsync();
        }
    }
}
