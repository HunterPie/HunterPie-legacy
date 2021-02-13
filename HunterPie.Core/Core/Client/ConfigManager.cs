using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static HunterPie.Logger.Debugger;
using HunterPie.Core.Settings;
using HunterPie.Utils;

namespace HunterPie.Core
{
    public class ConfigManager
    {
        private readonly static FileSystemWatcher fileSystemWatcher = new FileSystemWatcher()
        {
            Path = AbsolutePath,
            Filter = ConfigFileName,
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        public const string ConfigFileName = "config.json";
        public const string ConfigBackupFileName = ConfigFileName + ".bak";

        public static string AbsolutePath => AppDomain.CurrentDomain.BaseDirectory;
        public static string AbsoluteConfigPath => Path.Combine(AbsolutePath, ConfigFileName);
        public static string AbsoluteBackupPath => Path.Combine(AbsolutePath, ConfigBackupFileName);

        public static Config Settings { get; private set; }
        public static readonly Config Default = new Config();

        public static event EventHandler<EventArgs> OnSettingsUpdate;

        private static void Dispatch(EventHandler<EventArgs> e)
        {
            e?.Invoke(nameof(ConfigManager), EventArgs.Empty);
        }

        internal static async Task Initialize()
        {
            await TryLoadSettings();
            Action loadSettings = async () => await TryLoadSettings();
            var debounceLoadSettings = loadSettings.Debounce(100);

            fileSystemWatcher.Changed += (_, _) => debounceLoadSettings();
        }

        internal static async Task<bool> TryLoadSettings()
        {
            // if there is no config nor backup, notify user
            if (!File.Exists(AbsoluteConfigPath) && !File.Exists(AbsoluteBackupPath))
            {
                Log("config.json is missing. Creating a new one...");
            }

            try
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                var loadingFailed = false;
                var createConfig = false;

                // try load config if present...
                if (File.Exists(AbsoluteConfigPath))
                {
                    Settings = await ReadSettingsAsync(AbsoluteConfigPath);
                    if (Settings == null)
                    {
                        loadingFailed = true;
                    }
                }

                // ...otherwise use one from backup
                if (Settings == null && File.Exists(AbsoluteBackupPath))
                {
                    Settings = await ReadSettingsAsync(AbsoluteBackupPath);
                    createConfig = true;
                }

                // ...if backup restore failed, use default settings
                if (Settings == null)
                {
                    Settings = Default;
                    createConfig = true;
                }

                // if cannot load config from both sources, throw error
                if (Settings == null)
                    throw new NullReferenceException("Cannot read config");

                // at this point we're sure that we have at least some config, notify user
                // that config restore was done
                if (loadingFailed || createConfig)
                {
                    await TrySaveSettingsAsync();
                    Log(loadingFailed ? "Config restored from backup" : "Config created");
                }

                Dispatch(OnSettingsUpdate);

                return true;
            }
            catch (Exception err)
            {
                Error(err);
                return false;
            }
            finally
            {
                fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        internal static void TriggerSettingsEvent()
        {
            Dispatch(OnSettingsUpdate);
        }

        public static async Task<bool> TrySaveSettingsAsync()
        {
            string serialized = JsonConvert.SerializeObject(Settings, Formatting.Indented);

            if (serialized == JsonConvert.SerializeObject(Default))
                return false;

            byte[] buffer = Encoding.UTF8.GetBytes(serialized);
            try
            {
                // Write to dummy first
                using (FileStream stream = File.OpenWrite(AbsoluteBackupPath))
                {
                    stream.SetLength(0);
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                }

                // Check if the write was successful
                using (FileStream stream = File.OpenRead(AbsoluteBackupPath))
                {
                    byte[] rBuffer = new byte[stream.Length];
                    await stream.ReadAsync(rBuffer, 0, rBuffer.Length);

                    string decoded = Encoding.UTF8.GetString(buffer);

                    if (rBuffer[0] == 0x00 || decoded == "null" || string.IsNullOrEmpty(decoded))
                        throw new Exception("Something went wrong when saving your config.json");

                    // Then we move the data to our actual config.json
                    using (FileStream output = File.OpenWrite(AbsoluteConfigPath))
                    {
                        output.SetLength(0);
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                }

            } catch (Exception err)
            {
                Error(err);
                return false;
            }
            return true;
        }

        private static async Task<Config> ReadSettingsAsync(string path)
        {
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    byte[] buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);

                    string str = Encoding.UTF8.GetString(buffer);

                    if (string.IsNullOrEmpty(str) || str[0] == '\x00' || str == "null")
                        throw new Exception("Config was empty");

                    return JsonConvert.DeserializeObject<Config>(str);
                }
            } catch(Exception err)
            {
                Error(err);
                return null;
            }
        }

        public static void AddNewAbnormalityBar(int Amount)
        {
            // Kinda hacky. TODO: Change this to something better
            List<AbnormalityBar> AbnormalityBars = Settings.Overlay.AbnormalitiesWidget.BarPresets.ToList();
            int oldCount = AbnormalityBars.Count;
            for (int i = 0; i < Amount; i++)
            {
                AbnormalityBars.Add(new AbnormalityBar());
                AbnormalityBars[oldCount].AcceptedAbnormalities = new string[1] { "*" };
                oldCount++;
            }
            Settings.Overlay.AbnormalitiesWidget.BarPresets = AbnormalityBars.ToArray();
        }

        public static void RemoveAbnormalityBars()
        {
            List<AbnormalityBar> AbnormalityBars = Settings.Overlay.AbnormalitiesWidget.BarPresets.ToList();
            AbnormalityBars.RemoveAt(AbnormalityBars.Count - 1);
            Settings.Overlay.AbnormalitiesWidget.BarPresets = AbnormalityBars.ToArray();
        }

    }
}
