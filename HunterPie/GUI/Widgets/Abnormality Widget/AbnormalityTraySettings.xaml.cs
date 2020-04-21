using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using HunterPie.Core;
using HunterPie.Core.LPlayer;
using HunterPie.GUI.Widgets.Abnormality_Widget.Parts;

namespace HunterPie.GUI.Widgets.Abnormality_Widget {
    /// <summary>
    /// Interaction logic for AbnormalityTraySettings.xaml
    /// </summary>
    public partial class AbnormalityTraySettings : WidgetSettings {
        readonly List<Parts.AbnormalitySettingControl> abnormalityControls = new List<Parts.AbnormalitySettingControl>();
        Widget widgetParent;
        int buffTrayIndex;
        private UserSettings.Config.AbnormalityBar bar;

        public AbnormalityTraySettings(Widget parent = null, int trayIndex = 0) {
            InitializeComponent();
            buffTrayIndex = trayIndex;
            widgetParent = parent;
            bar = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[buffTrayIndex];
            WindowTitle.Text = $"Settings: {bar.Name}";

            PopulateAbnormalities();
            ConfigureWindow();
        }

        private void PopulateAbnormalities() {
            PopulateHuntingHornBuffs();
            PopulateOrchestraBuffs();
            PopulateDebuffs();
            PopulateConsumableBuffs();
            PopulateGearBuffs();
        }

        private void ConfigureWindow() {
            EnableName.IsEnabled = bar.ShowNames;
            OrientationSwitcher.IsEnabled = bar.Orientation == "Horizontal";
            EnableTimeLeftSwitcher.IsEnabled = bar.ShowTimeLeftText;
            TimerTextFormatBox.SelectedIndex = bar.TimeLeftTextFormat;
            BackgroundOpacitySlider.Value = bar.BackgroundOpacity;
        }

        private void PopulateHuntingHornBuffs() =>
            PopulateAbnormalities(AbnormalityData.HuntingHornAbnormalities, HuntingHornBuffs);

        private void PopulateOrchestraBuffs() =>
            PopulateAbnormalities(AbnormalityData.PalicoAbnormalities, PalicoBuffs);

        private void PopulateDebuffs() => PopulateAbnormalities(AbnormalityData.BlightAbnormalities, Debuffs);

        private void PopulateConsumableBuffs() =>
            PopulateAbnormalities(AbnormalityData.MiscAbnormalities, ConsumableBuffs);

        private void PopulateGearBuffs() => PopulateAbnormalities(AbnormalityData.GearAbnormalities, GearBuffs);

        private void PopulateAbnormalities(IEnumerable<AbnormalityInfo> abnormalities, Panel panel)
        {
            foreach (AbnormalityInfo abnormality in abnormalities)
            {
                string name = GStrings.GetAbnormalityByID(abnormality.Type, abnormality.Id, 0);
                bool isEnabled = bar.AcceptedAbnormalities.Contains(abnormality.InternalId);
                ImageSource icon = (ImageSource)FindResource(abnormality.IconName);
                icon?.Freeze();

                AbnormalitySettingControl settingsControl = new AbnormalitySettingControl();
                settingsControl.SetAbnormalityInfo(icon, name, abnormality.InternalId, isEnabled);
                abnormalityControls.Add(settingsControl);
                panel.Children.Add(settingsControl);
            }
        }

        private void OnCloseButtonClick(object sender, MouseButtonEventArgs e) => Close();

        private void OnDragWindow(object sender, MouseButtonEventArgs e) => DragMove();

        private void OnSaveButtonClick(object sender, RoutedEventArgs e) {
            string[] enabledAbnormalities = abnormalityControls
                .Where(a => a.IsEnabled)
                .Select(a => a.InternalID)
                .ToArray();

            bar.ShowNames = EnableName.IsEnabled;
            bar.AcceptedAbnormalities = enabledAbnormalities;
            bar.Orientation = OrientationSwitcher.IsEnabled ? "Horizontal" : "Vertical";
            bar.ShowTimeLeftText = EnableTimeLeftSwitcher.IsEnabled;
            bar.TimeLeftTextFormat = (byte)TimerTextFormatBox.SelectedIndex;
            bar.BackgroundOpacity = (float)BackgroundOpacitySlider.Value;
            UserSettings.SaveNewConfig();
        }

        private void OnSelectAllButtonClick(object sender, RoutedEventArgs e) => ToggleAllAbnormalitiesInTab(true);

        private void OnUnselectAllButtonClick(object sender, RoutedEventArgs e) => ToggleAllAbnormalitiesInTab(false);

        private void ToggleAllAbnormalitiesInTab(bool enable) {
            ContentControl selectedAbnormalityContainer = (ContentControl)AbnormalitySelectionContainer.SelectedContent;
            Panel selectedAbnormalityPanel = (Panel)selectedAbnormalityContainer.Content;
            foreach (AbnormalitySettingControl abnormalityDisplay in selectedAbnormalityPanel.Children.Cast<AbnormalitySettingControl>()) {
                abnormalityDisplay.IsEnabled = enable;
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            widgetParent = null;
            HuntingHornBuffs.Children.Clear();
            PalicoBuffs.Children.Clear();
            Debuffs.Children.Clear();
            ConsumableBuffs.Children.Clear();
            abnormalityControls.Clear();
        }

    }
}
