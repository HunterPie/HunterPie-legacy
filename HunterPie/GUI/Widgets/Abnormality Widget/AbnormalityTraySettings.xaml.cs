using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using HunterPie.Core;
using HunterPie.Memory;

namespace HunterPie.GUI.Widgets.Abnormality_Widget {
    /// <summary>
    /// Interaction logic for AbnormalityTraySettings.xaml
    /// </summary>
    public partial class AbnormalityTraySettings : WidgetSettings {

        List<Parts.AbnormalitySettingControl> AbnormalitiesList = new List<Parts.AbnormalitySettingControl>();
        Widget widgetParent;
        int BuffTrayIndex;

        public AbnormalityTraySettings(Widget parent = null, int TrayIndex = 0) {
            InitializeComponent();
            BuffTrayIndex = TrayIndex;
            widgetParent = parent;
            this.WindowTitle.Text = $"Settings: {UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Name}";
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
            EnableName.IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].ShowNames;
            OrientationSwitcher.IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Orientation == "Horizontal";
            EnableTimeLeftSwitcher.IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].ShowTimeLeftText;
            TimerTextFormatBox.SelectedIndex = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].TimeLeftTextFormat;
            BackgroundOpacitySlider.Value = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].BackgroundOpacity;
            
        }

        private void PopulateHuntingHornBuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetHuntingHornAbnormalities()) {
                string Type = "HUNTINGHORN";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                string InternalID = $"HH_{ID}";
                bool IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities.Contains(InternalID);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, InternalID, IsEnabled);
                AbnormalitiesList.Add(AbnormDisplay);
                HuntingHornBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateOrchestraBuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetPalicoAbnormalities()) {
                string Type = "PALICO";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                string InternalID = $"PAL_{ID}";
                bool IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities.Contains(InternalID);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, InternalID, IsEnabled);
                AbnormalitiesList.Add(AbnormDisplay);
                PalicoBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateDebuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetBlightAbnormalities()) {
                string Type = "DEBUFF";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                string InternalID = $"DE_{ID}";
                bool IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities.Contains(InternalID);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, InternalID, IsEnabled);
                AbnormalitiesList.Add(AbnormDisplay);
                Debuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateConsumableBuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetMiscAbnormalities()) {
                string Type = "MISC";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                string InternalID = $"MISC_{ID}";
                bool IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities.Contains(InternalID);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, InternalID, IsEnabled);
                AbnormalitiesList.Add(AbnormDisplay);
                ConsumableBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateGearBuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetGearAbnormalities()) {
                string Type = "GEAR";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                string InternalID = $"GEAR_{ID}";
                bool IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities.Contains(InternalID);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, InternalID, IsEnabled);
                AbnormalitiesList.Add(AbnormDisplay);
                GearBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void OnCloseButtonClick(object sender, MouseButtonEventArgs e) {
            this.Close();
        }

        private void OnDragWindow(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e) {
            List<string> EnabledAbnormalities = new List<string>();
            foreach (Parts.AbnormalitySettingControl AbnormDisplay in AbnormalitiesList) {
                if (AbnormDisplay.IsEnabled) EnabledAbnormalities.Add(AbnormDisplay.InternalID);
            }
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].ShowNames = EnableName.IsEnabled;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities = EnabledAbnormalities.ToArray();
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Orientation = OrientationSwitcher.IsEnabled ? "Horizontal" : "Vertical";
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].ShowTimeLeftText = EnableTimeLeftSwitcher.IsEnabled;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].TimeLeftTextFormat = (byte)TimerTextFormatBox.SelectedIndex;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].BackgroundOpacity = (float)BackgroundOpacitySlider.Value;
            UserSettings.SaveNewConfig();
        }

        private void OnSelectAllButtonClick(object sender, RoutedEventArgs e) {
            ToggleAllAbnormalitiesInTab(true);
        }

        private void OnUnselectAllButtonClick(object sender, RoutedEventArgs e) {
            ToggleAllAbnormalitiesInTab(false);
        }

        private void ToggleAllAbnormalitiesInTab(bool enable) {
            ScrollViewer SelectedAbnormalityContainer = AbnormalitySelectionContainer.SelectedContent as ScrollViewer;
            WrapPanel SelectedAbnormalityPanel = SelectedAbnormalityContainer.Content as WrapPanel;
            foreach (UIElement RawAbnormality in SelectedAbnormalityPanel.Children) {
                Parts.AbnormalitySettingControl AbnormalityDisplay = (Parts.AbnormalitySettingControl)RawAbnormality;
                AbnormalityDisplay.IsEnabled = enable;
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.widgetParent = null;
            HuntingHornBuffs.Children.Clear();
            PalicoBuffs.Children.Clear();
            Debuffs.Children.Clear();
            ConsumableBuffs.Children.Clear();
            this.AbnormalitiesList.Clear();
        }

    }
}
