using System.Windows;
using System.Windows.Media;

namespace HunterPie.Settings
{
    /// <summary>
    /// Includes everything that is needed to render and operate custom settings tab.
    /// </summary>
    public interface ISettingsTab
    {
        /// <summary>
        /// Used to identify tab owner (such as plugin name).
        /// </summary>
        string OwnerName { get; }

        string DisplayName { get; }
        ImageSource Image { get; }

        UIElement Control { get; }
        ISettings Settings { get; }
    }
}
