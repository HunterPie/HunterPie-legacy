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
        public AbnormalityTraySettings() {
            InitializeComponent();
            PopulateAbnormalities();
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
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, false);
                HuntingHornBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateOrchestraBuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetPalicoAbnormalities()) {
                string Type = "PALICO";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, false);
                PalicoBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateDebuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetBlightAbnormalities()) {
                string Type = "DEBUFF";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, false);
                Debuffs.Children.Add(AbnormDisplay);
            }
        }

        private void PopulateConsumableBuffs() {
            foreach (XmlNode Abnorm in AbnormalityData.GetMiscAbnormalities()) {
                string Type = "MISC";
                int ID = int.Parse(Abnorm.Attributes["ID"].Value);
                string Name = GStrings.GetAbnormalityByID(Type, ID, 0);
                ImageSource Icon = TryFindResource(Abnorm.Attributes["Icon"].Value) as ImageSource ?? FindResource("ICON_MISSING") as ImageSource;
                Icon?.Freeze();
                Parts.AbnormalitySettingControl AbnormDisplay = new Parts.AbnormalitySettingControl();
                AbnormDisplay.SetAbnormalityInfo(Icon, Name, false);
                ConsumableBuffs.Children.Add(AbnormDisplay);
            }
        }

        private void OnCloseButtonClick(object sender, MouseButtonEventArgs e) {
            this.Close();
        }

        private void OnDragWindow(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }
    }
}
