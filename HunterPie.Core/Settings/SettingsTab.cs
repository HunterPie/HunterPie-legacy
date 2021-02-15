using System.Windows;
using System.Windows.Media;

namespace HunterPie.Settings
{
    public class SettingsTab : ISettingsTab
    {
        public string OwnerName { get; set; }
        public string DisplayName { get; set; }
        public ImageSource Image { get; set; }
        public UIElement Control { get; set; }
        public ISettings Settings { get; set; }

        public SettingsTab(string ownerName, string displayName, ImageSource image, UIElement control, ISettings settings)
        {
            OwnerName = ownerName;
            DisplayName = displayName;
            Image = image;
            Control = control;
            Settings = settings;
        }

        public SettingsTab(string ownerName)
        {
            this.OwnerName = ownerName;
        }
    }
}
