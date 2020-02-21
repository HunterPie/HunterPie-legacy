using System;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls;
using HunterPie.GUIControls;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Core;
using HunterPie.GUI;

namespace HunterPie {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Hunterpie : Window {

        // Classes
        TrayIcon TrayIcon = new TrayIcon();
        Game MonsterHunter = new Game();
        Presence Discord;
        Overlay GameOverlay;

        // HunterPie version
        const string HUNTERPIE_VERSION = "1.0.3.0";

        public Hunterpie() {
            Debugger.InitializeDebugger();
            LoadCustomTheme();
            InitializeComponent();
            OpenDebugger();
            AppDomain.CurrentDomain.UnhandledException += ExceptionLogger;
            // Initialize everything under this line
            InitializeTrayIcon();
            UserSettings.InitializePlayerConfig();
            CheckIfUpdateEnableAndStart();
            // Updates version_text
            this.version_text.Content = $"Version: {HUNTERPIE_VERSION} ({UserSettings.PlayerConfig.HunterPie.Update.Branch})";
            Debugger.Warn("Initializing HunterPie!");
            GStrings.InitStrings(UserSettings.PlayerConfig.HunterPie.Language);
            MonsterData.LoadMonsterData();
            StartEverything();
        }

        private void InitializeTrayIcon() {
            // Tray icon itself
            TrayIcon.NotifyIcon.BalloonTipTitle = "HunterPie";
            TrayIcon.NotifyIcon.Text = "HunterPie";
            TrayIcon.NotifyIcon.Icon = Properties.Resources.LOGO_HunterPie;
            TrayIcon.NotifyIcon.Visible = true;
            TrayIcon.NotifyIcon.Click += OnTrayIconClick;

            // Menu items
            System.Windows.Forms.MenuItem ExitItem = new System.Windows.Forms.MenuItem() {
                Index = 0,
                Text = "Close"
            };
            ExitItem.Click += OnTrayIconExitClick;
            TrayIcon.ContextMenu.MenuItems.Add(ExitItem);
        }

        private void OnTrayIconExitClick(object sender, EventArgs e) {
            this.Close();
        }

        private void OnTrayIconClick(object sender, EventArgs e) {
            this.Show();
        }

        private void LoadCustomTheme() {
            /*
            try {
                using (FileStream stream = new FileStream("Themes/Light.xaml", FileMode.Open)) {
                    XamlReader reader = new XamlReader();
                    ResourceDictionary ThemeDictionary = (ResourceDictionary)reader.LoadAsync(stream);
                    Application.Current.Resources.MergedDictionaries.Add(ThemeDictionary);
                }
            } catch {
                Debugger.Error("Failed to load custom theme");
            }
            */
        }

        private void ExceptionLogger(object sender, UnhandledExceptionEventArgs e) {
            using (var crashfile = File.AppendText("crashes.txt")) {
                crashfile.Write(e.ExceptionObject.ToString());
            }
        }

        private bool StartUpdateProcess() {
            if (!File.Exists("Update.exe")) return false;

            System.Diagnostics.Process UpdateProcess = new System.Diagnostics.Process();
            UpdateProcess.StartInfo.FileName = "Update.exe";
            UpdateProcess.StartInfo.Arguments = $"version={HUNTERPIE_VERSION} branch={UserSettings.PlayerConfig.HunterPie.Update.Branch}";
            UpdateProcess.Start();
            return true;
        }

        private void CheckIfUpdateEnableAndStart() {
            if (UserSettings.PlayerConfig.HunterPie.Update.Enabled) {
                bool justUpdated = false;
                bool latestVersion = false;
                string[] args = Environment.GetCommandLineArgs();
                foreach (string argument in args) {
                    if (argument.StartsWith("justUpdated")) {
                        string parsed = ParseArgs(argument);
                        justUpdated = parsed == "True";
                    }
                    if (argument.StartsWith("latestVersion")) {
                        string parsed = ParseArgs(argument);
                        latestVersion = parsed == "True";
                    }
                }
                if (justUpdated) {
                    OpenChangelog();
                    return;
                }
                if (latestVersion) {
                    return;
                }
                // This will update Update.exe
                AutoUpdate au = new AutoUpdate(UserSettings.PlayerConfig.HunterPie.Update.Branch);
                au.Instance.DownloadFileCompleted += OnUpdaterDownloadComplete;
                if (!au.CheckAutoUpdate()) {
                    HandleUpdaterUpdate();
                }
                this.Hide();
            } else {
                Debugger.Error("Auto-update is disabled. If your HunterPie has any issues or doesn't support the current game version, try re-enabling auto-update!");
            }
        }

        private void OnUpdaterDownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
            if (e.Error != null) {
                Debugger.Error("Failed to update HunterPie. Check if you're connected to the internet.");
                Debugger.Warn("HunterPie is now in offline mode.");
                Discord.SetOfflineMode();
                return;
            }
            HandleUpdaterUpdate();
        }

        private void HandleUpdaterUpdate() {
            bool StartUpdate = StartUpdateProcess();
            if (StartUpdate) {
                Environment.Exit(0);
            } else {
                MessageBox.Show("Update.exe not found! Skipping auto-update...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ParseArgs(string arg) {
            try {
                return arg.Split('=')[1];
            } catch {
                return "";
            }
        }

        private void StartEverything() {
            HookEvents();
            Scanner.StartScanning(); // Scans game memory
        }

        /* Game events */
        private void HookEvents() {
            // Scanner events
            Scanner.OnGameStart += OnGameStart;
            Scanner.OnGameClosed += OnGameClose;
            // Settings
            UserSettings.OnSettingsUpdate += SendToOverlay;
        }

        private void OnSessionChange(object source, EventArgs args) {
            Debugger.Log($"SESSION: {MonsterHunter.Player.SessionID}");
            
        }

        private void UnhookEvents() {
            // Debug
            AppDomain.CurrentDomain.UnhandledException -= ExceptionLogger;
            // Scanner events
            Scanner.OnGameStart -= OnGameStart;
            Scanner.OnGameClosed -= OnGameClose;
            // Settings
            UserSettings.OnSettingsUpdate -= SendToOverlay;
        }

        private void HookGameEvents() {
            // Game events
            MonsterHunter.Player.OnZoneChange += OnZoneChange;
            MonsterHunter.Player.OnCharacterLogin += OnLogin;
            MonsterHunter.Player.OnSessionChange += OnSessionChange;
        }

        private void UnhookGameEvents() {
            MonsterHunter.Player.OnZoneChange -= OnZoneChange;
            MonsterHunter.Player.OnCharacterLogin -= OnLogin;
            MonsterHunter.Player.OnSessionChange -= OnSessionChange;
        }

        public void SendToOverlay(object source, EventArgs e) {
            GameOverlay?.Dispatch(() => {
                GameOverlay.GlobalSettingsEventHandler(source, e);
            });
        }

        public void OnZoneChange(object source, EventArgs e) {
            Debugger.Log($"ZoneID: {MonsterHunter.Player.ZoneID}");
        }

        public void OnLogin(object source, EventArgs e) {
            Debugger.Log($"Logged on {MonsterHunter.Player.Name}");
        }

        public void OnGameStart(object source, EventArgs e) {
            // Create game instances
            MonsterHunter.CreateInstances();

            // Hook game events
            HookGameEvents();

            // Creates new overlay
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (GameOverlay == null) {
                    GameOverlay = new Overlay(MonsterHunter);
                    GameOverlay.HookEvents();
                    UserSettings.TriggerSettingsEvent();
                } 
            }));
            
            // Loads memory map
            if (Address.LoadMemoryMap(Scanner.GameVersion) || Scanner.GameVersion == Address.GAME_VERSION) {
                Debugger.Warn($"Loaded 'MonsterHunterWorld.{Scanner.GameVersion}.map'");
            } else {
                Debugger.Error($"Detected game version ({Scanner.GameVersion}) not mapped yet!");
                return;
            }

            // Starts scanning
            MonsterHunter.StartScanning();

            // Initializes rich presence
            if (Discord == null) {
                Discord = new Presence(MonsterHunter);
                Discord.StartRPC();
            }
        }

        public void OnGameClose(object source, EventArgs e) {
            UnhookGameEvents();
            Discord.Dispose();
            Discord = null;
            if (UserSettings.PlayerConfig.HunterPie.Options.CloseWhenGameCloses) {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                    this.Close();
                }));
            }
            MonsterHunter.StopScanning();
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                GameOverlay.Dispose();
                GameOverlay = null;
            }));
            MonsterHunter.DestroyInstances();
        }

        /* Open sub windows */
        
        private void OpenDebugger() {
            this.SwitchButtonOn(BUTTON_CONSOLE);
            this.SwitchButtonOff(BUTTON_CHANGELOG);
            this.SwitchButtonOff(BUTTON_SETTINGS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Debugger.Instance);
        }

        private void OpenSettings() {
            this.SwitchButtonOff(BUTTON_CONSOLE);
            this.SwitchButtonOff(BUTTON_CHANGELOG);
            this.SwitchButtonOn(BUTTON_SETTINGS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Settings.Instance);
            Settings.RefreshSettingsUI();
        }

        private void OpenChangelog() {
            this.SwitchButtonOff(BUTTON_CONSOLE);
            this.SwitchButtonOn(BUTTON_CHANGELOG);
            this.SwitchButtonOff(BUTTON_SETTINGS);
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Changelog.Instance);
        }

        /* Animations */
        private void SwitchButtonOn(StackPanel buttonActive) {
            Border ButtonBorder = (Border)buttonActive.Children[0];
            ButtonBorder.SetValue(BorderThicknessProperty, new Thickness(4, 0, 0, 0));
        }

        private void SwitchButtonOff(StackPanel buttonActive) {
            Border ButtonBorder = (Border)buttonActive.Children[0];
            ButtonBorder.SetValue(BorderThicknessProperty, new Thickness(0, 0, 0, 0));
        }

        /* Events */

        private void OnCloseWindowButtonClick(object sender, MouseButtonEventArgs e) {
            // X button function
            bool ExitConfirmation = MessageBox.Show("Are you sure you want to exit HunterPie?", "HunterPie", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (ExitConfirmation) {
                this.Close();
            }
        }

        private void OnWindowDrag(object sender, MouseButtonEventArgs e) {
            // When top bar is held by LMB
            this.DragMove();
        }

        private void OnMinimizeButtonClick(object sender, MouseButtonEventArgs e) {
            if (UserSettings.PlayerConfig.HunterPie.MinimizeToSystemTray) {
                this.Hide();
            } else {
                this.WindowState = WindowState.Minimized;
            }
            
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.Hide();
            // Dispose tray icon
            TrayIcon.NotifyIcon.Click -= OnTrayIconClick;
            TrayIcon.Dispose();

            // Dispose stuff & stop scanning threads
            GameOverlay?.Dispose();
            if (MonsterHunter.IsActive) MonsterHunter.StopScanning();
            Discord?.Dispose();
            Scanner.StopScanning();
            UserSettings.RemoveFileWatcher();

            // Unhook events
            if (MonsterHunter.Player != null) UnhookGameEvents();
            this.UnhookEvents();
        }

        private void OnGithubButtonClick(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Haato3o/HunterPie");
        }

        private void OnConsoleButtonClick(object sender, MouseButtonEventArgs e) {
            OpenDebugger();
        }

        private void OnSettingsButtonClick(object sender, MouseButtonEventArgs e) {
            OpenSettings();
        }

        private void OnChangelogButtonClick(object sender, MouseButtonEventArgs e) {
            OpenChangelog();
        }

        private void OnLaunchGameButtonClick(object sender, RoutedEventArgs e) {
            // Shorten the class name
            var launchOptions = UserSettings.PlayerConfig.HunterPie.Launch;

            if (launchOptions.GamePath == "") {
                if (MessageBox.Show("You haven't added the game path yet. Do you want to do it now?", "Monster Hunter World path not found", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
                    OpenSettings();
                }
            } else {
                LaunchGame();
            }
        }

        private void LaunchGame() {
            try {
                System.Diagnostics.Process createGameProcess = new System.Diagnostics.Process();
                createGameProcess.StartInfo.FileName = UserSettings.PlayerConfig.HunterPie.Launch.GamePath;
                createGameProcess.StartInfo.Arguments = UserSettings.PlayerConfig.HunterPie.Launch.LaunchArgs;
                createGameProcess.Start();
            } catch {
                Debugger.Error("Failed to launch Monster Hunter World. Common reasons for this error are:\n- Wrong file path;");
            }
        }

        private void OnDiscordButtonClick(object sender, MouseButtonEventArgs e) {
            System.Diagnostics.Process.Start("https://discord.gg/5pdDq4Q");
        }
    }
}
