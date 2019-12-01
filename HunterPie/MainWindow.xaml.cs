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

        const string HUNTERPIE_VERSION = "1.0.1.9";

        public MainWindow() {
            InitializeComponent();
            OpenDebugger();
            // Initialize everything under this line
            UserSettings.InitializePlayerConfig();
            CheckIfUpdateEnableAndStart();
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
            Scanner.StartScanning(); // Scans game memory
            if (!OfflineMode) StartRichPresenceThread();
            GameOverlay = new Overlay();
            GameOverlay.Show();
            ThreadScanner();
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
                    GameLoaded = MonsterHunter.Player.ZoneID != 0;
                    if (UserSettings.PlayerConfig.RichPresence.Enabled && GameLoaded) {
                        Discord.ShowPresence();

                        string BigImage = MonsterHunter.Player.ZoneName.Replace(' ', '-').Replace("'", string.Empty).ToLower();
                        string SmallImage = MonsterHunter.Player.WeaponName.Replace(' ', '-').ToLower();
                        string PartyHash = "test";
                        string Details = MonsterHunter.HuntedMonster == null ? MonsterHunter.Player.inPeaceZone ? "Idle" : "Exploring" : $"Hunting {MonsterHunter.HuntedMonster.Name} ({(int)(MonsterHunter.HuntedMonster.HPPercentage * 100)}%)";
                        string State = MonsterHunter.Player.PartySize > 1 ? "In Party" : "Solo";
                        string SmallText = $"{MonsterHunter.Player.Name} | Lvl: {MonsterHunter.Player.Level}";

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
            } catch {
                Thread.Sleep(500);
                HandlePresence();
            }
        }

        private void MainLoop() {
            UserSettings.InitializePlayerConfig();
            bool lockSpam = false;
            while (true) {
                UserSettings.LoadPlayerConfig();
                // Set components
                GameOverlay.Dispatch(new Action(() => {
                    // Monsters component
                    double MonsterComponentPosX = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0];
                    double MonsterComponentPosY = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1];
                    GameOverlay.ChangeMonsterComponentPosition(MonsterComponentPosX, MonsterComponentPosY);

                    // Primary mantle component
                    double PrimaryMantlePosX = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[0];
                    double PrimaryMantlePosY = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[1];
                    GameOverlay.ChangePrimaryMantlePosition(PrimaryMantlePosX, PrimaryMantlePosY);
                    GameOverlay.ChangePrimaryMantleColor(UserSettings.PlayerConfig.Overlay.PrimaryMantle.Color);

                    // Secondary mantle component
                    double SecondaryMantlePosX = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[0];
                    double SecondaryMantlePosY = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[1];
                    GameOverlay.ChangeSecondaryMantlePosition(SecondaryMantlePosX, SecondaryMantlePosY);
                    GameOverlay.ChangeSecondaryMantleColor(UserSettings.PlayerConfig.Overlay.SecondaryMantle.Color);

                    // Harvest Box
                    double HarvestBoxPosX = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0];
                    double HarvestBoxPosY = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1];
                    GameOverlay.ChangeHarvestBoxPosition(HarvestBoxPosX, HarvestBoxPosY);
                }));

                if (Scanner.GameIsRunning) {

                    if (Scanner.GameVersion != Address.GAME_VERSION) {
                        // try loading older version of memory address
                        if (Address.LoadMemoryMap(Scanner.GameVersion)) continue;
                        // Checks if the current game version is equal to the HunterPie mapped version
                        Debugger.Error($"Detected game version ({Scanner.GameVersion}) not mapped yet!");
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.Close();
                        }));
                        return;
                    } else {
                        if (!lockSpam) {
                            Address.LoadMemoryMap(Scanner.GameVersion);
                            Debugger.Warn($"Loaded 'MonsterHunterWorld.{Scanner.GameVersion}.map'");
                            lockSpam = true;
                        }
                    }

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
                    // Monsters
                    if (MonsterHunter.FirstMonster.TotalHP > 0) {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowMonster(GameOverlay.fMonsterBox);
                            float[] HP = { MonsterHunter.FirstMonster.CurrentHP, MonsterHunter.FirstMonster.TotalHP };
                            string Name = MonsterHunter.FirstMonster.Name;
                            GameOverlay.UpdateFirstMonsterInformation(HP, Name);
                        }));
                    } else {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HideMonster(GameOverlay.fMonsterBox);
                        }));
                    }

                    if (MonsterHunter.SecondMonster.TotalHP > 0) {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowMonster(GameOverlay.sMonsterBox);
                            float[] HP = { MonsterHunter.SecondMonster.CurrentHP, MonsterHunter.SecondMonster.TotalHP };
                            string Name = MonsterHunter.SecondMonster.Name;
                            GameOverlay.UpdateSecondMonsterInformation(HP, Name);
                        }));
                    } else {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HideMonster(GameOverlay.sMonsterBox);
                        }));
                    }

                    if (MonsterHunter.ThirdMonster.TotalHP > 0) {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowMonster(GameOverlay.tMonsterBox);
                            float[] HP = { MonsterHunter.ThirdMonster.CurrentHP, MonsterHunter.ThirdMonster.TotalHP };
                            string Name = MonsterHunter.ThirdMonster.Name;
                            GameOverlay.UpdateThirdMonsterInformation(HP, Name);
                        }));
                    } else {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HideMonster(GameOverlay.tMonsterBox);
                        }));
                    }

                    // Mantle components
                    if (MonsterHunter.Player.PrimaryMantle.Cooldown > 0 || MonsterHunter.Player.PrimaryMantle.Timer > 0) {
                        double cooldown = MonsterHunter.Player.PrimaryMantle.Cooldown / MonsterHunter.Player.PrimaryMantle.staticCooldown;
                        double timer = MonsterHunter.Player.PrimaryMantle.Timer / MonsterHunter.Player.PrimaryMantle.staticTimer;
                        double Timer = cooldown != 1 ? cooldown : timer;
                        float TimeLeft = cooldown != 1 ? MonsterHunter.Player.PrimaryMantle.Cooldown : MonsterHunter.Player.PrimaryMantle.Timer;

                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowPrimaryMantle();
                            GameOverlay.UpdatePrimaryMantleTimer(Timer);
                            GameOverlay.UpdatePrimaryMantleText($"({(int)TimeLeft}s) {MonsterHunter.Player.PrimaryMantle.Name.ToUpper()}");
                        }));
                    } else {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HidePrimaryMantle();
                        }));
                    }

                    if (MonsterHunter.Player.SecondaryMantle.Cooldown > 0 || MonsterHunter.Player.SecondaryMantle.Timer > 0) {
                        double cooldown = MonsterHunter.Player.SecondaryMantle.Cooldown / MonsterHunter.Player.SecondaryMantle.staticCooldown;
                        double timer = MonsterHunter.Player.SecondaryMantle.Timer / MonsterHunter.Player.SecondaryMantle.staticTimer;
                        double Timer = cooldown != 1 ? cooldown : timer;
                        float TimeLeft = cooldown != 1 ? MonsterHunter.Player.SecondaryMantle.Cooldown : MonsterHunter.Player.SecondaryMantle.Timer;
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowSecondaryMantle();
                            GameOverlay.UpdateSecondaryMantleTimer(Timer);
                            GameOverlay.UpdateSecondaryMantleText($"({(int)TimeLeft}s) {MonsterHunter.Player.SecondaryMantle.Name.ToUpper()}");
                        }));
                    } else {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HideSecondaryMantle();
                        }));
                    }
                    
                    // Harvest box
                    if (UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled && MonsterHunter.Player.inHarvestZone) {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.ShowHarvestBoxContainer();
                            Fertilizer[] fertilizer = MonsterHunter.Player.Harvest.Box;
                            GameOverlay.UpdateFirstFertilizer(fertilizer[0].Name, fertilizer[0].Amount);
                            GameOverlay.UpdateSecondFertilizer(fertilizer[1].Name, fertilizer[1].Amount);
                            GameOverlay.UpdateThirdFertilizer(fertilizer[2].Name, fertilizer[2].Amount);
                            GameOverlay.UpdateFourthFertilizer(fertilizer[3].Name, fertilizer[3].Amount);
                            GameOverlay.UpdateHarvestBoxCounter(MonsterHunter.Player.Harvest.Counter, MonsterHunter.Player.Harvest.Max);
                        }));
                    } else {
                        GameOverlay.Dispatch(new Action(() => {
                            GameOverlay.HideHarvestBoxContainer();
                        }));
                    }

                } else {
                    GameOverlay.Dispatch(new Action(() => {
                        GameOverlay.Hide();
                    }));
                    Discord.HidePresence();
                    lockSpam = false;
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
