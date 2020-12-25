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
        private static readonly string ConfigBackupFileName = ConfigFileName + ".bak";
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
                public bool Initialize { get; set; } = true;
                public bool Enabled { get; set; } = true;
                public int DesiredAnimationFrameRate { get; set; } = 30;
                public int GameScanDelay { get; set; } = 150;
                public int[] Position { get; set; } = new int[] { 0, 0 };
                public string ToggleOverlayKeybind { get; set; } = "Ctrl+Alt+Z";
                public bool EnableHardwareAcceleration { get; set; } = true;
                public bool EnableAntiAliasing { get; set; } = true;
                public bool HideWhenGameIsUnfocused { get; set; } = false;
                public int ToggleDesignModeKey { get; set; } = 145;
                public string ToggleDesignKeybind { get; set; } = "ScrollLock";
                public bool EnableForceDirectX11Fullscreen { get; set; } = false;
                public Monsterscomponent MonstersComponent { get; set; } = new Monsterscomponent();
                public Harvestboxcomponent HarvestBoxComponent { get; set; } = new Harvestboxcomponent();
                public PlayerHealthComponent PlayerHealthComponent { get; set; } = new PlayerHealthComponent();
                public SpecializedTool PrimaryMantle { get; set; } = new SpecializedTool()
                {
                    Color = "#FF80FFFF",
                    Position = new int[] { 1145, 300 }
                };
                public SpecializedTool SecondaryMantle { get; set; } = new SpecializedTool()
                {
                    Color = "#FF9854E2",
                    Position = new int[] { 1145, 350 }
                };
                public DPSMeter DPSMeter { get; set; } = new DPSMeter();
                public AbnormalitiesWidget AbnormalitiesWidget { get; set; } = new AbnormalitiesWidget();
                public ClassesWidget ClassesWidget { get; set; } = new ClassesWidget();
            }

            public interface IWidgetSettings
            {
                bool Initialize { get; set; }
                bool Enabled { get; set; }
                int[] Position { get; set; }
                float Opacity { get; set; }
                double Scale { get; set; }
            }
            public class PlayerHealthComponent : IWidgetSettings
            {
                public bool Initialize { get; set; } = true;
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public string NameTextFormat { get; set; } = "Lv. {MR} {Name}";
                public bool HideHealthInVillages { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 20, 20 };
                public float Opacity { get; set; } = 1;
            }

            public class Monsterscomponent : IWidgetSettings
            {
                public bool Initialize { get; set; } = true;
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public string HealthTextFormat { get; set; } = "{Health:0}/{TotalHealth:0} ({Percentage:0}%)";
                public bool HideHealthInformation { get; set; } = false;
                public byte ShowMonsterBarMode { get; set; } = 0;
                public string SwitchMonsterBarModeHotkey = "Alt+Up";
                public int[] Position { get; set; } = new int[] { 335, 10 };
                public byte MonsterBarDock { get; set; } = 0;
                public int MaxNumberOfPartsAtOnce { get; set; } = 8;
                public int MaxPartColumns { get; set; } = 1;
                public bool ShowMonsterWeakness { get; set; } = true;
                public bool HidePartsAfterSeconds { get; set; } = true;
                public bool HideAilmentsAfterSeconds { get; set; } = true;
                public int SecondsToHideParts { get; set; } = 10;
                public bool EnableRemovableParts { get; set; } = true;
                public bool EnableMonsterParts { get; set; } = true;
                public bool EnableSortParts { get; set; } = true;
                public bool EnableOnlyPartsThatCanBeBroken { get; set; } = false;
                public bool HidePartsThatHaveAlreadyBeenBroken { get; set; } = false;
                public bool EnableMonsterAilments { get; set; } = true;
                public string[] EnabledPartGroups { get; set; } = new string[] { "HEAD", "BODY", "ARM", "WING", "LEG", "TAIL", "LIMB", "ABDOMEN", "CHEST", "REAR", "JAW", "BACK", "FIN", "HORN", "NECK", "SHELL", "ORGAN", "MISC", "MANE", "BONE" };
                public string[] EnabledAilmentGroups { get; set; } = new string[] { "POISON", "PARALYSIS", "SLEEP", "BLAST", "MOUNT", "EXHAUSTION", "STUN", "TRANQUILIZE", "FLASH", "KNOCKDOWN", "DUNGPOD", "TRAP", "ELDERSEAL", "SMOKING", "CLAW", "MISC", "ENRAGE", "UNKNOWN" };
                public float Opacity { get; set; } = 1;
                public bool UseLockonInsteadOfPin { get; set; } = false;
                public bool EnableAilmentsBarColor { get; set; } = true;
                public string AilmentBuildupTextFormat { get; set; } = "{Current}/{Max}";
                public string AilmentTimerTextFormat { get; set; } = "{Current}/{Max}";
                public string PartTextFormat { get; set; } = "{Current}/{Max}";
                public bool ShowMonsterActionName { get; set; } = true;
            }

            public class Harvestboxcomponent : IWidgetSettings
            {
                public bool Initialize { get; set; } = true;
                public bool Enabled { get; set; } = true;
                public bool AlwaysShow { get; set; } = false;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; } = new int[] { 1110, 30 };
                public bool ShowSteamTracker { get; set; } = true;
                public bool ShowArgosyTracker { get; set; } = true;
                public bool ShowTailraidersTracker { get; set; } = true;
                public float BackgroundOpacity { get; set; } = 0.5f;
                public float Opacity { get; set; } = 1;
                public bool CompactMode { get; set; } = false;
            }

            public class SpecializedTool : IWidgetSettings
            {
                public bool Initialize { get; set; } = true;
                public bool Enabled { get; set; } = true;
                public double Scale { get; set; } = 1;
                public int[] Position { get; set; }
                public string Color { get; set; }
                public float Opacity { get; set; } = 1;
                public bool CompactMode { get; set; } = false;
            }

            public class DPSMeter : IWidgetSettings
            {
                public bool Initialize { get; set; } = true;
                public bool Enabled { get; set; } = true;
                public bool ShowTotalDamage { get; set; } = true;
                public bool ShowDPSWheneverPossible { get; set; } = true;
                public double Scale { get; set; } = 0.8;
                public int[] Position { get; set; } = new int[] { 10, 350 };
                public Players[] PartyMembers { get; set; } = new Players[] { new Players() { Color = "#FFE14136" }, new Players() { Color = "#FF65B2B7" }, new Players() { Color = "#FFECE2A0" }, new Players() { Color = "#FF4AAB3F" } };
                public bool ShowOnlyMyself { get; set; } = false;
                public bool ShowTimerInExpeditions { get; set; } = true;
                public float BackgroundOpacity { get; set; } = 0.5f;
                public float Opacity { get; set; } = 1;
                public bool ShowOnlyTimer { get; set; } = false;
                public bool ShowTimer { get; set; } = true;
                public double Width { get; set; } = 1;
                public bool EnableDamagePlot { get; set; } = true;
                public Enums.DamagePlotMode DamagePlotMode { get; set; } = Enums.DamagePlotMode.CumulativeTotal;
                public int DamagePlotPollInterval { get; set; } = 200;
            }

            public class AbnormalityBar : IWidgetSettings
            {
                public bool Initialize { get; set; } = true;
                public string Name { get; set; } = "Abnormality Tray";
                public int[] Position { get; set; } = new int[] { 500, 60 };
                public string Orientation { get; set; } = "Horizontal";
                public int MaxSize { get; set; } = 300;
                public double Scale { get; set; } = 1;
                public string[] AcceptedAbnormalities { get; set; } = new string[1] { "*" };
                public bool Enabled { get; set; } = true;
                public byte OrderBy { get; set; } = 0;
                public bool ShowTimeLeftText { get; set; } = true;
                public byte TimeLeftTextFormat { get; set; } = 0;
                public float Opacity { get; set; } = 1;
                public float BackgroundOpacity { get; set; } = 0.7f;
                public bool ShowNames { get; set; } = false;
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
                public double PosX { get; set; } = 0;
                public double PosY { get; set; } = 0;
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
                public AbnormalityBar[] BarPresets { get; set; } = new AbnormalityBar[] { new AbnormalityBar() };
            }

            public class ClassesWidget
            {
                public bool Initialize { get; set; } = true;
                public GreatswordHelper GreatswordHelper { get; set; } = new GreatswordHelper();
                public DualBladesHelper DualBladesHelper { get; set; } = new DualBladesHelper();
                public LongSwordHelper LongSwordHelper { get; set; } = new LongSwordHelper();
                public HammerHelper HammerHelper { get; set; } = new HammerHelper();
                public LanceHelper LanceHelper { get; set; } = new LanceHelper();
                public HuntingHornHelper HuntingHornHelper { get; set; } = new HuntingHornHelper();
                public ChargeBladeHelper ChargeBladeHelper { get; set; } = new ChargeBladeHelper();
                public InsectGlaiveHelper InsectGlaiveHelper { get; set; } = new InsectGlaiveHelper();
                public GunLanceHelper GunLanceHelper { get; set; } = new GunLanceHelper();
                public SwitchAxeHelper SwitchAxeHelper { get; set; } = new SwitchAxeHelper();
                public BowHelper BowHelper { get; set; } = new BowHelper();
                public HeavyBowgunHelper HeavyBowgunHelper { get; set; } = new HeavyBowgunHelper();
                public LightBowgunHelper LightBowgunHelper { get; set; } = new LightBowgunHelper();
            }

            public class GreatswordHelper : IWeaponHelper
            {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class HammerHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class ChargeBladeHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class InsectGlaiveHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class GunLanceHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class SwitchAxeHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class LongSwordHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class BowHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class DualBladesHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class LanceHelper : IWeaponHelper {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            // TODO: Add custom settings for the song list
            public class HuntingHornHelper : IWeaponHelper {
                public bool Enabled { get; set; } = false;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class HeavyBowgunHelper : IWeaponHelper
            {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public class LightBowgunHelper : IWeaponHelper
            {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[] { 683, 384 };
                public float Opacity { get; set; } = 1;
                public float Scale { get; set; } = 1;
            }

            public interface IWeaponHelper
            {
                bool Enabled { get; set; }
                int[] Position { get; set; }
                float Opacity { get; set; }
                float Scale { get; set; }
            }

            public class Debug
            {
                public bool ShowUnknownStatuses { get; set; } = false;
                public bool ShowDebugMessages { get; set; } = false;
                public string CustomMonsterData { get; set; } = null;
                public bool LoadCustomMonsterData { get; set; } = false;
                public bool SendCrashFileToDev { get; set; } = true;
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
                if (configContent[0] == '\x00' || string.IsNullOrEmpty(configContent) || configContent == "null")
                {
                    Debugger.Warn("config.json was corrupted. Trying to load backup config instead.");

                    if (File.Exists(ConfigBackupFileName))
                    {
                        configContent = File.ReadAllText(ConfigBackupFileName);

                        if (configContent[0] == '\x00' || string.IsNullOrEmpty(configContent) || configContent == "null")
                            throw new Exception("Backup config was also corrupted!");

                    }
                    else
                    {
                        throw new FileNotFoundException("No backup config found!");
                    }
                }

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
                if (ConfigSerialized == null)
                    return;

                // Check if config is valid

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

                if (newPlayerConfig[0] == '\x00')
                    throw new Exception("Whoops! Something went wrong when trying to save your config!");

                File.WriteAllText(ConfigBackupFileName, newPlayerConfig);

                // Check if the config is valid, this is necessary so we don't overwrite the
                // current config with a corrupted one

                string backupConfig = File.ReadAllText(ConfigBackupFileName);

                if (backupConfig[0] != '\x00')
                {
                    string backup = File.ReadAllText(ConfigBackupFileName);
                    File.WriteAllText(ConfigFileName, backup);
                }

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
