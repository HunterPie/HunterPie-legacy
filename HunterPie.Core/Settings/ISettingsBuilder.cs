using System.Collections.Generic;

namespace HunterPie.Settings
{
    /// <summary>
    /// Used to simplify settings tab creation.
    /// </summary>
    public interface ISettingsBuilder
    {
        string DisplayName { get; }
        string OwnerName { get; }

        /// <summary>
        /// Adds settings tab.
        /// </summary>
        ISettingsBuilder AddTab(ISettingsTab tab);

        /// <summary>
        /// Returns all added tabs as collection.
        /// </summary>
        IEnumerable<ISettingsTab> Value();
    }
}
