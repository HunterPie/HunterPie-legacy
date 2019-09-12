using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Animation;
using HunterPie.Memory;
using HunterPie.Core;
using HunterPie.GUI;

namespace HunterPie {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        Game MonsterHunter = new Game();
        Overlay GameOverlay;

        public MainWindow() {
            InitializeComponent();
            OpenDebugger();
            // Initialize everything under this line
            Debugger.Warn("Initializing HunterPie!");
            GStrings.InitStrings();
            StartEverything();
        }

        private void MainLoop(object sender, EventArgs e) {

        }

        private void StartEverything() {
            ThreadScanner();
            MonsterHunter.StartScanning();
            Scanner.StartScanning(); // Scans game memory
            GameOverlay = new Overlay();
            GameOverlay.Show();
        }

        private void ThreadScanner() {
            ThreadStart ThreadScannerRef = new ThreadStart(MainLoop);
            Thread MainThreadScanner = new Thread(ThreadScannerRef);
            MainThreadScanner.Name = "Scanner_Main";
            MainThreadScanner.Start();
            
        }

        private void MainLoop() {
            while (true) {
                UserSettings.LoadPlayerConfig();

                // Moves overlay components 
                GameOverlay.Dispatch(new Action(() => {
                    double posX = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0];
                    double posY = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1];
                    GameOverlay.ChangeMonsterComponentPosition(posX, posY);
                }));

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
                } else {
                    GameOverlay.Dispatch(new Action(() => {
                        GameOverlay.Hide();
                    }));
                }

                Thread.Sleep(200);
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e) {
            // X button function
            bool ExitConfirmation = MessageBox.Show("Are you sure you want to exit HunterPie?", "HunterPie", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
            if (ExitConfirmation) {
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
        }

        private void consoleButton_Click(object sender, RoutedEventArgs e) {
            OpenDebugger();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e) {
            OpenSettingsWindow();
            
        }
    }
}
