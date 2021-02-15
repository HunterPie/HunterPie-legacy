using System.Collections.Generic;
using System.Linq;
using HunterPie.Plugins;

namespace HunterPie.Settings
{
    public class SettingsBuilder : ISettingsBuilder
    {
        private readonly PluginPackage package;
        private readonly List<ISettingsTab> blocks = new();

        public string DisplayName => string.IsNullOrEmpty(package.information.DisplayName)
            ? package.information.Name
            : package.information.DisplayName;

        public string OwnerName => package.information.Name;

        public SettingsBuilder(PluginPackage package)
        {
            this.package = package;
        }

        public ISettingsBuilder AddTab(ISettingsTab tab)
        {
            this.blocks.Add(tab);
            return this;
        }

        public IEnumerable<ISettingsTab> Value() => blocks.ToList();
    }
}
