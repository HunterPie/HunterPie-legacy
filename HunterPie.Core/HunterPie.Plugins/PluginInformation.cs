using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public PluginUpdateInformation Update { get; set; } = new PluginUpdateInformation();

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
