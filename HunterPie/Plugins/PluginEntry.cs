namespace HunterPie.Plugins
{
    public class PluginEntry
    {
        public PluginInformation PluginInformation { get; set; }
        public PluginPackage? Package { get; set; }

        public string RootPath { get; set; }
        public bool IsLoaded => Package != null;
        public bool IsFailed { get; set; }
    }
}
