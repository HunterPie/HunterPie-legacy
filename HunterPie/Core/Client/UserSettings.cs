using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HunterPie.Logger;
using Newtonsoft.Json;

namespace HunterPie.Core
{
    public class UserSettings
    {

        // Config file watcher
        private static FileSystemWatcher ConfigWatcher;

        private static readonly string ConfigFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public static Config.Rootobject PlayerConfig;
        private static string ConfigSerialized;

        // Config events
        public delegate void SettingsEvents(object source, EventArgs args);
        public static event SettingsEvents OnSettingsUpdate;

        protected static void _onSettingsUpdate() => OnSettingsUpdate?.Invoke(typeof(UserSettings), EventArgs.Empty);

        // Config template
        public class Config
        {

            public class Rootobject
            {
                public Overlay Overlay { get; set; } = new Overlay();
                public Richpresence RichPresence { get; set; } = new Richpresence();
                public Hunterpie HunterPie { get; set; } = new Hunterpie();
            }

            public class Overlay
            {
                public bool Enabled { get; set; } = true;
                public int DesiredAnimationFrameRate { get; set; } = 30;
                public int GameScanDelay { get; set; } = 150;
                public int[] Position { get; set; } = new int[2] { 0, 0 };
                public string ToggleOverlayKeybind { get; set; } = "Ctrl+Alt+Z";
                public bool EnableHardwareAcceleration { get; set; } = true;
                public bool HideWhenGameIsUnfocused { get; set; } = false;
                public int ToggleDesignModeKey { get; set; } = 145;
                public string ToggleDesignKeybind { get; set; } = "ScrollLock";
                public Monsterscomponent MonstersComponent { get; set; } = new Monsterscomponent();
                public Harvestboxcomponent HarvestBoxComponent { get; set; } = new Harvestboxcomponent();
                public SpecializedTool PrimaryMantle { get; set; } = new SpecializedTool()
                {
                    Color = "#FF80FFFF",
                    Position = new int[2] { 1145, 300 }
                };
                public SpecializedTool SecondaryMantle { get; set; } = new SpecializedTool()
                {
                    Color = "#FF9854E2",
                    Position = new int[2] { 1145, 350 }
                };
                public DPSMeter DPSMeter { get; set; } = new DPSMeter();
                public AbnormalitiesWidget AbnormalitiesWidget { get; set; } = new AbnormalitiesWidget();
                public ClassesWidget ClassesWidget { get; set; } = new ClassesWidget();
            }

            public class Monsterscomponent
            {
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public string HealthTextFormat { get; set; } = "{Health:0}/{TotalHealth:0} ({Percentage:0}%)";
                public byte ShowMonsterBarMode { get; set; } = 0;
                public string SwitchMonsterBarModeHotkey = "Alt+Up";
                public int[] Position { get; set; } = new int[2] { 335, 10 };
                public byte MonsterBarDock { get; set; } = 0;
                public int MaxNumberOfPartsAtOnce { get; set; } = 8;
                public int MaxPartColumns { get; set; } = 1;
                public bool ShowMonsterWeakness { get; set; } = true;
                public bool HidePartsAfterSeconds { get; set; } = true;
                public int SecondsToHideParts { get; set; } = 10;
                public bool EnableRemovableParts { get; set; } = true;
                public bool EnableMonsterParts { get; set; } = true;
                public bool EnableMonsterAilments { get; set; } = true;
                public string[] EnabledPartGroups { get; set; } = new string[20] { "HEAD", "BODY", "ARM", "WING", "LEG", "TAIL", "LIMB", "ABDOMEN", "CHEST", "REAR", "JAW", "BACK", "FIN", "HORN", "NECK", "SHELL", "ORGAN", "MISC", "MANE", "BONE" };
                public float Opacity { get; set; } = 1;
                public bool UseLockonInsteadOfPin { get; set; } = false;
            }

            public class Harvestboxcomponent
            {
                public bool Enabled { get; set; } = true;
                public bool AlwaysShow { get; set; } = false;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; } = new int[2] { 1110, 30 };
                public bool ShowSteamTracker { get; set; } = true;
                public bool ShowArgosyTracker { get; set; } = true;
                public bool ShowTailraidersTracker { get; set; } = true;
                public float BackgroundOpacity { get; set; } = 0.5f;
                public float Opacity { get; set; } = 1;
                public bool CompactMode { get; set; } = false;
            }

            public class SpecializedTool
            {
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; }
                public string Color { get; set; }
                public float Opacity { get; set; } = 1;
            }

            public class DPSMeter
            {
                public bool Enabled { get; set; } = true;
                public bool ShowTotalDamage { get; set; } = true;
                public bool ShowDPSWheneverPossible { get; set; } = true;
                public double Scale { get; set; } = 0.8;
                public int[] Position { get; set; } = new int[2] { 10, 350 };
                public Players[] PartyMembers { get; set; } = new Players[4] { new Players() { Color = "#FFE14136" }, new Players() { Color = "#FF65B2B7" }, new Players() { Color = "#FFECE2A0" }, new Players() { Color = "#FF4AAB3F" } };
                public bool ShowOnlyMyself { get; set; } = false;
                public bool ShowTimerInExpeditions { get; set; } = true;
                public float BackgroundOpacity { get; set; } = 0.5f;
                public float Opacity { get; set; } = 1;
                public bool ShowOnlyTimer { get; set; } = false;
                public bool ShowTimer { get; set; } = true;
            }

            public class Players
            {
                public string Color { get; set; } = "#FFFFFF";
            }

            public class Richpresence
            {
                public bool Enabled { get; set; } = true;
                public bool ShowMonsterHealth { get; set; } = true;
                public bool LetPeopleJoinSession { get; set; } = true;
            }

            public class Hunterpie
            {
                public string Language { get; set; } = @"Languages\en-us.xml";
                public string Theme { get; set; } = null;
                public bool MinimizeToSystemTray { get; set; } = true;
                public bool StartHunterPieMinimized { get; set; } = false;
                public float Width { get; set; } = 1000;
                public float Height { get; set; } = 590;
                public Update Update { get; set; } = new Update();
                public Launch Launch { get; set; } = new Launch();
                public Options Options { get; set; } = new Options();
                public Debug Debug { get; set; } = new Debug();
            }

            public class Update
            {
                public bool Enabled { get; set; } = true;
                public string Branch { get; set; } = "master";
            }

            public class Launch
            {
                public string GamePath { get; set; } = "";
                public string LaunchArgs { get; set; } = "";
            }

            public class Options
            {
                public bool CloseWhenGameCloses { get; set; } = false;
            }

            public class AbnormalitiesWidget
            {
                public int ActiveBars { get; set; } = 1;
                public AbnormalityBar[] BarPresets { get; set; } = new AbnormalityBar[1] { new AbnormalityBar() };
            }

            public class AbnormalityBar
            {
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
                public float BackgroundOpacity { get; set; } = 0.7f;
                public bool ShowNames { get; set; } = false;
            }

            public class ClassesWidget
            {
                public DualBladesHelper DualBladesHelper { get; set; } = new DualBladesHelper();
                public LongSwordHelper LongSwordHelper { get; set; } = new LongSwordHelper();
                public HammerHelper HammerHelper { get; set; } = new HammerHelper();
                public LanceHelper LanceHelper { get; set; } = new LanceHelper();
                public ChargeBladeHelper ChargeBladeHelper { get; set; } = new ChargeBladeHelper();
                public InsectGlaiveHelper InsectGlaiveHelper { get; set; } = new InsectGlaiveHelper();
                public GunLanceHelper GunLanceHelper { get; set; } = new GunLanceHelper();
                public SwitchAxeHelper SwitchAxeHelper { get; set; } = new SwitchAxeHelper();
                public BowHelper BowHelper { get; set; } = new BowHelper();
            }

            public class HammerHelper : WeaponHelperStructure { }

            public class ChargeBladeHelper : WeaponHelperStructure { }

            public class InsectGlaiveHelper : WeaponHelperStructure { }

            public class GunLanceHelper : WeaponHelperStructure { }

            public class SwitchAxeHelper : WeaponHelperStructure { }

            public class LongSwordHelper : WeaponHelperStructure { }

            public class BowHelper : WeaponHelperStructure { }

            public class DualBladesHelper : WeaponHelperStructure { }

            public class LanceHelper : WeaponHelperStructure { };

            public class WeaponHelperStructure
            {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 683, 384 };
                public float Opacity { get; set; } = 1f;
                public float Scale { get; set; } = 1f;
            }

            public class Debug
            {
                public bool ShowUnknownStatuses { get; set; } = false;
                public bool ShowDebugMessages { get; set; } = false;
                public string CustomMonsterData { get; set; } = null;
                public bool LoadCustomMonsterData { get; set; } = false;
            }
        }

        public static void TriggerSettingsEvent() => _onSettingsUpdate();

        public static void InitializePlayerConfig()
        {
            // This is called only once when HunterPie starts
            LoadPlayerConfig();
            SaveNewConfig();
            CreateFileWatcher();
        }

        private static void CreateFileWatcher()
        {
            // Prevents it from hooking the event multiple times
            if (ConfigWatcher != null) return;
            ConfigWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(ConfigFileName),
                Filter = Path.GetFileName(ConfigFileName)
            };
            ConfigWatcher.Changed += OnConfigChanged;
            ConfigWatcher.EnableRaisingEvents = true;
        }

        public static void RemoveFileWatcher()
        {
            if (ConfigWatcher == null) return;
            ConfigWatcher.Changed -= OnConfigChanged;
            ConfigWatcher.Dispose();
        }

        private static void OnConfigChanged(object source, FileSystemEventArgs e)
        {
            // Use try/catch because FileSystemWatcher sends the same event twice
            // and one of them is when the file is still open 
            try
            {
                using (var fw = File.OpenRead(e.FullPath))
                {
                    fw.Close();
                }
                LoadPlayerConfig();
            }
            catch { }
        }

        public static string GetSerializedDefaultConfig() => JsonConvert.SerializeObject(new Config.Rootobject(), Formatting.Indented);

        public static void MakeNewConfig()
        {
            string d_Config = GetSerializedDefaultConfig();
            try
            {
                File.WriteAllText(ConfigFileName, d_Config);
            }
            catch (Exception err)
            {
                Debugger.Log($"Failed to create new config!{err}");
            }

        }

        private static string TryGetConfig()
        {
            try
            {
                string c = File.ReadAllText(ConfigFileName);
                return c;
            }
            catch
            {
                return null;
            }
        }

        private static string LoadPlayerSerializedConfig()
        {
            string configContent;
            if (!File.Exists(ConfigFileName))
            {
                Debugger.Error($"Config.json was missing. Creating a new one.");
                MakeNewConfig();
            }
            try
            {
                configContent = File.ReadAllText(ConfigFileName);
                if (configContent == "null") throw new Exception("config.json was null");
            }
            catch (IOException err)
            {
                // If there was an IOException, we just use the default config instead
                Debugger.Error($"Config.json could not be loaded.\n{err}");
                configContent = TryGetConfig() ?? GetSerializedDefaultConfig();
            }
            catch (Exception err)
            {
                Debugger.Error($"Failed to parse config.json!\n{err}");
                Debugger.Warn("Generating new config");
                MakeNewConfig();
                configContent = File.ReadAllText(ConfigFileName);
            }
            if (ConfigSerialized != configContent)
            {
                ConfigSerialized = configContent;
                return ConfigSerialized;
            }
            return null;
        }

        public static void LoadPlayerConfig()
        {
            LoadPlayerSerializedConfig();
            try
            {
                if (ConfigSerialized == null) return;
                PlayerConfig = JsonConvert.DeserializeObject<Config.Rootobject>(ConfigSerialized);
                _onSettingsUpdate();
            }
            catch (Exception err)
            {
                Debugger.Error($"Failed to parse config.json!\n{err}");
            }
        }

        public static void SaveNewConfig()
        {
            try
            {
                string newPlayerConfig = JsonConvert.SerializeObject(PlayerConfig, Formatting.Indented);
                if (newPlayerConfig == "null") throw new Exception("Whoops! Something went wrong when trying to save your config!");
                File.WriteAllText(ConfigFileName, newPlayerConfig);
            }
            catch (Exception err)
            {
                Debugger.Error($"Failed to save config.json!\n{err}");
            }
        }

        public static void AddNewAbnormalityBar(int Amount)
        {
            // Kinda hacky. TODO: Change this to something better
            List<Config.AbnormalityBar> AbnormalityBars = PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.ToList();
            int oldCount = AbnormalityBars.Count;
            for (int i = 0; i < Amount; i++)
            {
                AbnormalityBars.Add(new Config.AbnormalityBar());
                AbnormalityBars[oldCount].AcceptedAbnormalities = new string[1] { "*" };
                oldCount++;
            }
            PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets = AbnormalityBars.ToArray();
        }

        public static void RemoveAbnormalityBars()
        {
            List<Config.AbnormalityBar> AbnormalityBars = PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets.ToList();
            AbnormalityBars.RemoveAt(AbnormalityBars.Count - 1);
            PlayerConfig.Overlay.AbnormalitiesWidget.BarPresets = AbnormalityBars.ToArray();
        }
    }
}
