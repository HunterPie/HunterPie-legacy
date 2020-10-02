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
using HunterPie.GUI;
using HunterPie.GUIControls;
using HunterPie.GUIControls.Custom_Controls;
using HunterPie.Plugins;
using HunterPie.Logger;
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

namespace HunterPie
{
    /// <summary>
    /// HunterPie main window logic;
    /// </summary>
    public partial class Hunterpie : Window
    {
        // TODO: Refactor all this messy code

        // Classes
        TrayIcon TrayIcon;
        readonly Game MonsterHunter = new Game();
        Presence Discord;
        Overlay GameOverlay;
        readonly Exporter dataExporter = new Exporter();
        PluginManager pluginManager = new PluginManager();
        bool OfflineMode = false;
        bool IsUpdating = true;

        // HunterPie version
        public const string HUNTERPIE_VERSION = "1.0.3.97";

        private readonly List<int> registeredHotkeys = new List<int>();

        // Helpers

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

        public Hunterpie()
        {
            if (CheckIfItsRunningFromWinrar())
            {
                MessageBox.Show("You must extract HunterPie files before running it, otherwise it will most likely crash due to missing files or not save your settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }
            CheckIfHunterPieOpen();

            AppDomain.CurrentDomain.UnhandledException += ExceptionLogger;
            
            IsPlayerLoggedOn = false;
            
            SetDPIAwareness();
            
            Buffers.Initialize(1024);
            Buffers.Add<byte>(64);

            // Initialize debugger and player config
            Debugger.InitializeDebugger();
            UserSettings.InitializePlayerConfig();

            // Initialize localization
            GStrings.InitStrings(UserSettings.PlayerConfig.HunterPie.Language);

            // Load custom theme and console colors
            LoadCustomTheme();
            LoadOverwriteTheme();
            Debugger.LoadNewColors();
            AdministratorIconVisibility = IsRunningAsAdmin() ? Visibility.Visible : Visibility.Collapsed;

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            InitializeComponent();
            WindowBlur.SetIsEnabled(this, true);
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
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe"))) return false;

            Process UpdateProcess = new Process();
            UpdateProcess.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update.exe");
            UpdateProcess.StartInfo.Arguments = $"version={HUNTERPIE_VERSION} branch={UserSettings.PlayerConfig.HunterPie.Update.Branch}";
            UpdateProcess.Start();
            return true;
        }

        private bool CheckIfUpdateEnableAndStart()
        {
            if (UserSettings.PlayerConfig.HunterPie.Update.Enabled)
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
                        OfflineMode = ParseArgs(argument) == "True";
                    }
                }
                if (justUpdated)
                {
                    OpenChangelog();
                    return true;
                }
                if (latestVersion)
                {
                    return true;
                }
                Debugger.Log("Updating updater.exe");
                // This will update Update.exe
                AutoUpdate au = new AutoUpdate(UserSettings.PlayerConfig.HunterPie.Update.Branch);
                au.Instance.DownloadFileCompleted += OnUpdaterDownloadComplete;
                OfflineMode = au.offlineMode;
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

        private void OnUpdaterDownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_UPDATE_ERROR']"));
                Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_OFFLINEMODE_WARN']"));
                OfflineMode = true;
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
                UserSettings.PlayerConfig.Overlay.ToggleOverlayKeybind,
                UserSettings.PlayerConfig.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey,
                UserSettings.PlayerConfig.Overlay.ToggleDesignKeybind
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
                if (hotkey == "None")
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

        private void ToggleOverlayCallback()
        {
            if (GameOverlay == null)
            {
                return;
            }

            UserSettings.PlayerConfig.Overlay.Enabled = !UserSettings.PlayerConfig.Overlay.Enabled;
            UserSettings.SaveNewConfig();
        }

        private void SwitchMonsterBarModeCallback()
        {
            if (GameOverlay == null)
            {
                return;
            }

            UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode = UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode + 1 >= 5 ? (byte)0 : (byte)(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode + 1);
            UserSettings.SaveNewConfig();
        }

        private void ToggleDesignModeCallback()
        {
            GameOverlay?.ToggleDesignMode();
        }

        private void ConvertOldHotkeyToNew(int Key)
        {
            if (Key == 0) return;
            UserSettings.PlayerConfig.Overlay.ToggleDesignKeybind = KeyboardHookHelper.GetKeyboardKeyByID(Key).ToString();
            UserSettings.PlayerConfig.Overlay.ToggleDesignModeKey = 0;
        }

        #endregion

        #region TRAY ICON
        private void InitializeTrayIcon()
        {
            TrayIcon = new TrayIcon();
            // Tray icon itself
            TrayIcon.NotifyIcon.BalloonTipTitle = "HunterPie";
            TrayIcon.NotifyIcon.Text = "HunterPie";
            TrayIcon.NotifyIcon.Icon = Properties.Resources.LOGO_HunterPie;
            TrayIcon.NotifyIcon.Visible = true;
            TrayIcon.NotifyIcon.MouseDoubleClick += OnTrayIconClick;

            // Menu items
            System.Windows.Forms.MenuItem ExitItem = new System.Windows.Forms.MenuItem()
            {
                Text = "Close"
            };
            ExitItem.Click += OnTrayIconExitClick;
            System.Windows.Forms.MenuItem SettingsItem = new System.Windows.Forms.MenuItem()
            {
                Text = "Settings"
            };
            SettingsItem.Click += OnTrayIconSettingsClick;

            TrayIcon.ContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { SettingsItem, ExitItem });
        }

        private void OnTrayIconSettingsClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();
            OpenSettings();
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
            return path.Contains("Temp\\Rar");
        }

        private void LoadCustomTheme()
        {
            if (UserSettings.PlayerConfig.HunterPie.Theme == null || UserSettings.PlayerConfig.HunterPie.Theme == "Default") return;
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes"))) { Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes")); }
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Themes/{UserSettings.PlayerConfig.HunterPie.Theme}")))
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_THEME_NOT_FOUND_ERROR']".Replace("{THEME_NAME}", UserSettings.PlayerConfig.HunterPie.Theme)));
                return;
            }
            try
            {
                using (FileStream stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Themes/{UserSettings.PlayerConfig.HunterPie.Theme}"), FileMode.Open))
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
                using (FileStream stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"HunterPie.Resources/UI/Overwrite.xaml"), FileMode.Open))
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

        private void ExceptionLogger(object sender, UnhandledExceptionEventArgs e)
        {
            File.WriteAllText("crashes.txt", e.ExceptionObject.ToString());

            if (UserSettings.PlayerConfig.HunterPie.Debug.SendCrashFileToDev)
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
                            content.Add(new StringContent($"```Exception type: {e.ExceptionObject.GetType()}\n-----------------------------------\nBranch: {UserSettings.PlayerConfig.HunterPie.Update.Branch}\nVersion: {FileVersionInfo.GetVersionInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HunterPie.exe")).FileVersion}\nGAME BUILD VERSION: {Game.Version}\nHunterPie elapsed time: {DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()}```"), "content");
                            content.Add(new StringContent(e.ExceptionObject.ToString()), "file", "crashes.txt");
                            req.Content = content;

                            httpClient.SendAsync(req).Wait();
                        }

                    }
                }
            }
        }

        private void StartEverything()
        {
            SetAnimationsFramerate();
            HookEvents();
            Kernel.StartScanning(); // Scans game memory
            if (UserSettings.PlayerConfig.HunterPie.StartHunterPieMinimized)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void SetAnimationsFramerate() => Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = Math.Min(60, Math.Max(1, UserSettings.PlayerConfig.Overlay.DesiredAnimationFrameRate)) });

        #region Game & Client Events
        private void HookEvents()
        {
            // Kernel events
            Kernel.OnGameStart += OnGameStart;
            Kernel.OnGameClosed += OnGameClose;
            // Settings
            UserSettings.OnSettingsUpdate += SendToOverlay;
        }

        private void UnhookEvents()
        {
            // Debug
            AppDomain.CurrentDomain.UnhandledException -= ExceptionLogger;
            // Kernel events
            Kernel.OnGameStart -= OnGameStart;
            Kernel.OnGameClosed -= OnGameClose;
            // Settings
            UserSettings.OnSettingsUpdate -= SendToOverlay;
        }

        public void SendToOverlay(object source, EventArgs e)
        {
            GameOverlay?.GlobalSettingsEventHandler(source, e);
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
                    notification.ShowNotification();
                }
                Settings.RefreshSettingsUI();
            }));
        }

        private void HookGameEvents()
        {
            // Game events
            MonsterHunter.Player.OnZoneChange += OnZoneChange;
            MonsterHunter.Player.OnCharacterLogin += OnLogin;
            MonsterHunter.Player.OnCharacterLogout += OnLogout;
            MonsterHunter.Player.OnSessionChange += OnSessionChange;
            MonsterHunter.Player.OnClassChange += OnClassChange;
        }

        private void UnhookGameEvents()
        {
            MonsterHunter.Player.OnZoneChange -= OnZoneChange;
            MonsterHunter.Player.OnCharacterLogin -= OnLogin;
            MonsterHunter.Player.OnCharacterLogout -= OnLogout;
            MonsterHunter.Player.OnSessionChange -= OnSessionChange;
            MonsterHunter.Player.OnClassChange -= OnClassChange;
        }

        private void ExportGameData()
        {
            if (MonsterHunter.Player.ZoneID != 0)
            {
                string sSession = MonsterHunter.Player.SteamID != 0 ? $"steam://joinlobby/582010/{MonsterHunter.Player.SteamSession}/{MonsterHunter.Player.SteamID}" : "";
                Data playerData = new Data
                {
                    Name = MonsterHunter.Player.Name,
                    HR = MonsterHunter.Player.Level,
                    MR = MonsterHunter.Player.MasterRank,
                    BuildURL = Honey.LinkStructureBuilder(MonsterHunter.Player.GetPlayerGear()),
                    Session = MonsterHunter.Player.SessionID,
                    SteamSession = sSession,
                    Playtime = MonsterHunter.Player.PlayTime,
                    WeaponName = MonsterHunter.Player.WeaponName
                };
                dataExporter.ExportData(playerData);
            }
        }

        private void OnSessionChange(object source, EventArgs args)
        {
            Debugger.Log($"SESSION: {MonsterHunter.Player.SessionID}");
            // Writes the session ID to a Sessions.txt
            if (!string.IsNullOrEmpty(MonsterHunter.Player.SessionID) && MonsterHunter.Player.IsLoggedOn)
            {
                ExportGameData();
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sessions.txt"), MonsterHunter.Player.SessionID);
            }
        }

        public void OnZoneChange(object source, EventArgs e)
        {
            if (MonsterHunter.Player.IsLoggedOn)
            {
                Debugger.Debug($"ZoneID: {MonsterHunter.Player.ZoneID}");
                ExportGameData();
            }
        }

        public void OnLogin(object source, EventArgs e)
        {
            Debugger.Log($"Logged on {MonsterHunter.Player.Name}");
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                IsPlayerLoggedOn = true;
            }));
            ExportGameData();
        }

        public void OnLogout(object source, EventArgs e) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            IsPlayerLoggedOn = false;
        }));

        private void OnClassChange(object source, EventArgs args) => ExportGameData();


        public void OnGameStart(object source, EventArgs e)
        {
            // Set HunterPie hotkeys
            SetHotKeys();

            // Create game instances
            MonsterHunter.CreateInstances();

            // Hook game events
            HookGameEvents();

            // Set game context and load the modules
            PluginManager.ctx = MonsterHunter;
            if (pluginManager.IsReady)
            {
                pluginManager.LoadPlugins();
            }
            else
            {
                pluginManager.QueueLoad = true;
            }

            // Creates new overlay
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                if (GameOverlay == null)
                {
                    GameOverlay = new Overlay(MonsterHunter);
                    GameOverlay.HookEvents();
                    UserSettings.TriggerSettingsEvent();
                }
            }));

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

            // Starts scanning
            MonsterHunter.StartScanning();

            // Initializes rich presence
            if (Discord is null)
            {
                Discord = new Presence(MonsterHunter);
                if (OfflineMode) Discord.SetOfflineMode();
                Discord.StartRPC();
            }

        }

        public void OnGameClose(object source, EventArgs e)
        {
            // Remove global hotkeys
            RemoveHotKeys();

            UnhookGameEvents();
            pluginManager?.UnloadPlugins();
            Discord?.Dispose();
            Discord = null;

            MonsterHunter?.StopScanning();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                GameOverlay?.Dispose();
                GameOverlay = null;
            }));
            MonsterHunter?.DestroyInstances();
            if (UserSettings.PlayerConfig.HunterPie.Options.CloseWhenGameCloses)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Close();
                }));
            }
        }

        #endregion

        #region Sub Windows
        /* Open sub windows */

        private void OpenDebugger()
        {
            SwitchButtonOn(BUTTON_CONSOLE);
            SwitchButtonOff(BUTTON_CHANGELOG);
            SwitchButtonOff(BUTTON_SETTINGS);
            SwitchButtonOff(BUTTON_PLUGINS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Debugger.Instance);
        }

        private void OpenSettings()
        {
            SwitchButtonOff(BUTTON_CONSOLE);
            SwitchButtonOff(BUTTON_CHANGELOG);
            SwitchButtonOn(BUTTON_SETTINGS);
            SwitchButtonOff(BUTTON_PLUGINS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Settings.Instance);
            Settings.RefreshSettingsUI();
        }

        private void OpenPlugins()
        {
            SwitchButtonOff(BUTTON_CONSOLE);
            SwitchButtonOff(BUTTON_CHANGELOG);
            SwitchButtonOff(BUTTON_SETTINGS);
            SwitchButtonOn(BUTTON_PLUGINS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(PluginDisplay.Instance);
        }

        private void OpenChangelog()
        {
            SwitchButtonOff(BUTTON_CONSOLE);
            SwitchButtonOn(BUTTON_CHANGELOG);
            SwitchButtonOff(BUTTON_SETTINGS);
            SwitchButtonOff(BUTTON_PLUGINS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Changelog.Instance);
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

        private void OnWindowInitialized(object sender, EventArgs e)
        {
            Hide();
            Width = UserSettings.PlayerConfig.HunterPie.Width;
            Height = UserSettings.PlayerConfig.HunterPie.Height;
            Top = UserSettings.PlayerConfig.HunterPie.PosY;
            Left = UserSettings.PlayerConfig.HunterPie.PosX;

            OpenDebugger();
            // Initialize everything under this line
            if (!CheckIfUpdateEnableAndStart()) return;

            // Convert the old HotKey to the new one
            ConvertOldHotkeyToNew(UserSettings.PlayerConfig.Overlay.ToggleDesignModeKey);

            IsUpdating = false;
            InitializeTrayIcon();
            // Update version text
            Version = GStrings.GetLocalizationByXPath("/Console/String[@ID='CONSOLE_VERSION']").Replace("{HunterPie_Version}", HUNTERPIE_VERSION).Replace("{HunterPie_Branch}", UserSettings.PlayerConfig.HunterPie.Update.Branch);

            // Initializes the Hotkey API
            Hotkey.Load();

            // Initializes the rest of HunterPie
            LoadData();
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_HUNTERPIE_INITIALIZED']"));

            StartEverything();

            Task.Factory.StartNew(async () =>
            {
                await pluginManager.PreloadPlugins();
                Dispatcher.Invoke(() =>
                {
                    PluginDisplay.Instance.InitializePluginDisplayer(PluginManager.packages);
                });
            });
        }

        private void OnCloseWindowButtonClick(object sender, MouseButtonEventArgs e)
        {
            // X button function;
            bool ExitConfirmation = MessageBox.Show(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_QUIT']"), "HunterPie", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (ExitConfirmation) Close();
        }

        private void OnWindowDrag(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                var point = PointToScreen(e.MouseDevice.GetPosition(this));

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
            if (UserSettings.PlayerConfig.HunterPie.MinimizeToSystemTray)
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
            pluginManager?.UnloadPlugins();
            UserSettings.PlayerConfig.HunterPie.PosX = Left;
            UserSettings.PlayerConfig.HunterPie.PosY = Top;

            if (!IsUpdating) UserSettings.SaveNewConfig();
            Debugger.DumpLog();
            Hide();
            // Dispose tray icon
            if (TrayIcon != null)
            {
                TrayIcon.NotifyIcon.Click -= OnTrayIconClick;
                TrayIcon.ContextMenu.MenuItems[0].Click -= OnTrayIconSettingsClick;
                TrayIcon.ContextMenu.MenuItems[1].Click -= OnTrayIconExitClick;
                TrayIcon.Dispose();
            }
            
            // Dispose stuff & stop scanning threads
            GameOverlay?.Dispose();
            if (MonsterHunter.IsActive) MonsterHunter?.StopScanning();
            Discord?.Dispose();
            Kernel.StopScanning();
            UserSettings.RemoveFileWatcher();

            Settings.Instance?.UninstallKeyboardHook();

            // Unhook events
            if (MonsterHunter.Player != null) UnhookGameEvents();

            UnhookEvents();
            Hotkey.Unload();
        }

        private void OnGithubButtonClick(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/Haato3o/HunterPie");

        private void OnConsoleButtonClick(object sender, MouseButtonEventArgs e) => OpenDebugger();

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e) => OpenSettings();

        private void OnPluginsButtonClick(object sender, MouseButtonEventArgs e) => OpenPlugins();

        private void OnChangelogButtonClick(object sender, MouseButtonEventArgs e) => OpenChangelog();

        private void OnUploadBuildButtonClick(object sender, MouseButtonEventArgs e)
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
                    string BuildLink = Honey.LinkStructureBuilder(MonsterHunter.Player.GetPlayerGear(), true);
                    Clipboard.SetData(DataFormats.Text, BuildLink);
                }),
                Callback2 = new Action(() =>
                {
                    string BuildLink = Honey.LinkStructureBuilder(MonsterHunter.Player.GetPlayerGear(), true);
                    Process.Start(BuildLink);
                }),
                ShowTime = 11
            };
            NotificationsPanel.Children.Add(notification);
            notification.ShowNotification();
        }

        private void OnExportGearButtonClick(object sender, MouseButtonEventArgs e)
        {
            // Task, so it doesnt freeze the UI
            Task.Factory.StartNew(() =>
            {

                sItem[] decoration = MonsterHunter.Player.GetDecorationsFromStorage();
                sGear[] gears = MonsterHunter.Player.GetGearFromStorage();

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
                    notification.ShowNotification();
                });
            });
        }

        private void OnDiscordButtonClick(object sender, MouseButtonEventArgs e) => Process.Start("https://discord.gg/5pdDq4Q");

        private void OnLaunchGameButtonClick(object sender, RoutedEventArgs e) => LaunchGame();

        private void LaunchGame()
        {
            try
            {
                ProcessStartInfo GameStartInfo = new ProcessStartInfo
                {
                    FileName = "steam://run/582010",
                    Arguments = UserSettings.PlayerConfig.HunterPie.Launch.LaunchArgs,
                    UseShellExecute = true
                };
                Process.Start(GameStartInfo);
            }
            catch (Exception err)
            {
                Debugger.Error($"{GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_LAUNCH_ERROR']")}\n{err}");
            }
        }

        private void OnWindowSizeChange(object sender, SizeChangedEventArgs e)
        {
            UserSettings.PlayerConfig.HunterPie.Width = (float)e.NewSize.Width;
            UserSettings.PlayerConfig.HunterPie.Height = (float)e.NewSize.Height;
        }

        private void Reload()
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

        #endregion

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

            PluginInformation moduleInformation = JsonConvert.DeserializeObject<PluginInformation>(moduleContent);


            if (moduleInformation is null || string.IsNullOrEmpty(moduleInformation?.Name))
            {
                Debugger.Log("Invalid module.json!");
                return;
            }

            string modPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", moduleInformation.Name);

            if (!Directory.Exists(modPath))
            {
                Directory.CreateDirectory(modPath);
            }

            File.WriteAllText(Path.Combine(modPath, "module.json"), JsonConvert.SerializeObject(moduleInformation, Formatting.Indented));


            if (PluginUpdate.PluginSupportsUpdate(moduleInformation))
            {
                if (await PluginUpdate.UpdateAllFiles(moduleInformation, modPath))
                {
                    Debugger.Module($"Installed {moduleInformation.Name}!");
                } else
                {
                    Debugger.Error($"Failed to install {moduleInformation.Name}.");
                }
            } else
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='ERROR_PLUGIN_AUTO_UPDATE']"));
            }

            CNotification notification = new CNotification()
            {
                Text = GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLUGIN_INSTALLED']").Replace("{name}", moduleInformation.Name),
                NIcon = FindResource("ICON_PLUGIN") as ImageSource,
                FirstButtonVisibility = Visibility.Collapsed,
                SecondButtonVisibility = Visibility.Collapsed,
                ShowTime = 5
            };
            NotificationsPanel.Children.Add(notification);
            notification.ShowNotification();
        }

        private void window_DragEnter(object sender, DragEventArgs e)
        {
            IsDragging = true;
        }

        private void window_DragLeave(object sender, DragEventArgs e)
        {
            IsDragging = false;
        }
    }
}
