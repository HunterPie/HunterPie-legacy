using System;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using HunterPie.Memory;
using HunterPie.Core;
using HunterPie.GUI;
using DiscordRPC;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace HunterPie {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        bool OfflineMode = false;

        Game MonsterHunter = new Game();
        Presence Discord = new Presence();
        Overlay GameOverlay;

        ThreadStart ThreadScannerRef;
        Thread MainThreadScanner;

        ThreadStart RichPresenceThreadRef;
        Thread RichPresenceThread;

        const string HUNTERPIE_VERSION = "1.0.2.0";

        public MainWindow() {
            InitializeComponent();
            OpenDebugger();
            // Initialize everything under this line
            UserSettings.InitializePlayerConfig();
            //CheckIfUpdateEnableAndStart();
            // Updates version_text
            this.version_text.Content = $"Version: {HUNTERPIE_VERSION}";
            Debugger.Warn("Initializing HunterPie!");
            GStrings.InitStrings();
            Discord.InitializePresence();
            StartEverything();
        }

        private bool StartUpdateProcess() {
            if (!File.Exists("Update.exe")) return false;

            Process UpdateProcess = new Process();
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
                    openChangeLog();
                    return;
                }
                if (latestVersion) {
                    return;
                }
                // This will update Update.exe
                AutoUpdate au = new AutoUpdate(UserSettings.PlayerConfig.HunterPie.Update.Branch);
                au.checkAutoUpdate();
                if (au.offlineMode) {
                    Debugger.Error("Failed to update HunterPie. Check if you're connected to the internet.");
                    Debugger.Warn("HunterPie is now in offline mode.");
                    OfflineMode = true;
                    return;
                }
                bool StartUpdate = StartUpdateProcess();
                if (StartUpdate) {
                    Environment.Exit(0);
                } else {
                    MessageBox.Show("Update.exe not found! Skipping auto-update...", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                Debugger.Error("Auto-update is disabled. If your HunterPie has any issues or doesn't support the current game version, try re-enabling auto-update!");
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
            MonsterHunter.StartScanning();
            SetGameEventHandlers();
            Scanner.StartScanning(); // Scans game memory
            if (!OfflineMode) StartRichPresenceThread();
            GameOverlay = new Overlay(MonsterHunter);
            GameOverlay.Show();
            ThreadScanner();
        }

        private void SetGameEventHandlers() {
            // Scanner events
            Scanner.OnGameStart += OnGameStart;
            Scanner.OnGameClosed += OnGameClose;
            // Session
            MonsterHunter.Player.OnZoneChange += OnZoneChange;
            MonsterHunter.Player.OnCharacterLogin += OnLogin;
            // Settings
            UserSettings.OnSettingsUpdate += SendToOverlay;
        }

        public void SendToOverlay(object source, EventArgs e) {
            GameOverlay.Dispatch(() => {
                GameOverlay.GlobalSettingsEventHandler(source, e);
            });
        }

        public void OnZoneChange(object source, EventArgs e) {
            Debugger.Log($"ZoneID: {MonsterHunter.Player.ZoneID}");
        }

        public void OnLogin(object source, EventArgs e) {
            //Debugger.Log(MonsterHunter.Player.Slot.ToString());
        }

        public void OnGameStart(object source, EventArgs e) {
            Debugger.Log("hi");
            if (Address.LoadMemoryMap(Scanner.GameVersion) || Scanner.GameVersion == Address.GAME_VERSION) {
                Debugger.Warn($"Loaded 'MonsterHunterWorld.{Scanner.GameVersion}.map'");
            } else {
                Debugger.Error($"Detected game version ({Scanner.GameVersion}) not mapped yet!");
                try {
                    GameOverlay.Dispatch(new Action(() => {
                        GameOverlay.Close();
                    }));
                } catch {}
                return;
            }
        }

        public void OnGameClose(object source, EventArgs e) {
            if (UserSettings.PlayerConfig.HunterPie.Options.CloseWhenGameCloses) {
                this.Close();
                Environment.Exit(0);
            }
        }

        private void StopMainThread() {
            MainThreadScanner.Abort();
        }

        private void StartRichPresenceThread() {
            RichPresenceThreadRef = new ThreadStart(HandlePresence);
            RichPresenceThread = new Thread(RichPresenceThreadRef);
            RichPresenceThread.Name = "Thread_RichPresence";
            RichPresenceThread.Start();
        }

        private void ThreadScanner() {
            ThreadScannerRef = new ThreadStart(MainLoop);
            MainThreadScanner = new Thread(ThreadScannerRef);
            MainThreadScanner.Name = "Scanner_Main";
            MainThreadScanner.Start();
        }

        private void HandlePresence() {
            bool GameLoaded = false;
            try {
                while (Scanner.GameIsRunning) {
                    if (MonsterHunter.Player.Slot == -1) {
                        break;
                    }
                    GameLoaded = MonsterHunter.Player.ZoneID != 0 || MonsterHunter.Player.Slot == 999;
                    if (UserSettings.PlayerConfig.RichPresence.Enabled && GameLoaded) {
                        Discord.ShowPresence();

                        string BigImage;
                        string SmallImage;
                        string PartyHash = "test";
                        string Details;
                        string State;
                        string SmallText;
                        if (MonsterHunter.Player.Slot == 999) {
                            BigImage = "main-menu";
                            SmallImage = null;
                            Details = MonsterHunter.Player.ZoneID == 0 ? "Character selection" : "In Main menu";
                            State = null;
                            SmallText = null;
                        } else {
                            BigImage = MonsterHunter.Player.ZoneName == null ? "main-menu" : MonsterHunter.Player.ZoneName.Replace(' ', '-').Replace("'", string.Empty).ToLower();
                            SmallImage = MonsterHunter.Player.WeaponName == null ? "hunter-rank" : MonsterHunter.Player.WeaponName.Replace(' ', '-').ToLower();
                            Details = MonsterHunter.HuntedMonster == null ? MonsterHunter.Player.inPeaceZone ? "Idle" : "Exploring" : $"Hunting {MonsterHunter.HuntedMonster.Name} ({(int)(MonsterHunter.HuntedMonster.HPPercentage * 100)}%)";
                            State = MonsterHunter.Player.PartySize > 1 ? "In Party" : "Solo";
                            SmallText = $"{MonsterHunter.Player.Name} | Lvl: {MonsterHunter.Player.Level}";
                        }
                         
                        Assets presenceAssets = Discord.GenerateAssets(BigImage, MonsterHunter.Player.ZoneName == "Main Menu" ? null : MonsterHunter.Player.ZoneName, SmallImage, SmallText);
                        Party presenceParty = Discord.MakeParty(MonsterHunter.Player.PartySize, MonsterHunter.Player.PartyMax, PartyHash);
                        Timestamps presenceTime = Discord.NewTimestamp(MonsterHunter.Time);
                        Discord.UpdatePresenceInfo(Details, State, presenceAssets, presenceParty, presenceTime);
                    } else {
                        Discord.HidePresence();
                    }
                    Thread.Sleep(10000);
                }
                Discord.HidePresence();
                Thread.Sleep(500);
                HandlePresence();
            } catch(Exception err) {
                Debugger.Error(err.ToString());
                Thread.Sleep(500);
                HandlePresence();
            }
        }

        private void MainLoop() {
            while (true) {

                if (Scanner.GameIsRunning) {

                    // Hides/show overlay when user disable/enable it
                    if (!UserSettings.PlayerConfig.Overlay.Enabled) {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HideOverlay();
                        }));
                    } else if (UserSettings.PlayerConfig.Overlay.Enabled) {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowOverlay();
                        }));
                    }

                } else {
                    GameOverlay.Dispatch(new Action(() => {
                        GameOverlay.Hide();
                    }));
                    Discord.HidePresence();
                }
                Thread.Sleep(200);
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {
            // X button function
            bool ExitConfirmation = MessageBox.Show("Are you sure you want to exit HunterPie?", "HunterPie", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (ExitConfirmation) {
                try {
                    // Stop Threads
                    Discord.DisconnectPresence();
                    MonsterHunter.StopScanning();
                    StopMainThread();
                    Scanner.StopScanning();
                    GameOverlay.Close();
                } catch {}
                // Close stuff
                this.Close();
                Environment.Exit(0);
            }
        }

        private void WindowTopBar_MouseDown(object sender, MouseButtonEventArgs e) {
            // When top bar is held by LMB
            this.DragMove();
        }

        private void minimizeWindow_MouseDown(object sender, MouseButtonEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void OpenDebugger() {
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Debugger.Instance);
        }

        private void OpenSettingsWindow() {
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Settings.Instance);
            Settings.RefreshSettingsUI();
        }

        private void consoleButton_Click(object sender, RoutedEventArgs e) {
            OpenDebugger();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e) {
            OpenSettingsWindow();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (GameOverlay != null) GameOverlay.Close();
        }

        private void githubButton_Click(object sender, RoutedEventArgs e) {
            Process.Start("https://github.com/Haato3o/HunterPie");
        }

        private void openChangeLog() {
            ConsolePanel.Children.Clear();
            ConsolePanel.Children.Add(Changelog.Instance);
        }

        private void changelogButton_click(object sender, RoutedEventArgs e) {
            openChangeLog();
        }

        private void LaunchGame() {
            try {
                Process createGameProcess = new Process();
                createGameProcess.StartInfo.FileName = UserSettings.PlayerConfig.HunterPie.Launch.GamePath;
                createGameProcess.StartInfo.Arguments = UserSettings.PlayerConfig.HunterPie.Launch.LaunchArgs;
                createGameProcess.Start();
            } catch {
                Debugger.Error("Failed to launch Monster Hunter World. Common reasons for this error are:\n- Wrong file path;");
            }
            
        }

        private void launchGameButton_Click(object sender, RoutedEventArgs e) {
            // Shorten the class name
            var launchOptions = UserSettings.PlayerConfig.HunterPie.Launch;

            if (launchOptions.GamePath == "") {
                if (MessageBox.Show("You haven't added the game path yet. Do you want to do it now?", "Monster Hunter World path not found", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes) {
                    OpenSettingsWindow();
                }
            } else {
                LaunchGame();
            }
        }
    }
}
