using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Abnormality_Widget {
    /// <summary>
    /// Interaction logic for AbnormalityTraySettings.xaml
    /// </summary>
    public partial class AbnormalityTraySettings : WidgetSettings {

        List<Parts.AbnormalitySettingControl> AbnormalitiesList = new List<Parts.AbnormalitySettingControl>();
        Widget widgetParent;
        int BuffTrayIndex;

        public AbnormalityTraySettings(Widget parent, int TrayIndex) {
            InitializeComponent();
            BuffTrayIndex = TrayIndex;
            widgetParent = parent;
            this.WindowTitle.Text = $"Settings: {UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Name}";
            PopulateAbnormalities();
            OrientationSwitcher.IsEnabled = UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Orientation == "Horizontal";
        }

        private void PopulateAbnormalities() {
            PopulateHuntingHornBuffs();
            PopulateOrchestraBuffs();
            PopulateDebuffs();
            PopulateConsumableBuffs();
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
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].AcceptedAbnormalities = EnabledAbnormalities.ToArray();
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Orientation = OrientationSwitcher.IsEnabled ? "Horizontal" : "Vertical";
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Position[0] = (int)widgetParent.Left;
            UserSettings.PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets[BuffTrayIndex].Position[1] = (int)widgetParent.Top;
            UserSettings.SaveNewConfig();
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
