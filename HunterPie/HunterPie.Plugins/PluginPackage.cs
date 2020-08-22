namespace HunterPie.Plugins
{
    /// <summary>
    /// A package structure with all the plugin information
    /// </summary>
    public struct PluginPackage
    {
        public IPlugin plugin;
        public PluginInformation information;
        public PluginSettings settings;
        public string path;
    }
}
