using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HunterPie.Plugins
{
    public class PluginInformation
    {
        public string Name;
        public string DisplayName { get; set; }
        public string EntryPoint;
        public string Description;
        public string Author;
        public string Version;
        public string[] Dependencies { get; set; } = Array.Empty<string>();
        public List<string> Links { get; set; } = null;

        public PluginUpdateInformation Update { get; set; } = new PluginUpdateInformation();

        public DateTime? ReleaseDate { get; set; }

        internal PluginSettings InternalSettings { get; set; } = new PluginSettings();

        public class PluginUpdateInformation
        {
            public string UpdateUrl { get; set; }
            public string MinimumVersion { get; set; }
            public IDictionary<string, string> FileHashes { get; set; }
        }

        public class FileHash
        {
            public string FileName { get; set; }
            public string Sha256 { get; set; }
        }
    }
}
