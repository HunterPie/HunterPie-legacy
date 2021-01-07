using System;

namespace HunterPie.Plugins
{
    public interface IPluginListProxy
    {
        event EventHandler RegistryPluginsLoaded;

        void UpdatePluginsArrangement();
        PluginRegistryEntry TryGetRegistryEntry(string pluginName);
    }
}
