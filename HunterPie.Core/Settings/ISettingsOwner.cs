using System.Collections.Generic;

namespace HunterPie.Settings
{
    /// <summary>
    /// Represents entity (such as plugin) that can add its own setting tabs.
    /// </summary>
    public interface ISettingsOwner
    {
        public IEnumerable<ISettingsTab> GetSettings(ISettingsBuilder build);
    }
}
