using System;

namespace HunterPie.Plugins
{
    public class PluginInformation
    {
        public string Name;
        public string EntryPoint;
        public string Description;
        public string Author;
        public string Version;
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public PluginSettings InternalSettings { get; set; } = new PluginSettings();
    }

    public class PluginUpdateInformation
    {
        public string UpdateUrl;
        public string MinimumVersion;
    }
}
