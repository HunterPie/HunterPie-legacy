using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;
using HunterPie.Core.Definitions;
using HunterPie.Core.Integrations.DataExporter;
using HunterPie.GUIControls;
using HunterPie.GUIControls.Custom_Controls;
using HunterPie.Plugins;
using Debugger = HunterPie.Logger.Debugger;
using PluginDisplay = HunterPie.GUIControls.Plugins;
using HunterPie.Core.Input;
// HunterPie
using HunterPie.Memory;
using Presence = HunterPie.Core.Integrations.Discord.Presence;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;
using System.Threading.Tasks;
using System.Net.Http;
using HunterPie.Core.Craft;
using Newtonsoft.Json;
using System.Diagnostics;
using HunterPie.Core.Events;
using System.Xml;
using System.Reflection;
using HunterPie.Core.Native;
using HunterPie.Native;
using HunterPie.Native.Connection;
using ConfigManager = HunterPie.Core.ConfigManager;
using HunterPie.Core.Settings;
using HunterPie.UI.Infrastructure;
using Overlay = HunterPie.GUI.Overlay;
using SettingsControl = HunterPie.GUIControls.Settings;

namespace HunterPie
{
    /// <summary>
    /// HunterPie main window logic;
    /// </summary>
    public partial class Hunterpie : Window
    {
        // TODO: Refactor all this messy code

        // Classes
        private TrayIcon trayIcon;
        private readonly Game game = new Game();
        private Presence presence;
        private Overlay overlay;
        private readonly Exporter dataExporter = new Exporter();
        private readonly PluginManager pluginManager = new PluginManager();
        private bool offlineMode = false;
        private bool isUpdating = true;

        public static Hunterpie Instance;

        // HunterPie version
        public const string HUNTERPIE_VERSION = "1.0.5";
        public static readonly Version AssemblyVersion = Assembly.GetEntryAssembly().GetName().Version;

        private readonly List<int> registeredHotkeys = new List<int>();

        private Config config => ConfigManager.Settings;

        #region Properties

        public bool IsPlayerLoggedOn
        {
            get => (bool)GetValue(IsPlayerLoggedOnProperty);
            set => SetValue(IsPlayerLoggedOnProperty, value);
        }
        public static readonly DependencyProperty IsPlayerLoggedOnProperty =
            DependencyProperty.Register("IsPlayerLoggedOn", typeof(bool), typeof(Hunterpie));

        public Visibility AdministratorIconVisibility
        {
            get => (Visibility)GetValue(AdministratorIconVisibilityProperty);
            set => SetValue(AdministratorIconVisibilityProperty, value);
        }
        public static readonly DependencyProperty AdministratorIconVisibilityProperty =
            DependencyProperty.Register("AdministratorIconVisibility", typeof(Visibility), typeof(Hunterpie));

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(Hunterpie));

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }
        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(Hunterpie));

        public ICommand MaximizeCommand { get; }

        #endregion

        public Hunterpie()
        {
            Instance = this;

            if (CheckIfItsRunningFromWinrar())
            {
                MessageBox.Show("You must extract HunterPie files before running it, otherwise it will most likely crash due to missing files or not save your settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            CheckIfHunterPieOpen();

            MaximizeCommand = new ArglessRelayCommand(() => WindowState = WindowState.Maximized);

            AppDomain.CurrentDomain.UnhandledException += ExceptionLogger;

            IsPlayerLoggedOn = false;

            SetDPIAwareness();
            Buffers.Initialize(1024);
            Buffers.Add<byte>(64);

            // Initialize debugger and player config
            DebuggerControl.InitializeDebugger();

            InitializeComponent();
        }

        private bool IsRunningAsAdmin()
        {
            var winIdentity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(winIdentity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void LoadData()
        {
            MonsterData.LoadMonsterData();
            AbnormalityData.LoadAbnormalityData();
            Recipes.LoadRecipes();
        }

        private void CheckIfHunterPieOpen()
        {
            // Block new instances of HunterPie if there's one already running
            Process instance = Process.GetCurrentProcess();
            IEnumerable<Process> processes = Process.GetProcessesByName("HunterPie").Where(p => p.Id != instance.Id);
            foreach (Process p in processes) p.Kill();
        }


        private void SetDPIAwareness()
        {

            if (Environment.OSVersion.Version >= new Version(6, 3, 0))
            {
                if (Environment.OSVersion.Version >= new Version(10, 0, 15063))
                {
                    WindowsHelper.SetProcessDpiAwarenessContext((int)WindowsHelper.DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else
                {
                    WindowsHelper.SetProcessDpiAwareness(WindowsHelper.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
                }
            }
            else
            {
                WindowsHelper.SetProcessDPIAware();
            }
        }

        #region AUTO UPDATE
        private bool StartUpdateProcess()
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe")))
                return false;

            Process UpdateProcess = new Process();
            UpdateProcess.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe");
            UpdateProcess.StartInfo.Arguments = $"version={HUNTERPIE_VERSION} branch={config.HunterPie.Update.Branch}";
            UpdateProcess.Start();
            return true;
        }

        private async Task<bool> CheckIfUpdateEnableAndStart()
        {
            bool serverUp = true;
            try
            {
                using (var httpGet = new HttpClient())
                {
                    await httpGet.GetAsync("https://hunterpie.herokuapp.com/ping");
                }
            } catch {serverUp = false; }

            if (config.HunterPie.Update.Enabled && serverUp)
            {
                bool justUpdated = false;
                bool latestVersion = false;
                string[] args = Environment.GetCommandLineArgs();
                foreach (string argument in args)
                {
                    if (argument.StartsWith("justUpdated"))
                    {
                        string parsed = ParseArgs(argument);
                        justUpdated = parsed == "True";
                    }
                    if (argument.StartsWith("latestVersion"))
                    {
                        string parsed = ParseArgs(argument);
                        latestVersion = parsed == "True";
                    }
                    if (argument.StartsWith("offlineMode"))
                    {
                        offlineMode = ParseArgs(argument) == "True";
                    }
                }
                if (justUpdated)
                {
                    ChangeConsoleChild(ChangelogBtn, Changelog.Instance);
                    return true;
                }
                if (latestVersion)
                {
                    return true;
                }
                Debugger.Log("Updating updater.exe");
                // This will update Update.exe
                AutoUpdate au = new AutoUpdate(config.HunterPie.Update.Branch);
                au.Instance.DownloadFileCompleted += OnUpdaterDownloadComplete;
                offlineMode = au.offlineMode;
                if (!au.CheckAutoUpdate() && !au.offlineMode)
                {
                    HandleUpdaterUpdate();
                }
                else
                {
                    return true;
                }
                Hide();
                return false;
            }
            else
            {
                Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_AUTOUPDATE_DISABLED_WARN']"));
                return true;
            }
        }

        private void OnUpdaterDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_UPDATE_ERROR']"));
                Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_OFFLINEMODE_WARN']"));
                offlineMode = true;
                return;
            }
            else
            {
                Debugger.Error(e.Error);
            }
            HandleUpdaterUpdate();
        }

        private void HandleUpdaterUpdate()
        {
            bool StartUpdate = StartUpdateProcess();
            if (StartUpdate)
            {
                Close();
            }
            else
            {
                MessageBox.Show("Update.exe not found! Skipping auto-update...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ParseArgs(string arg)
        {
            try
            {
                return arg.Split('=')[1];
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region HOT KEYS

        private void SetHotKeys()
        {
            string[] hotkeys =
            {
                config.Overlay.ToggleOverlayKeybind,
                config.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey,
                config.Overlay.ToggleDesignKeybind
            };
            Action[] callbacks =
            {
                ToggleOverlayCallback,
                SwitchMonsterBarModeCallback,
                ToggleDesignModeCallback
            };

            for (int i = 0; i < hotkeys.Length; i++)
            {
                string hotkey = hotkeys[i];
                Action callback = callbacks[i];
                if (hotkey == "None" || hotkey is null)
                {
                    continue;
                }
                int id = Hotkey.Register(hotkey, callback);
                if (id > 0)
                {
                    registeredHotkeys.Add(id);
                } else
                {
                    Debugger.Error("Failed to register hotkey");
                }
            }
        }

        private void RemoveHotKeys()
        {
            foreach (int id in registeredHotkeys)
            {
                Hotkey.Unregister(id);
            }
            registeredHotkeys.Clear();
        }

        private async void ToggleOverlayCallback()
        {
            if (overlay == null)
            {
                return;
            }

            config.Overlay.Enabled = !config.Overlay.Enabled;
            await ConfigManager.TrySaveSettingsAsync();
        }

        private async void SwitchMonsterBarModeCallback()
        {
            if (overlay == null)
            {
                return;
            }

            config.Overlay.MonstersComponent.ShowMonsterBarMode = config.Overlay.MonstersComponent.ShowMonsterBarMode + 1 >= 5 ? (byte)0 : (byte)(config.Overlay.MonstersComponent.ShowMonsterBarMode + 1);
            await ConfigManager.TrySaveSettingsAsync();
        }

        private void ToggleDesignModeCallback()
        {
            overlay?.ToggleDesignMode();
        }

        private void ConvertOldHotkeyToNew(int Key)
        {
            if (Key == 0)
                return;

            config.Overlay.ToggleDesignKeybind = KeyboardHookHelper.GetKeyboardKeyByID(Key).ToString();
            config.Overlay.ToggleDesignModeKey = 0;
        }

        #endregion

        #region TRAY ICON
        private void InitializeTrayIcon()
        {
            // Tray icon itself

            trayIcon = new TrayIcon(
                "HunterPie",
                "HunterPie",
                Properties.Resources.LOGO_HunterPie,
                OnTrayIconClick
            );

            // Settings button
            var settingsItem = trayIcon.AddItem(GStrings.GetLocalizationByXPath("/TrayIcon/String[@ID='TRAYICON_SETTINGS']"));
            settingsItem.Click += OnTrayIconSettingsClick;

            // Close button
            var closeItem = trayIcon.AddItem(GStrings.GetLocalizationByXPath("/TrayIcon/String[@ID='TRAYICON_CLOSE']"));
            closeItem.Click += OnTrayIconExitClick;
        }

        private void OnTrayIconSettingsClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();

            SettingsControl.Instantiate(PluginManager.PluginSettingsTabs);

            ChangeConsoleChild(SettingsBtn, SettingsControl.Instance);
            SettingsControl.RefreshSettingsUI();
        }

        private void OnTrayIconExitClick(object sender, EventArgs e) => Close();

        private void OnTrayIconClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();
        }

        #endregion

        // Why are people running HunterPie before extracting the files??????????????????????
        private bool CheckIfItsRunningFromWinrar()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            return path.Contains("AppData\\Local\\Temp\\");
        }

        private void LoadCustomTheme()
        {
            if (config.HunterPie.Theme == null || config.HunterPie.Theme == "Default") return;
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes"))) { Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes")); }
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Themes/{config.HunterPie.Theme}")))
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_THEME_NOT_FOUND_ERROR']".Replace("{THEME_NAME}", config.HunterPie.Theme)));
                return;
            }
            try
            {
                string themePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Themes/{config.HunterPie.Theme}");

                PatchThemeAndValidate(themePath);

                using (FileStream stream = new FileStream(themePath, FileMode.Open))
                {
                    XamlReader reader = new XamlReader();
                    ResourceDictionary ThemeDictionary = (ResourceDictionary)reader.LoadAsync(stream);
                    Application.Current.Resources.MergedDictionaries.Add(ThemeDictionary);
                    Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_THEME_LOAD_WARN']"));
                }
            }
            catch (Exception err)
            {
                Debugger.Error($"{GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_THEME_NOT_LOAD_ERROR']")}\n{err}");
            }
        }

        private void LoadOverwriteTheme()
        {
            try
            {
                string overwritePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"HunterPie.Resources/UI/Overwrite.xaml");

                PatchThemeAndValidate(overwritePath);

                using (FileStream stream = new FileStream(overwritePath, FileMode.Open))
                {
                    XamlReader reader = new XamlReader();
                    ResourceDictionary res = (ResourceDictionary)reader.LoadAsync(stream);
                    Application.Current.Resources.MergedDictionaries.Add(res);
                }
            } catch (Exception err)
            {
                Debugger.Error(err);
            }
        }

        private bool PatchThemeAndValidate(string path)
        {
            string theme = File.ReadAllText(path);

            if (theme.Contains(";assembly=Hunterpie\""))
            {
                try
                {

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(theme);

                    foreach (XmlAttribute e in xml.DocumentElement.Attributes)
                    {
                        if (!e.Value.Contains(";assembly=HunterPie.UI"))
                        {
                            e.Value = e.Value.Replace(";assembly=Hunterpie", ";assembly=HunterPie.UI");
                        }
                    }

                    xml.Save(path);
                    Debugger.Warn($"Patched theme");

                    return true;
                } catch (Exception err)
                {
                    Debugger.Error(err);
                    return false;
                }
            } else
            {
                return true;
            }
        }

        private void ExceptionLogger(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("crashes.txt", e.ExceptionObject.ToString());

            if (config.HunterPie.Debug.SendCrashFileToDev)
            {
                const string HunterPieCrashesWebhook = "https://discordapp.com/api/webhooks/756301992930050129/sTbp4PmjYZMlGGT0IYIhYtTiVw9hpaqwjo-n1Aawl2omWfnV-SD3NpH691xm4TleJ2p-";
                // Also try to send the crash error to my webhook so I can fix it
                using (var httpClient = new HttpClient())
                {
                    using (var req = new HttpRequestMessage(new HttpMethod("POST"), HunterPieCrashesWebhook))
                    {
                        using (var content = new MultipartFormDataContent())
                        {
                            content.Add(new StringContent(""), "username");
                            content.Add(new StringContent($"```Exception type: {e.ExceptionObject.GetType()}\n-----------------------------------\nBranch: {config.HunterPie.Update.Branch}\nVersion: {FileVersionInfo.GetVersionInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.exe")).FileVersion}\nGAME BUILD VERSION: {Game.Version}\nHunterPie elapsed time: {DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()}```"), "content");
                            content.Add(new StringContent(e.ExceptionObject.ToString()), "file", "crashes.txt");
                            req.Content = content;

                            httpClient.SendAsync(req).Wait();
                        }

                    }
                }
            }
            DebuggerControl.WriteStacktrace();
        }

        private void StartEverything()
        {
            SetAnimationsFramerate();
            HookEvents();
            Kernel.StartScanning(); // Scans game memory

        }

        private void SetAnimationsFramerate() => Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = Math.Min(60, Math.Max(1, config.Overlay.DesiredAnimationFrameRate)) });

        #region Game & Client Events
        private void HookEvents()
        {
            // Kernel events
            Kernel.OnGameStart += OnGameStart;
            Kernel.OnGameClosed += OnGameClose;
            // Settings
            ConfigManager.OnSettingsUpdate += SendToOverlay;
        }

        private void UnhookEvents()
        {
            // Debug
            AppDomain.CurrentDomain.UnhandledException -= ExceptionLogger;
            // Kernel events
            Kernel.OnGameStart -= OnGameStart;
            Kernel.OnGameClosed -= OnGameClose;
            // Settings
            ConfigManager.OnSettingsUpdate -= SendToOverlay;
        }

        public void SendToOverlay(object source, EventArgs e)
        {
            Debugger.IsDebugEnabled = config.HunterPie.Debug.ShowDebugMessages;
            overlay?.GlobalSettingsEventHandler(source, e);
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                // Only shows notification if HunterPie is visible
                if (IsVisible)
                {
                    CNotification notification = new CNotification()
                    {
                        Text = GStrings.GetLocalizationByXPath("/Notifications/String[@ID='STATIC_SETTINGS_LOAD']"),
                        FirstButtonVisibility = Visibility.Collapsed,
                        SecondButtonVisibility = Visibility.Collapsed,
                        NIcon = FindResource("ICON_CHECKED") as ImageSource,
                        ShowTime = 11
                    };
                    NotificationsPanel.Children.Add(notification);
                }
                SettingsControl.RefreshSettingsUI();
            }));
        }

        private void HookGameEvents()
        {
            // Game events
            game.Player.OnZoneChange += OnZoneChange;
            game.Player.OnCharacterLogin += OnLogin;
            game.Player.OnCharacterLogout += OnLogout;
            game.Player.OnSessionChange += OnSessionChange;
            game.Player.OnClassChange += OnClassChange;
        }

        private void UnhookGameEvents()
        {
            game.Player.OnZoneChange -= OnZoneChange;
            game.Player.OnCharacterLogin -= OnLogin;
            game.Player.OnCharacterLogout -= OnLogout;
            game.Player.OnSessionChange -= OnSessionChange;
            game.Player.OnClassChange -= OnClassChange;
        }

        private void ExportGameData()
        {
            if (game.Player.ZoneID != 0)
            {
                string sSession = game.Player.SteamID != 0 ? $"steam://joinlobby/582010/{game.Player.SteamSession}/{game.Player.SteamID}" : "";
                Data playerData = new Data
                {
                    Name = game.Player.Name,
                    HR = game.Player.Level,
                    MR = game.Player.MasterRank,
                    BuildURL = Honey.LinkStructureBuilder(game.Player.GetPlayerGear()),
                    Session = game.Player.SessionID,
                    SteamSession = sSession,
                    Playtime = game.Player.PlayTime,
                    WeaponName = game.Player.WeaponName
                };
                dataExporter.ExportData(playerData);
            }
        }

        private void OnSessionChange(object source, EventArgs args)
        {
            PlayerEventArgs e = (PlayerEventArgs)args;
            Debugger.Log($"SESSION: {e.SessionId}");
            // Writes the session ID to a Sessions.txt
            if (!string.IsNullOrEmpty(e.SessionId) && game.Player.IsLoggedOn)
            {
                ExportGameData();
                // Because some people don't give permissions to write to files zzzzz
                try
                {
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sessions.txt"), e.SessionId);
                } catch
                {
                    Debugger.Error("Missing permissions to write to files.");
                }
            }
        }

        public void OnZoneChange(object source, EventArgs e)
        {
            if (game.Player.IsLoggedOn)
            {
                Debugger.Debug($"ZoneID: {game.Player.ZoneID}");
                ExportGameData();
            }
        }

        public async void OnLogin(object source, EventArgs e)
        {
            Debugger.Log($"Logged on {game.Player.Name}");

            await Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                IsPlayerLoggedOn = true;
            }));

            await Chat.SystemMessage("<STYL MOJI_LIGHTBLUE_DEFAULT><ICON SLG_NEWS>HunterPie Native</STYL>\nConnected.", -1, 0, 0);

            ExportGameData();
        }

        public void OnLogout(object source, EventArgs e) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            IsPlayerLoggedOn = false;
        }));

        private void OnClassChange(object source, EventArgs args) => ExportGameData();


        private async void OnGameStart(object source, EventArgs e)
        {
            // Set HunterPie hotkeys
            SetHotKeys();

            // Create game instances
            game.CreateInstances();

            // Loads memory map
            if (Address.LoadMemoryMap(Kernel.GameVersion) || Kernel.GameVersion == Address.GAME_VERSION)
            {
                Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_MAP_LOAD']").Replace("{HunterPie_Map}", $"'MonsterHunterWorld.{Kernel.GameVersion}.map'"));
            }
            else
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_GAME_VERSION_UNSUPPORTED']").Replace("{GAME_VERSION}", $"{Kernel.GameVersion}"));
                return;
            }

            Honey.Load();

            await Task.Run(() =>
            {
                GMD.InitializeGMDs();
                MusicSkillData.Load();
            });

            // Hook game events
            HookGameEvents();

            // Set game context and load the modules
            PluginManager.ctx = game;
            await pluginManager.LoadPlugins();

            await InitializeNative();

            // Creates new overlay
            Dispatcher.Invoke(() =>
            {
                if (overlay is null)
                {
                    overlay = new Overlay(game);
                    overlay.HookEvents();
                    ConfigManager.TriggerSettingsEvent();
                }
            });

            // Initializes rich presence
            if (presence is null)
            {
                presence = new Presence(game);
                if (offlineMode)
                    presence.SetOfflineMode();

                presence.Initialize();
            }

            // Starts scanning
            game.StartScanning();

        }

        private async Task InitializeNative()
        {
            if (Injector.CheckIfAlreadyInjected())
            {
                await Client.Initialize();
            } else
            {
                if (Injector.InjectNative())
                {
                    Debugger.Write("Injected HunterPie.Native.dll succesfully!", "#FFC88DF2");
                    await Client.Initialize();
                }
                else
                {
                    if (ConfigManager.Settings.HunterPie.EnableNativeFunctions)
                        Debugger.Error("Failed to inject HunterPie.Native.dll");
                    else
                    {
                        Debugger.Warn("Skipped HunterPie.Native.dll load");
                    }
                    return;
                }
            }
            await Dispatcher.InvokeAsync(() =>
            {
                if (!Injector.CheckIfCRCBypassExists())
                {
                    CNotification notification = new CNotification()
                    {
                        Text = "Some of HunterPie Native features require CRCBypass",
                        NIcon = FindResource("ICON_WARN") as ImageSource,
                        FirstButtonImage = FindResource("ICON_DOWNLOAD") as ImageSource,
                        FirstButtonText = "Stracker's Loader",
                        FirstButtonVisibility = Visibility.Visible,
                        SecondButtonImage = FindResource("ICON_DOWNLOAD") as ImageSource,
                        SecondButtonText = "CRCBypass",
                        SecondButtonVisibility = Visibility.Visible,
                        Callback1 = new Action(() =>
                        {
                            Process.Start("https://www.nexusmods.com/monsterhunterworld/mods/1982");
                        }),
                        Callback2 = new Action(() =>
                        {
                            Process.Start("https://www.nexusmods.com/monsterhunterworld/mods/3473");
                        }),
                        ShowTime = 20
                    };
                    NotificationsPanel.Children.Add(notification);
                }
            });
        }

        private async void OnGameClose(object source, EventArgs e)
        {
            Client.Instance.Dispose();
            // Remove global hotkeys
            RemoveHotKeys();

            UnhookGameEvents();
            pluginManager?.UnloadPlugins();
            presence?.Dispose();
            presence = null;

            // For some reason it crashes here if we stop scanning when the game is closed before it is initialized?
            try { game?.StopScanning(); }
            catch {}

            await Dispatcher.InvokeAsync(async () =>
            {
                if (overlay is null)
                    return;

                await overlay.Dispose().ContinueWith(task => game?.DestroyInstances());
                overlay = null;
            });

            if (config.HunterPie.Options.CloseWhenGameCloses)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Close();
                }));
            }
        }

        #endregion

        #region Animations
        private void SwitchButtonOn(StackPanel buttonActive)
        {
            Border ButtonBorder = (Border)buttonActive.Children[0];
            ButtonBorder.SetValue(BorderThicknessProperty, new Thickness(4, 0, 0, 0));
        }

        private void SwitchButtonOff(StackPanel buttonActive)
        {
            Border ButtonBorder = (Border)buttonActive.Children[0];
            ButtonBorder.SetValue(BorderThicknessProperty, new Thickness(0, 0, 0, 0));
        }
        #endregion

        #region WINDOW EVENTS

        private async void OnWindowInitialized(object sender, EventArgs e)
        {
            Hide();
            await ConfigManager.Initialize();

            // Initialize localization
            GStrings.InitStrings(config.HunterPie.Language, App.Current);

            // Load custom theme and console colors
            LoadCustomTheme();
            LoadOverwriteTheme();
            DebuggerControl.LoadNewColors();
            AdministratorIconVisibility = IsRunningAsAdmin() ? Visibility.Visible : Visibility.Collapsed;

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            WindowBlur.SetIsEnabled(this, true);

            Width = config.HunterPie.Width;
            Height = config.HunterPie.Height;
            Top = config.HunterPie.PosY;
            Left = config.HunterPie.PosX;

            ChangeConsoleChild(ConsoleBtn, DebuggerControl.Instance);

            // Initialize everything under this line
            if (!await CheckIfUpdateEnableAndStart())
                return;

            // Convert the old HotKey to the new one
            ConvertOldHotkeyToNew(config.Overlay.ToggleDesignModeKey);

            isUpdating = false;
            InitializeTrayIcon();
            // Update version text
            Version = GStrings.GetLocalizationByXPath("/Console/String[@ID='CONSOLE_VERSION']").Replace("{HunterPie_Version}", HUNTERPIE_VERSION).Replace("{HunterPie_Branch}", config.HunterPie.Update.Branch);

            if (config.HunterPie.StartHunterPieMinimized)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
            else
            {
                Show();
            }

            // Initializes the Hotkey API
            Hotkey.Load();

            // Initializes the rest of HunterPie
            LoadData();
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_HUNTERPIE_INITIALIZED']"));

            await pluginManager.PreloadPlugins();
            // Support message :)
            //ShowSupportMessage();

            StartEverything();
        }

        private async void OnCloseWindowButtonClick(object sender, MouseButtonEventArgs e)
        {
            // X button function;
            bool exitConfirmation = MessageBox.Show(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_QUIT']"), "HunterPie", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            config.HunterPie.PosX = Left;
            config.HunterPie.PosY = Top;

            await ConfigManager.TrySaveSettingsAsync();

            if (exitConfirmation)
                Close();
        }

        private void OnWindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Point point = PointToScreen(e.MouseDevice.GetPosition(this));

                if (point.X <= RestoreBounds.Width / 2) Left = 0;
                else if (point.X >= RestoreBounds.Width) Left = point.X - (RestoreBounds.Width - (ActualWidth - point.X));
                else { Left = point.X - (RestoreBounds.Width / 2); }

                Top = point.Y - (((FrameworkElement)sender).ActualWidth / 2);
                WindowState = WindowState.Normal;
            }
            // When top bar is held by LMB
            DragMove();
        }

        private void OnMinimizeButtonClick(object sender, MouseButtonEventArgs e)
        {
            MinimizeWindow();
        }

        private void MinimizeWindow()
        {
            if (config.HunterPie.MinimizeToSystemTray)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
            else
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Hotkey.Unload();

            Hide();
            Client.Instance.Disconnect();

            DebuggerControl.WriteStacktrace();
            pluginManager?.UnloadPlugins();

            DebuggerControl.DumpLog();

            // Dispose tray icon
            trayIcon?.Dispose();

            // Dispose stuff & stop scanning threads
            overlay?.Dispose();

            if (game.IsActive)
                game?.StopScanning();

            presence?.Dispose();

            Kernel.StopScanning();

            SettingsControl.Instance?.UninstallKeyboardHook();

            // Unhook events
            if (game.Player != null)
                UnhookGameEvents();

            UnhookEvents();
        }

        private void OnLaunchGameButtonClick(object sender, RoutedEventArgs e) => LaunchGame();

        private void LaunchGame()
        {
            try
            {
                ProcessStartInfo GameStartInfo = new ProcessStartInfo
                {
                    FileName = "steam://run/582010",
                    Arguments = config.HunterPie.Launch.LaunchArgs,
                    UseShellExecute = true
                };
                Process.Start(GameStartInfo);

                if (config.HunterPie.MinimizeAfterGameStart)
                {
                    MinimizeWindow();
                }
            }
            catch (Exception err)
            {
                Debugger.Error($"{GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_LAUNCH_ERROR']")}\n{err}");
            }
        }

        private void OnWindowSizeChange(object sender, SizeChangedEventArgs e)
        {
            if (config is null)
                return;
            config.HunterPie.Width = (float)e.NewSize.Width;
            config.HunterPie.Height = (float)e.NewSize.Height;
        }

        public void Reload()
        {
            // Welp
            Process.Start(Application.ResourceAssembly.Location, "latestVersion=True");
            Application.Current.Shutdown();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.R)
            {
                Reload();
            }
        }
        #endregion

        #region Helpers

        public static Version ParseVersion(string version)
        {
            return new Version(version);
        }

        private void ShowSupportMessage()
        {
            CNotification notification = new CNotification()
            {
                Text = "Do you like HunterPie and want to support its development? Consider donating!",
                NIcon = FindResource("LOGO_HunterPie") as ImageSource,
                FirstButtonImage = FindResource("ICON_PAYPAL") as ImageSource,
                FirstButtonText = "PayPal",
                FirstButtonVisibility = Visibility.Visible,
                SecondButtonImage = FindResource("ICON_PATREON") as ImageSource,
                SecondButtonText = "Patreon",
                SecondButtonVisibility = Visibility.Visible,
                Callback1 = new Action(() =>
                {
                    Process.Start("https://server.haato.dev/donate");
                }),
                Callback2 = new Action(() =>
                {
                    Process.Start("https://www.patreon.com/HunterPie");
                }),
                ShowTime = 20
            };
            NotificationsPanel.Children.Add(notification);
        }

        #endregion

        public async Task<string> InstallPlugin(string moduleContent)
        {
            PluginInformation moduleInformation = JsonConvert.DeserializeObject<PluginInformation>(moduleContent);


            if (moduleInformation is null || string.IsNullOrEmpty(moduleInformation?.Name))
            {
                Debugger.Log("Invalid module.json!");
                return null;
            }

            string modPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", moduleInformation.Name);

            if (!Directory.Exists(modPath))
            {
                Directory.CreateDirectory(modPath);
            }

            File.WriteAllText(Path.Combine(modPath, "module.json"), JsonConvert.SerializeObject(moduleInformation, Newtonsoft.Json.Formatting.Indented));


            if (PluginUpdate.PluginSupportsUpdate(moduleInformation))
            {
                switch (await PluginUpdate.UpdateAllFiles(moduleInformation, modPath))
                {
                    case UpdateResult.Updated:
                        Debugger.Module($"Installed {moduleInformation.Name}! (ver {moduleInformation.Version})");

                        CNotification notification = new CNotification
                        {
                            Text = GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_INSTALLED']").Replace("{name}", moduleInformation.Name),
                            NIcon = FindResource("ICON_PLUGIN") as ImageSource,

                            FirstButtonVisibility = Visibility.Visible,
                            Callback1 = Reload,
                            FirstButtonImage = FindResource("ICON_RESTART") as ImageSource,
                            FirstButtonText = GStrings.GetLocalizationByXPath("/Notifications/String[@ID='STATIC_RESTART']"),

                            SecondButtonVisibility = Visibility.Collapsed,
                            ShowTime = 15
                        };
                        NotificationsPanel.Children.Add(notification);
                        break;

                    case UpdateResult.Failed:
                        Debugger.Error($"Failed to install {moduleInformation.Name}.");
                        return null;

                    case UpdateResult.UpToDate:
                        string uptodateText =
                            $"Plugin {moduleInformation.Name} is already installed and up-to-date! (ver {moduleInformation.Version})";
                        Debugger.Module(uptodateText);
                        CNotification uptodateNotification = new CNotification
                        {
                            Text = uptodateText,
                            NIcon = FindResource("ICON_PLUGIN") as ImageSource,

                            FirstButtonVisibility = Visibility.Collapsed,
                            SecondButtonVisibility = Visibility.Collapsed,
                            ShowTime = 5
                        };
                        NotificationsPanel.Children.Add(uptodateNotification);
                        return modPath;
                }
            }
            else
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='ERROR_PLUGIN_AUTO_UPDATE']"));
                return null;
            }

            return modPath;
        }

        private async void window_Drop(object sender, DragEventArgs e)
        {
            IsDragging = false;
            string modulejson = ((string[])e.Data.GetData("FileName"))?.FirstOrDefault();
            string moduleContent;
            bool isOnline = false;
            if (modulejson is null)
            {
                isOnline = true;
                modulejson = e.Data.GetData("UnicodeText") as string;
            }

            if (!modulejson.ToLower().EndsWith("module.json"))
            {
                return;
            }

            if (isOnline)
            {
                if (!modulejson.StartsWith("http"))
                {
                    modulejson = $"https://{modulejson}";
                }
                moduleContent = await PluginUpdate.ReadOnlineModuleJson(modulejson);
            } else
            {
                moduleContent = File.ReadAllText(modulejson);
            }

            await InstallPlugin(moduleContent);
        }

        private void window_DragEnter(object sender, DragEventArgs e)
        {
            IsDragging = true;
        }

        private void window_DragLeave(object sender, DragEventArgs e)
        {
            IsDragging = false;
        }

        private void OnDebuggerButtonClick(object sender, MouseButtonEventArgs e)
        {
            ChangeConsoleChild(sender, DebuggerControl.Instance);
        }

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e)
        {
            SettingsControl.Instantiate(PluginManager.PluginSettingsTabs);

            ChangeConsoleChild(sender, SettingsControl.Instance);
            SettingsControl.RefreshSettingsUI();
        }

        private void OnPluginsButtonClick(object sender, MouseButtonEventArgs e)
        {
            ChangeConsoleChild(sender, PluginDisplay.Instance);
        }

        public void OpenSettingsForOwner(string name)
        {
            SettingsControl.Instantiate(PluginManager.PluginSettingsTabs);

            ChangeConsoleChild(SettingsBtn, SettingsControl.Instance);
            SettingsControl.Instance.OpenSettingsForOwner(name);
        }

        private void ChangeConsoleChild(object sender, UIElement newChild)
        {
            // Ignore if the current selected tab is already open
            if (ConsolePanel.Children.Contains(newChild))
                return;

            // if sender is button, depress all other buttons
            if (sender is SideButton {Parent: StackPanel panel} snd)
            {
                UnclickButtons(panel, snd);
            }

            // notify previously activated views if any
            foreach (UIElement child in ConsolePanel.Children)
            {
                if (child is IActivatable activatable)
                {
                    activatable.OnDeactivate();
                }
            }

            // replace existing view with new one
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(newChild);

            // notify new view about activation
            if (newChild is IActivatable activatableChild)
            {
                activatableChild.OnActivate();
            }
        }

        private void OnBuildUploadButtonClick(object sender, MouseButtonEventArgs e)
        {
            CNotification notification = new CNotification()
            {
                Text = GStrings.GetLocalizationByXPath("/Notifications/String[@ID='MESSAGE_BUILD_UPLOADED']"),
                NIcon = FindResource("ICON_BUILD") as ImageSource,
                FirstButtonImage = FindResource("ICON_COPY") as ImageSource,
                FirstButtonText = GStrings.GetLocalizationByXPath("/Settings/String[@ID='STATIC_COPYCLIPBOARD']"),
                FirstButtonVisibility = Visibility.Visible,
                SecondButtonImage = FindResource("ICON_LINK") as ImageSource,
                SecondButtonText = GStrings.GetLocalizationByXPath("/Notifications/String[@ID='STATIC_OPENDEFAULTBROWSER']"),
                SecondButtonVisibility = Visibility.Visible,
                Callback1 = new Action(() =>
                {
                    string buildLink = Honey.LinkStructureBuilder(game.Player.GetPlayerGear(), true);
                    Clipboard.SetData(DataFormats.Text, buildLink);
                }),
                Callback2 = new Action(() =>
                {
                    string buildLink = Honey.LinkStructureBuilder(game.Player.GetPlayerGear(), true);
                    Process.Start(buildLink);
                }),
                ShowTime = 11
            };
            NotificationsPanel.Children.Add(notification);
        }

        private async void OnExportGearButtonClick(object sender, MouseButtonEventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {

                sItem[] decoration = game.Player.GetDecorationsFromStorage();
                sGear[] gears = game.Player.GetGearFromStorage();

                string exported = Honey.ExportDecorationsToHoney(decoration, gears);

                if (dataExporter.ExportCustomData("Decorations-HoneyHuntersWorld.txt", exported))
                {
                    Debugger.Warn("Exported decorations to ./DataExport/Decorations-HoneyHuntersWorld.txt!");
                }
                else
                {
                    Debugger.Error("Failed to export decorations. Make sure HunterPie has permission to create/write to files.");
                }

                exported = Honey.ExportCharmsToHoney(gears);
                if (dataExporter.ExportCustomData("Charms-HoneyHuntersWorld.txt", exported))
                {
                    Debugger.Warn("Exported charms to ./DataExport/Charms-HoneyHuntersWorld.txt!");
                }
                else
                {
                    Debugger.Error("Failed to export charms. Make sure HunterPie has permission to create/write to files.");
                }
                Dispatcher.Invoke(() =>
                {
                    CNotification notification = new CNotification()
                    {
                        NIcon = FindResource("ICON_DECORATION") as ImageSource,
                        Text = GStrings.GetLocalizationByXPath("/Notifications/String[@ID='MESSAGE_GEAR_EXPORTED']"),
                        FirstButtonImage = FindResource("ICON_LINK") as ImageSource,
                        FirstButtonText = GStrings.GetLocalizationByXPath("/Notifications/String[@ID='STATIC_OPENFOLDER']"),
                        FirstButtonVisibility = Visibility.Visible,
                        SecondButtonVisibility = Visibility.Collapsed,
                        ShowTime = 11,
                        Callback1 = new Action(() =>
                        {
                            Process.Start(dataExporter.ExportPath);
                        })
                    };
                    NotificationsPanel.Children.Add(notification);
                });
            });
        }

        private void OnChangelogButtonClick(object sender, MouseButtonEventArgs e)
        {
            ChangeConsoleChild(sender, Changelog.Instance);
        }

        private void OnDiscordButtonClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://discord.gg/5pdDq4Q");
        }

        private void OnGithubButtonClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/Haato3o/HunterPie");
        }

        private static void UnclickButtons(StackPanel parent, SideButton exception = null)
        {
            foreach (SideButton btn in parent.Children.Cast<SideButton>())
            {
                btn.IsClicked = btn == exception;
            }
        }
    }
}
