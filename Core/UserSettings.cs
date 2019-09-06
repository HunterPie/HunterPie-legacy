using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace HunterPie.Core {
    class UserSettings {

        static private string ConfigFileName = @"config.json";
        public static Config.Rootobject PlayerConfig;
        private static string ConfigSerialized;
        
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
                public Monsterscomponent MonstersComponent { get; set; } = new Monsterscomponent();
                public Harvestboxcomponent HarvestBoxComponent { get; set; } = new Harvestboxcomponent();
                public Primarymantle PrimaryMantle { get; set; } = new Primarymantle();
                public Secondarymantle SecondaryMantle { get; set; } = new Secondarymantle();
            }

            public class Monsterscomponent {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 335, 10 };
            }

            public class Harvestboxcomponent {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 1110, 30 };
            }

            public class Primarymantle {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 1170, 500 };
            }

            public class Secondarymantle {
                public bool Enabled { get; set; } = true;
                public int[] Position { get; set; } = new int[2] { 1170, 540 };
            }

            public class Richpresence {
                public bool Enabled { get; set; } = true;
            }

            public class Hunterpie {
                public Update Update { get; set; } = new Update();
            }

            public class Update {
                public bool Enabled { get; set; } = true;
                public string Branch { get; set; } = "master";
            }

        }

        private static Config.Rootobject Default_Config = new Config.Rootobject {
            Overlay = new Config.Overlay {
                Enabled = true,
                Position = new int[2] { 0, 0 },
                MonstersComponent = new Config.Monsterscomponent {
                    Enabled = true,
                    Position = new int[2] { 335, 10 }
                },
                HarvestBoxComponent = new Config.Harvestboxcomponent {
                    Enabled = true,
                    Position = new int[2] { 1110, 30 }
                },
                PrimaryMantle = new Config.Primarymantle {
                    Enabled = true,
                    Position = new int[2] { 1170, 500 }
                },
                SecondaryMantle = new Config.Secondarymantle {
                    Enabled = true,
                    Position = new int[2] { 1170, 540 }
                }
            },
            RichPresence = new Config.Richpresence {
                Enabled = true
            },
            HunterPie = new Config.Hunterpie {
                Update = new Config.Update {
                    Enabled = true,
                    Branch = "master"
                }
            }
        };

        public static string GetSerializedDefaultConfig() {
            return JsonConvert.SerializeObject(Default_Config, Formatting.Indented);
        }

        public static void MakeNewConfig() {
            string d_Config = GetSerializedDefaultConfig();
            File.WriteAllText(ConfigFileName, d_Config);
        }

        private static void LoadPlayerSerializedConfig() {
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
                Debugger.Warn("Config.json loaded!");
            }
        }

        public static void LoadPlayerConfig() {
            LoadPlayerSerializedConfig();
            PlayerConfig = JsonConvert.DeserializeObject<Config.Rootobject>(ConfigSerialized);
        }

        public static void SaveNewConfig() {
            string newPlayerConfig = JsonConvert.SerializeObject(PlayerConfig, Formatting.Indented);
            File.WriteAllText(ConfigFileName, newPlayerConfig);
        }


    }
}
