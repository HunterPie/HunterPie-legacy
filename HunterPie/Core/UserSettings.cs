using Newtonsoft.Json;
using System;
using System.IO;
using HunterPie.Logger;

namespace HunterPie.Core {
    public class UserSettings {

        // Config file watcher
        private static FileSystemWatcher ConfigWatcher;

        static private string ConfigFileName = @"config.json";
        public static Config.Rootobject PlayerConfig;
        private static string ConfigSerialized;

        // Config events
        public delegate void SettingsEvents(object source, EventArgs args);
        public static event SettingsEvents OnSettingsUpdate;

        protected static void _onSettingsUpdate() {
            OnSettingsUpdate?.Invoke(typeof(UserSettings), EventArgs.Empty);
        }

        // Config template
        public class Config {

            public class Rootobject {
                public Overlay Overlay { get; set; } = new Overlay();
                public Richpresence RichPresence { get; set; } = new Richpresence();
                public Hunterpie HunterPie { get; set; } = new Hunterpie();
            }

            public class Overlay {
                public bool Enabled { get; set; } = false;
                public int[] Position { get; set; } = new int[2] { 0, 0 };
                public bool HideWhenGameIsUnfocused { get; set; } = false;
                public Monsterscomponent MonstersComponent { get; set; } = new Monsterscomponent();
                public Harvestboxcomponent HarvestBoxComponent { get; set; } = new Harvestboxcomponent();
                public Primarymantle PrimaryMantle { get; set; } = new Primarymantle();
                public Secondarymantle SecondaryMantle { get; set; } = new Secondarymantle();
                public DPSMeter DPSMeter { get; set; } = new DPSMeter();
            }

            public class Monsterscomponent {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 335, 10 };
                public bool ShowMonsterWeakness { get; set; } = true;
            }

            public class Harvestboxcomponent {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 1110, 30 };
            }

            public class Primarymantle {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 1145, 300 };
                public string Color { get; set; } = "#99C500AA";
            }

            public class Secondarymantle {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 1145, 300 };
                public string Color { get; set; } = "#996900FF";
            }

            public class DPSMeter {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 10, 350 };
                public Players[] PartyMembers { get; set; } = new Players[4] { new Players(), new Players(), new Players(), new Players() };
            }

            public class Players {
                public string Color { get; set; } = "#FFFFFF";
            }

            public class Richpresence {
                public bool Enabled { get; set; } = true;
            }

            public class Hunterpie {
                public string Language { get; set; } = @"Languages\en-us.xml";
                public Update Update { get; set; } = new Update();
                public Launch Launch { get; set; } = new Launch();
                public Options Options { get; set; } = new Options();
            }

            public class Update {
                public bool Enabled { get; set; } = true;
                public string Branch { get; set; } = "master";
            }

            public class Launch {
                public string GamePath { get; set; } = "";
                public string LaunchArgs { get; set; } = "";
            }

            public class Options {
                public bool CloseWhenGameCloses { get; set; } = false;
            }
        }

        public static void TriggerSettingsEvent() {
            _onSettingsUpdate();
        }

        public static void InitializePlayerConfig() {
            // This is called only once when HunterPie starts
            LoadPlayerConfig();
            SaveNewConfig();
            CreateFileWatcher();
        }

        private static void CreateFileWatcher() {
            // Prevents it from hooking the event multiple times
            if (ConfigWatcher != null) return;
            ConfigWatcher = new FileSystemWatcher {
                Path = Environment.CurrentDirectory,
                Filter = ConfigFileName
            };
            ConfigWatcher.Changed += OnConfigChanged;
            ConfigWatcher.EnableRaisingEvents = true;
        }

        private static void OnConfigChanged(object source, FileSystemEventArgs e) {
            // Use try/catch because FileSystemWatcher sends the same event twice
            // and one of them is when the file is still open 
            try {
                using (var fw = File.OpenWrite(e.FullPath)) {
                    fw.Close();
                }
                LoadPlayerConfig();
            } catch {}
        }

        public static string GetSerializedDefaultConfig() {
            return JsonConvert.SerializeObject(new Config.Rootobject(), Formatting.Indented);
        }

        public static void MakeNewConfig() {
            string d_Config = GetSerializedDefaultConfig();
            File.WriteAllText(ConfigFileName, d_Config);
        }

        private static string LoadPlayerSerializedConfig() {
            string configContent;
            try {
                configContent = File.ReadAllText(ConfigFileName);
            } catch {
                Debugger.Error("Config.json not found!");
                Debugger.Warn("Generating new config");
                MakeNewConfig();
                configContent = File.ReadAllText(ConfigFileName);
            }
            if (ConfigSerialized != configContent) {
                ConfigSerialized = configContent;
                return ConfigSerialized;
            }
            return null;
        }

        public static void LoadPlayerConfig() {
            LoadPlayerSerializedConfig();
            try {
                if (ConfigSerialized == null) return;
                PlayerConfig = JsonConvert.DeserializeObject<Config.Rootobject>(ConfigSerialized);
                _onSettingsUpdate();
                Debugger.Warn("Loaded user config!");
            } catch(Exception err) {
                Debugger.Error($"Failed to parse config.json!\n{err.Message}");
            }
        }

        public static void SaveNewConfig() {
            string newPlayerConfig = JsonConvert.SerializeObject(PlayerConfig, Formatting.Indented);
            File.WriteAllText(ConfigFileName, newPlayerConfig);
        }


    }
}
