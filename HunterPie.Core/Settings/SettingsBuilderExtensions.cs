using System.Windows;
using System.Windows.Media;

namespace HunterPie.Settings
{
    /// <summary>
    /// More specialized methods to add setting tabs.
    /// </summary>
    public static class SettingsBuilderExtensions
    {
        /// <summary>
        /// Adds settings tab where control also implements  <see cref="ISettings"/>.
        /// </summary>
        public static ISettingsBuilder AddTab<T>(this ISettingsBuilder factory, T settingsControl, ImageSource img = null, string displayName = null)
            where T : UIElement, ISettings
        {
            return factory.AddTab(new SettingsTab(factory.OwnerName, displayName ?? factory.DisplayName, img, settingsControl, settingsControl));
        }

        /// <summary>
        /// Adds tab. If <param name="displayName"></param> is null, using plugin's display name.
        /// </summary>
        public static ISettingsBuilder AddTab(this ISettingsBuilder factory, UIElement control, ISettings settings, ImageSource img = null, string displayName = null)
        {
            return factory.AddTab(new SettingsTab(factory.OwnerName, displayName ?? factory.DisplayName, img, control, settings));
        }
    }
}
