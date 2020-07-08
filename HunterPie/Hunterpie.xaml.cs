using System;
using System.IO;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;
using HunterPie.Core.Definitions;
using HunterPie.Core.Integrations.DataExporter;
using HunterPie.GUI;
using HunterPie.GUIControls;
using HunterPie.GUIControls.Custom_Controls;
using HunterPie.Logger;
// HunterPie
using HunterPie.Memory;
using Presence = HunterPie.Core.Integrations.Discord.Presence;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

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
        bool OfflineMode = false;
        readonly bool IsUpdating = true;

        // HunterPie version
        const string HUNTERPIE_VERSION = "1.0.3.94";

        // Helpers
        IntPtr _windowHandle;
        HwndSource _source;

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


        public Hunterpie()
        {

            if (CheckIfHunterPieOpen())
            {
                Close();
                return;
            }

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
            Debugger.LoadNewColors();

            AdministratorIconVisibility = IsRunningAsAdmin() ? Visibility.Visible : Visibility.Collapsed;

            InitializeComponent();

            OpenDebugger();
            // Initialize everything under this line
            if (!CheckIfUpdateEnableAndStart()) return;

            // Convert the old HotKey to the new one
            ConvertOldHotkeyToNew(UserSettings.PlayerConfig.Overlay.ToggleDesignModeKey);

            IsUpdating = false;
            InitializeTrayIcon();
            // Update version text
            Version = GStrings.GetLocalizationByXPath("/Console/String[@ID='CONSOLE_VERSION']").Replace("{HunterPie_Version}", HUNTERPIE_VERSION).Replace("{HunterPie_Branch}", UserSettings.PlayerConfig.HunterPie.Update.Branch);

            // Initializes the rest of HunterPie
            LoadData();
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_HUNTERPIE_INITIALIZED']"));

            SetHotKeys();
            StartEverything();
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
        }

        private bool CheckIfHunterPieOpen() =>
            // Block new instances of HunterPie if there's one already running
            Process.GetProcessesByName("HunterPie").Length > 1;

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
            _windowHandle = new WindowInteropHelper(this).EnsureHandle();
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            BindHotKey(0); // Toggle overlay
            BindHotKey(1); // Switch monster bar mode
            BindHotKey(2); // Toggle design mode
        }

        private void RemoveHotKeys()
        {
            KeyboardHookHelper.UnregisterHotKey(_windowHandle, 0);
            KeyboardHookHelper.UnregisterHotKey(_windowHandle, 1);
            KeyboardHookHelper.UnregisterHotKey(_windowHandle, 2);
            _source?.RemoveHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case 0: // Toggle overlay
                            if (GameOverlay == null) break;
                            UserSettings.PlayerConfig.Overlay.Enabled = !UserSettings.PlayerConfig.Overlay.Enabled;
                            UserSettings.SaveNewConfig();
                            break;
                        case 1: // Switch monster bar mode
                            if (GameOverlay == null) break;
                            UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode = UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode + 1 >= 5 ? (byte)0 : (byte)(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode + 1);
                            UserSettings.SaveNewConfig();
                            break;
                        case 2:
                            GameOverlay?.ToggleDesignMode();
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private int[] ParseHotKey(string hotkey)
        {
            if (hotkey == "None") return null;
            string[] Keys = hotkey.Split('+');
            int Modifier = 0x4000;  // Start with no-repeat
            int key = 0x0;
            foreach (string hkey in Keys)
            {
                switch (hkey)
                {
                    case "Alt":
                        Modifier |= 0x0001;
                        break;
                    case "Ctrl":
                        Modifier |= 0x0002;
                        break;
                    case "Shift":
                        Modifier |= 0x0004;
                        break;
                    default:
                        key = (int)Enum.Parse(typeof(KeyboardHookHelper.KeyboardKeys), hkey);
                        break;
                }
            }
            int[] parsed = new int[2] { Modifier, key };
            return parsed;
        }

        private void BindHotKey(int ID)
        {
            switch (ID)
            {
                case 0: // Overlay toggle
                    int[] ParsedToggleOverlayHotKey = ParseHotKey(UserSettings.PlayerConfig.Overlay.ToggleOverlayKeybind);
                    if (ParsedToggleOverlayHotKey == null) break;
                    if (!KeyboardHookHelper.RegisterHotKey(_windowHandle, 0, ParsedToggleOverlayHotKey[0], ParsedToggleOverlayHotKey[1]))
                    {
                        Debugger.Error("Failed to register hotkey for Overlay toggle");
                    }
                    break;
                case 1: // Monster bar mode switch
                    int[] ParsedToggleBarModeHotKey = ParseHotKey(UserSettings.PlayerConfig.Overlay.MonstersComponent.SwitchMonsterBarModeHotkey);
                    if (ParsedToggleBarModeHotKey == null) break;
                    if (!KeyboardHookHelper.RegisterHotKey(_windowHandle, 1, ParsedToggleBarModeHotKey[0], ParsedToggleBarModeHotKey[1]))
                    {
                        Debugger.Error("Failed to register hotkey for monster bar mode switch");
                    }
                    break;
                case 2: // Design mode toggle
                    int[] Parsed = ParseHotKey(UserSettings.PlayerConfig.Overlay.ToggleDesignKeybind);
                    if (Parsed == null) break;
                    if (!KeyboardHookHelper.RegisterHotKey(_windowHandle, 2, Parsed[0], Parsed[1]))
                    {
                        Debugger.Error("Failed to register hotkey for Design mode toggle");
                    }
                    break;
            }
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

        private void ExceptionLogger(object sender, UnhandledExceptionEventArgs e) => File.WriteAllText("crashes.txt", e.ExceptionObject.ToString());

        private void StartEverything()
        {
            SetAnimationsFramerate();
            HookEvents();
            Scanner.StartScanning(); // Scans game memory
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
            // Scanner events
            Scanner.OnGameStart += OnGameStart;
            Scanner.OnGameClosed += OnGameClose;
            // Settings
            UserSettings.OnSettingsUpdate += SendToOverlay;
        }

        private void UnhookEvents()
        {
            // Debug
            AppDomain.CurrentDomain.UnhandledException -= ExceptionLogger;
            // Scanner events
            Scanner.OnGameStart -= OnGameStart;
            Scanner.OnGameClosed -= OnGameClose;
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
                Debugger.Log($"ZoneID: {MonsterHunter.Player.ZoneID}");
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
            // Create game instances
            MonsterHunter.CreateInstances();

            // Hook game events
            HookGameEvents();

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
            if (Address.LoadMemoryMap(Scanner.GameVersion) || Scanner.GameVersion == Address.GAME_VERSION)
            {
                Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_MAP_LOAD']").Replace("{HunterPie_Map}", $"'MonsterHunterWorld.{Scanner.GameVersion}.map'"));
            }
            else
            {
                Debugger.Error(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_GAME_VERSION_UNSUPPORTED']").Replace("{GAME_VERSION}", $"{Scanner.GameVersion}"));
                return;
            }

            // Starts scanning
            MonsterHunter.StartScanning();

            // Initializes rich presence
            if (Discord == null)
            {
                Discord = new Presence(MonsterHunter);
                if (OfflineMode) Discord.SetOfflineMode();
                Discord.StartRPC();
            }
        }

        public void OnGameClose(object source, EventArgs e)
        {
            UnhookGameEvents();
            Discord.Dispose();
            Discord = null;
            if (UserSettings.PlayerConfig.HunterPie.Options.CloseWhenGameCloses)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                {
                    Close();
                }));
            }
            MonsterHunter.StopScanning();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                GameOverlay.Dispose();
                GameOverlay = null;
            }));
            MonsterHunter.DestroyInstances();
        }

        #endregion

        #region Sub Windows
        /* Open sub windows */

        private void OpenDebugger()
        {
            SwitchButtonOn(BUTTON_CONSOLE);
            SwitchButtonOff(BUTTON_CHANGELOG);
            SwitchButtonOff(BUTTON_SETTINGS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Debugger.Instance);
        }

        private void OpenSettings()
        {
            SwitchButtonOff(BUTTON_CONSOLE);
            SwitchButtonOff(BUTTON_CHANGELOG);
            SwitchButtonOn(BUTTON_SETTINGS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Settings.Instance);
            Settings.RefreshSettingsUI();
        }

        private void OpenChangelog()
        {
            SwitchButtonOff(BUTTON_CONSOLE);
            SwitchButtonOn(BUTTON_CHANGELOG);
            SwitchButtonOff(BUTTON_SETTINGS);
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
            Width = UserSettings.PlayerConfig.HunterPie.Width;
            Height = UserSettings.PlayerConfig.HunterPie.Height;
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

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
            if (MonsterHunter.IsActive) MonsterHunter.StopScanning();
            Discord?.Dispose();
            Scanner.StopScanning();
            UserSettings.RemoveFileWatcher();
            Settings.Instance.UninstallKeyboardHook();
            // Unhook events
            if (MonsterHunter.Player != null) UnhookGameEvents();
            if (_source != null) RemoveHotKeys();
            UnhookEvents();
        }

        private void OnGithubButtonClick(object sender, MouseButtonEventArgs e) => Process.Start("https://github.com/Haato3o/HunterPie");

        private void OnConsoleButtonClick(object sender, MouseButtonEventArgs e) => OpenDebugger();

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e) => OpenSettings();

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

        #endregion

    }
}
