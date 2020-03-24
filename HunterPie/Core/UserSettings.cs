using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
                public bool Enabled { get; set; } = true;
                public int DesiredAnimationFrameRate { get; set; } = 30;
                public int[] Position { get; set; } = new int[2] { 0, 0 };
                public string ToggleOverlayKeybind { get; set; } = "Ctrl+Alt+Z";
                public bool EnableHardwareAcceleration { get; set; } = true;
                public bool HideWhenGameIsUnfocused { get; set; } = false;
                public int ToggleDesignModeKey { get; set; } = 145;
                public Monsterscomponent MonstersComponent { get; set; } = new Monsterscomponent();
                public Harvestboxcomponent HarvestBoxComponent { get; set; } = new Harvestboxcomponent();
                public Primarymantle PrimaryMantle { get; set; } = new Primarymantle();
                public Secondarymantle SecondaryMantle { get; set; } = new Secondarymantle();
                public DPSMeter DPSMeter { get; set; } = new DPSMeter();
                public AbnormalitiesWidget AbnormalitiesWidget { get; set; } = new AbnormalitiesWidget();
            }

            public class Monsterscomponent {
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public byte ShowMonsterBarMode { get; set; } = 0;
                public string SwitchMonsterBarModeHotkey = "Tab";
                public int[] Position { get; set; } = new int[2] { 335, 10 };
                public byte MonsterBarDock { get; set; } = 0;
                public int MaxNumberOfPartsAtOnce { get; set; } = 8;
                public int MaxPartColumns { get; set; } = 1;
                public bool ShowMonsterWeakness { get; set; } = true;
                public bool HidePartsAfterSeconds { get; set; } = true;
                public int SecondsToHideParts { get; set; } = 10;
                public bool EnableRemovableParts { get; set; } = true;
                public bool EnableMonsterParts { get; set; } = true;
                public string[] EnabledPartGroups { get; set; } = new string[20] { "HEAD", "BODY", "ARM", "WING", "LEG", "TAIL", "LIMB", "ABDOMEN", "CHEST", "REAR", "JAW", "BACK", "FIN", "HORN", "NECK", "SHELL", "ORGAN", "MISC", "MANE", "BONE" };
            }

            public class Harvestboxcomponent {
                public bool Enabled { get; set; } = true;
                public bool AlwaysShow { get; set; } = false;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; } = new int[2] { 1110, 30 };
            }

            public class Primarymantle {
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; } = new int[2] { 1145, 300 };
                public string Color { get; set; } = "#99C500AA";
            }

            public class Secondarymantle {
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; } = new int[2] { 1145, 350 };
                public string Color { get; set; } = "#996900FF";
            }

            public class DPSMeter {
                public bool Enabled { get; set; } = true;
                public bool ShowDPSWheneverPossible { get; set; } = false;
                public double Scale { get; set; } = 0.8;
                public int[] Position { get; set; } = new int[2] { 10, 350 };
                public Players[] PartyMembers { get; set; } = new Players[4] { new Players(), new Players(), new Players(), new Players() };
            }

            public class Players {
                public string Color { get; set; } = "#FFFFFF";
            }

            public class Richpresence {
                public bool Enabled { get; set; } = true;
                public bool ShowMonsterHealth { get; set; } = true;
            }

            public class Hunterpie {
                public string Language { get; set; } = @"Languages\en-us.xml";
                public string Theme { get; set; } = null;
                public bool MinimizeToSystemTray { get; set; } = true;
                public bool StartHunterPieMinimized { get; set; } = false;
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

            public class AbnormalitiesWidget {
                public int ActiveBars { get; set; } = 1;
                public AbnormalityBar[] BarPresets { get; set; } = new AbnormalityBar[1] { new AbnormalityBar() };
            }

            public class AbnormalityBar {
                public string Name { get; set; } = "Abnormality Tray";
                public int[] Position { get; set; } = new int[2] { 500, 60 };
                public string Orientation { get; set; } = "Horizontal";
                public int MaxSize { get; set; } = 300;
                public double Scale { get; set; } = 1;
                public string[] AcceptedAbnormalities { get; set; } = new string[1] { "*" };
                public bool Enabled { get; set; } = true;
                public byte OrderBy { get; set; } = 0;
                public bool ShowTimeLeftText { get; set; } = true;
                public byte TimeLeftTextFormat { get; set; } = 0;
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

        public static void RemoveFileWatcher() {
            if (ConfigWatcher == null) return;
            ConfigWatcher.Changed -= OnConfigChanged;
            ConfigWatcher.Dispose();
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
            try {
                File.WriteAllText(ConfigFileName, d_Config);
            } catch(Exception err) {
                Debugger.Log($"Failed to create new config!{err}");
            }
            
        }

        private static string LoadPlayerSerializedConfig() {
            string configContent;
            try {
                configContent = File.ReadAllText(ConfigFileName);
                if (configContent == "null") throw new Exception("Config.json is null");
            } catch (IOException err) {
                Debugger.Error("Config.json could not be loaded. Re-trying again...");
                configContent = ConfigSerialized;
            } catch(Exception err) {
                Debugger.Error($"Failed to parse config.json!\n{err}");
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
            } catch(Exception err) {
                Debugger.Error($"Failed to parse config.json!\n{err}");
            }
        }

        public static void SaveNewConfig() {
            try {
                string newPlayerConfig = JsonConvert.SerializeObject(PlayerConfig, Formatting.Indented);
                File.WriteAllText(ConfigFileName, newPlayerConfig);
            } catch(Exception err) {
                Debugger.Error($"Failed to save config.json!\n{err}");
            }
        }

        public static void AddNewAbnormalityBar(int Amount) {
            // Kinda hacky. TODO: Change this to something better
            List<Config.AbnormalityBar> AbnormalityBars = PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.ToList();
            int oldCount = AbnormalityBars.Count;
            for (int i = 0; i < Amount; i++) {
                AbnormalityBars.Add(new Config.AbnormalityBar());
                AbnormalityBars[oldCount].AcceptedAbnormalities = new string[1] { "*" };
                oldCount++;
            }
            PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets = AbnormalityBars.ToArray();
        }

        public static void RemoveAbnormalityBars() {
            List<Config.AbnormalityBar> AbnormalityBars = PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.ToList();
            AbnormalityBars.RemoveAt(AbnormalityBars.Count - 1);
            PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets = AbnormalityBars.ToArray();
        }
    }
}
