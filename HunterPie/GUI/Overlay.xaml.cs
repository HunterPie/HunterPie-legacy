using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using HunterPie.Core;
using HunterPie.GUIControls;
using HunterPie.Logger;

namespace HunterPie.GUI {
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window {

        Game ctx;
        double w_Height = Screen.PrimaryScreen.Bounds.Height;
        double w_Width = Screen.PrimaryScreen.Bounds.Width;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int style);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public Overlay(Game Context) {
            ctx = Context;
            InitializeComponent();
            SetOverlaySize();
            HookEvents();
            makeOverlayClickThrough();
        }

        public void Destroy() {
            UnhookEvents();
            this.Close();
        }

        private void HookEvents() {
            HookMonsterEvents();
            HookMantleEvents();
            HookHarvestBoxEvents();
        }

        public void UnhookEvents() {
            UnhookMonsterEvents();
            UnhookMantleEvents();
            UnhookHarvestBoxEvents();
        }

        public void GlobalSettingsEventHandler(object source, EventArgs e) {
            this.ToggleOverlay(source, e);
            this.ChangeHarvestBoxPosition(source, e);
            this.ChangeMonsterComponentPosition(source, e);
            this.ChangePrimaryMantlePosition(source, e);
            this.ChangeSecondaryMantlePosition(source, e);
            this.ChangePrimaryMantleColor(source, e);
            this.ChangeSecondaryMantleColor(source, e);
        }

        private void HookHarvestBoxEvents() {
            // Hooks player location event
            ctx.Player.OnVillageEnter += this.ShowHarvestBox;
            ctx.Player.OnVillageLeave += this.HideHarvestBox;
            // Hook fertilizer and Harvest box events
            ctx.Player.Harvest.OnCounterChange += this.UpdateHarvestBoxCounter;
            // First fertilizer
            ctx.Player.Harvest.Box[0].OnAmountUpdate += this.UpdateFirstFertilizer;
            ctx.Player.Harvest.Box[0].OnFertilizerChange += this.UpdateFirstFertilizer;
            // Second fertilizer
            ctx.Player.Harvest.Box[1].OnAmountUpdate += this.UpdateSecondFertilizer;
            ctx.Player.Harvest.Box[1].OnFertilizerChange += this.UpdateSecondFertilizer;
            // Third fertilizer
            ctx.Player.Harvest.Box[2].OnAmountUpdate += this.UpdateThirdFertilizer;
            ctx.Player.Harvest.Box[2].OnFertilizerChange += this.UpdateThirdFertilizer;
            // Fourth fertilizer
            ctx.Player.Harvest.Box[3].OnAmountUpdate += this.UpdateFourthFertilizer;
            ctx.Player.Harvest.Box[3].OnFertilizerChange += this.UpdateFourthFertilizer;
        }

        private void UnhookHarvestBoxEvents() {
            // Hooks player location event
            ctx.Player.OnVillageEnter -= this.ShowHarvestBox;
            ctx.Player.OnVillageLeave -= this.HideHarvestBox;
            // Hook fertilizer and Harvest box events
            ctx.Player.Harvest.OnCounterChange -= this.UpdateHarvestBoxCounter;
            // First fertilizer
            ctx.Player.Harvest.Box[0].OnAmountUpdate -= this.UpdateFirstFertilizer;
            ctx.Player.Harvest.Box[0].OnFertilizerChange -= this.UpdateFirstFertilizer;
            // Second fertilizer
            ctx.Player.Harvest.Box[1].OnAmountUpdate -= this.UpdateSecondFertilizer;
            ctx.Player.Harvest.Box[1].OnFertilizerChange -= this.UpdateSecondFertilizer;
            // Third fertilizer
            ctx.Player.Harvest.Box[2].OnAmountUpdate -= this.UpdateThirdFertilizer;
            ctx.Player.Harvest.Box[2].OnFertilizerChange -= this.UpdateThirdFertilizer;
            // Fourth fertilizer
            ctx.Player.Harvest.Box[3].OnAmountUpdate -= this.UpdateFourthFertilizer;
            ctx.Player.Harvest.Box[3].OnFertilizerChange -= this.UpdateFourthFertilizer;
        }

        private void HookMonsterEvents() {
            // First monster
            ctx.FirstMonster.OnMonsterSpawn += this.OnFirstMonsterSpawn;
            ctx.FirstMonster.OnMonsterDespawn += this.OnFirstMonsterDespawn;
            ctx.FirstMonster.OnMonsterDeath += this.OnFirstMonsterDespawn;
            ctx.FirstMonster.OnHPUpdate += this.UpdateFirstMonster;

            // Second monster
            ctx.SecondMonster.OnMonsterSpawn += this.OnSecondMonsterSpawn;
            ctx.SecondMonster.OnMonsterDespawn += this.OnSecondMonsterDespawn;
            ctx.SecondMonster.OnMonsterDeath += this.OnSecondMonsterDespawn;
            ctx.SecondMonster.OnHPUpdate += this.UpdateSecondMonster;

            // Third monster
            ctx.ThirdMonster.OnMonsterSpawn += this.OnThirdMonsterSpawn;
            ctx.ThirdMonster.OnMonsterDespawn += this.OnThirdMonsterDespawn;
            ctx.ThirdMonster.OnMonsterDespawn += this.OnThirdMonsterDespawn;
            ctx.ThirdMonster.OnHPUpdate += this.UpdateThirdMonster;
        }

        private void UnhookMonsterEvents() {
            // First monster
            ctx.FirstMonster.OnMonsterSpawn -= this.OnFirstMonsterSpawn;
            ctx.FirstMonster.OnMonsterDespawn -= this.OnFirstMonsterDespawn;
            ctx.FirstMonster.OnMonsterDeath -= this.OnFirstMonsterDespawn;
            ctx.FirstMonster.OnHPUpdate -= this.UpdateFirstMonster;

            // Second monster
            ctx.SecondMonster.OnMonsterSpawn -= this.OnSecondMonsterSpawn;
            ctx.SecondMonster.OnMonsterDespawn -= this.OnSecondMonsterDespawn;
            ctx.SecondMonster.OnMonsterDeath -= this.OnSecondMonsterDespawn;
            ctx.SecondMonster.OnHPUpdate -= this.UpdateSecondMonster;

            // Third monster
            ctx.ThirdMonster.OnMonsterSpawn -= this.OnThirdMonsterSpawn;
            ctx.ThirdMonster.OnMonsterDespawn -= this.OnThirdMonsterDespawn;
            ctx.ThirdMonster.OnMonsterDeath -= this.OnThirdMonsterDespawn;
            ctx.ThirdMonster.OnHPUpdate -= this.UpdateThirdMonster;
        }

        private void HookMantleEvents() {
            // Primary mantle
            ctx.Player.PrimaryMantle.OnMantleTimerUpdate += this.UpdatePrimaryMantleTimer;
            ctx.Player.PrimaryMantle.OnMantleCooldownUpdate += this.UpdatePrimaryMantleCooldown;

            // Secondary mantle
            ctx.Player.SecondaryMantle.OnMantleTimerUpdate += this.UpdateSecondaryMantleTimer;
            ctx.Player.SecondaryMantle.OnMantleCooldownUpdate += this.UpdateSecondaryMantleCooldown;
        }

        private void UnhookMantleEvents() {
            // Primary mantle
            ctx.Player.PrimaryMantle.OnMantleTimerUpdate -= this.UpdatePrimaryMantleTimer;
            ctx.Player.PrimaryMantle.OnMantleCooldownUpdate -= this.UpdatePrimaryMantleCooldown;

            // Secondary mantle
            ctx.Player.SecondaryMantle.OnMantleTimerUpdate -= this.UpdateSecondaryMantleTimer;
            ctx.Player.SecondaryMantle.OnMantleCooldownUpdate -= this.UpdateSecondaryMantleCooldown;
        }

        private void makeOverlayClickThrough() {
            // flags to make overlay click-through
            int WS_EX_TRANSPARENT = 0x20;
            int GWL_EXSTYLE = (-20);

            var wnd = GetWindow(this);
            IntPtr hwnd = new WindowInteropHelper(wnd).EnsureHandle();
            // Get overlay flags
            int Styles = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, Styles | WS_EX_TRANSPARENT);
        }

        public void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, function);
        } 

        private void SetOverlaySize() {
            OverlayWnd.Width = w_Width * 2;
            OverlayWnd.Height = w_Height;
            OverlayGrid.Width = OverlayWnd.Width;
            OverlayGrid.Height = OverlayWnd.Height;
        }

        public void ChangeMonsterComponentPosition(object source, EventArgs e) {
            bool ContainerEnabled = UserSettings.PlayerConfig.Overlay.MonstersComponent.Enabled;
            double X = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0];
            double Y = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1];
            double Left = MonstersContainer.Margin.Left;
            double Top = MonstersContainer.Margin.Top;
            double Right = MonstersContainer.Margin.Right;
            double Bottom = MonstersContainer.Margin.Bottom;
            Dispatch(() => {
                if (X != Left || Y != Top) {
                    MonstersContainer.Margin = new Thickness(X, Y, Right, Bottom);
                }
                if (ContainerEnabled) {
                    MonstersContainer.Visibility = Visibility.Visible;
                } else {
                    MonstersContainer.Visibility = Visibility.Hidden;
                }
            });
            
            //Debugger.Warn($"Changed Monster component position to X:{X} Y:{Y}");
        }

        /* Harvest box */
        public void ShowHarvestBox(object source, EventArgs e) {
            if (HarvestBoxComponent.Visibility == Visibility.Visible || !UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled) return;
            Dispatch(() => {
                HarvestBoxComponent.Visibility = Visibility.Visible;
            });
        }

        public void HideHarvestBox(object source, EventArgs e) {
            if (HarvestBoxComponent.Visibility == Visibility.Hidden) return;
            Dispatch(() => {
                HarvestBoxComponent.Visibility = Visibility.Hidden;
            });
        }

        /* Mantles */
        // Primary mantle
        public void UpdatePrimaryMantleCooldown(object source, MantleEventArgs e) {
            bool ContainerEnabled = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Enabled;
            if (e.Cooldown == 0) {
                Dispatch(() => {
                    if (PrimaryMantleContainer.Visibility == Visibility.Visible) PrimaryMantleContainer.Visibility = Visibility.Hidden;
                });
                return;
            }
            Dispatch(() => {
                if (ContainerEnabled && PrimaryMantleContainer.Visibility == Visibility.Hidden) PrimaryMantleContainer.Visibility = Visibility.Visible;
                string FormatMantleName = $"({(int)e.Cooldown}) {e.Name.ToUpper()}";
                PrimaryMantleName.Content = FormatMantleName;
                PrimaryMantleTimer.Slice = e.Cooldown / e.staticCooldown;
            });
        }

        public void UpdatePrimaryMantleTimer(object source, MantleEventArgs e) {
            bool ContainerEnabled = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Enabled;
            if (e.Timer == 0) {
                Dispatch(() => {
                    if (PrimaryMantleContainer.Visibility == Visibility.Visible) PrimaryMantleContainer.Visibility = Visibility.Hidden;
                });
                return;
            }
            Dispatch(() => {
                if (ContainerEnabled && PrimaryMantleContainer.Visibility == Visibility.Hidden) PrimaryMantleContainer.Visibility = Visibility.Visible;
                string FormatMantleName = $"({(int)e.Timer}) {e.Name.ToUpper()}";
                PrimaryMantleName.Content = FormatMantleName;
                PrimaryMantleTimer.Slice = e.Timer / e.staticTimer;
            });
        }

        // Secondary mantle
        public void UpdateSecondaryMantleCooldown(object source, MantleEventArgs e) {
            bool ContainerEnabled = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Enabled;
            if (e.Cooldown == 0) {
                Dispatch(() => {
                    if (SecondaryMantleContainer.Visibility == Visibility.Visible) SecondaryMantleContainer.Visibility = Visibility.Hidden;
                });
                return;
            }
            Dispatch(() => {
                if (ContainerEnabled && SecondaryMantleContainer.Visibility == Visibility.Hidden) SecondaryMantleContainer.Visibility = Visibility.Visible;
                string FormatMantleName = $"({(int)e.Cooldown}) {e.Name.ToUpper()}";
                SecondaryMantleName.Content = FormatMantleName;
                SecondaryMantleTimer.Slice = e.Cooldown / e.staticCooldown;
            });
        }

        public void UpdateSecondaryMantleTimer(object source, MantleEventArgs e) {
            bool ContainerEnabled = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Enabled;
            if (e.Timer == 0) {
                Dispatch(() => {
                    if (SecondaryMantleContainer.Visibility == Visibility.Visible) SecondaryMantleContainer.Visibility = Visibility.Hidden;
                });
                return;
            }
            Dispatch(() => {
                if (ContainerEnabled && SecondaryMantleContainer.Visibility == Visibility.Hidden) SecondaryMantleContainer.Visibility = Visibility.Visible;
                string FormatMantleName = $"({(int)e.Timer}) {e.Name.ToUpper()}";
                SecondaryMantleName.Content = FormatMantleName;
                SecondaryMantleTimer.Slice = e.Timer / e.staticTimer;
            });
        }

        /*  Monsters  */
        // First monster
        public void OnFirstMonsterSpawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                fMonsterBox.Visibility = Visibility.Visible;
                fMonsterName.Content = e.Name.ToUpper();
                fMonsterHpBar.Maximum = e.TotalHP;
                fMonsterHpBar.Value = e.CurrentHP;
                fMonsterHpText.Content = $"{e.CurrentHP}/{e.TotalHP} ({(e.CurrentHP / e.TotalHP) * 100:F2}%)";
            });
        }

        public void OnFirstMonsterDespawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                fMonsterBox.Visibility = Visibility.Hidden;
            });
        }

        public void UpdateFirstMonster(object source, MonsterEventArgs e) {
            if (e.Name == null) return;
            Dispatch(() => {
                fMonsterName.Content = e.Name.ToUpper();
                fMonsterHpBar.Maximum = e.TotalHP;
                fMonsterHpBar.Value = e.CurrentHP;
                fMonsterHpText.Content = $"{e.CurrentHP}/{e.TotalHP} ({(e.CurrentHP / e.TotalHP) * 100:F2}%)";
            });
        }

        // Second monster
        public void OnSecondMonsterSpawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                sMonsterBox.Visibility = Visibility.Visible;
                sMonsterName.Content = e.Name.ToUpper();
                sMonsterHpBar.Maximum = e.TotalHP;
                sMonsterHpBar.Value = e.CurrentHP;
                sMonsterHpText.Content = $"{e.CurrentHP}/{e.TotalHP} ({(e.CurrentHP / e.TotalHP) * 100:F2}%)";
            });
        }

        public void OnSecondMonsterDespawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                sMonsterBox.Visibility = Visibility.Hidden;
            });
        }

        public void UpdateSecondMonster(object source, MonsterEventArgs e) {
            if (e.Name == null) return;
            Dispatch(() => {
                sMonsterName.Content = e.Name.ToUpper();
                sMonsterHpBar.Maximum = e.TotalHP;
                sMonsterHpBar.Value = e.CurrentHP;
                sMonsterHpText.Content = $"{e.CurrentHP}/{e.TotalHP} ({(e.CurrentHP / e.TotalHP) * 100:F2}%)";
            });
        }

        // Third monster

        public void OnThirdMonsterSpawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                tMonsterBox.Visibility = Visibility.Visible;
                tMonsterName.Content = e.Name.ToUpper();
                tMonsterHpBar.Maximum = e.TotalHP;
                tMonsterHpBar.Value = e.CurrentHP;
                tMonsterHpText.Content = $"{e.CurrentHP}/{e.TotalHP} ({(e.CurrentHP / e.TotalHP) * 100:F2}%)";
            });
        }

        public void OnThirdMonsterDespawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                tMonsterBox.Visibility = Visibility.Hidden;
            });
        }

        public void UpdateThirdMonster(object source, MonsterEventArgs e) {
            if (e.Name == null) return;
            Dispatch(() => {
                tMonsterName.Content = e.Name.ToUpper();
                tMonsterHpBar.Maximum = e.TotalHP;
                tMonsterHpBar.Value = e.CurrentHP;
                tMonsterHpText.Content = $"{e.CurrentHP}/{e.TotalHP} ({(e.CurrentHP / e.TotalHP) * 100:F2}%)";
            });
        }

        /* Positions and enable/disable components */

        public void ToggleOverlay(object source, EventArgs e) {
            Dispatch(() => {
                this.Visibility = UserSettings.PlayerConfig.Overlay.Enabled ? Visibility.Visible : Visibility.Hidden;
            });
        }

        public void ChangePrimaryMantlePosition(object source, EventArgs e) {
            double X = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[0];
            double Y = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[1];
            double Left = PrimaryMantleContainer.Margin.Left;
            double Top = PrimaryMantleContainer.Margin.Top;
            double Right = PrimaryMantleContainer.Margin.Right;
            double Bottom = PrimaryMantleContainer.Margin.Bottom;
            Dispatch(() => {
                if (X != Left || Y != Top) {
                    PrimaryMantleContainer.Margin = new Thickness(X, Y, Right, Bottom);
                }
                if (PrimaryMantleContainer.IsVisible && !UserSettings.PlayerConfig.Overlay.PrimaryMantle.Enabled) {
                    PrimaryMantleContainer.Visibility = Visibility.Hidden;
                } else if (UserSettings.PlayerConfig.Overlay.PrimaryMantle.Enabled) {
                    if (ctx.Player.PrimaryMantle.Timer > 0 || ctx.Player.PrimaryMantle.Cooldown > 0) {
                        PrimaryMantleContainer.Visibility = Visibility.Visible;
                    } else {
                        PrimaryMantleContainer.Visibility = Visibility.Hidden;
                    }
                }
            });
        }

        public void ChangeHarvestBoxPosition(object source, EventArgs e) {
            double X = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[0];
            double Y = UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Position[1];
            double Left = HarvestBoxComponent.Margin.Left;
            double Top = HarvestBoxComponent.Margin.Top;
            double Right = HarvestBoxComponent.Margin.Right;
            double Bottom = HarvestBoxComponent.Margin.Bottom;
            Dispatch(() => {
                if (X != Left || Y != Top) {
                    HarvestBoxComponent.Margin = new Thickness(X, Y, Right, Bottom);
                }
                if (HarvestBoxComponent.IsVisible && !UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled) {
                    HarvestBoxComponent.Visibility = Visibility.Hidden;
                } else if (UserSettings.PlayerConfig.Overlay.HarvestBoxComponent.Enabled) {
                    if (ctx.Player.inHarvestZone) {
                        HarvestBoxComponent.Visibility = Visibility.Visible;
                    } else {
                        HarvestBoxComponent.Visibility = Visibility.Hidden;
                    }
                }
            });
        }

        public void ShowHarvestBoxContainer() {
            if (!HarvestBoxComponent.IsVisible) {
                HarvestBoxComponent.Visibility = Visibility.Visible;
            }
        }

        public void HideHarvestBoxContainer() {
            if (HarvestBoxComponent.IsVisible) {
                HarvestBoxComponent.Visibility = Visibility.Hidden;
            }
        }

        public void UpdateFirstFertilizer(object source, FertilizerEventArgs e) {
            Dispatch(() => {
                fert1Name.Content = e.Name;
                fert1Counter.Content = $"x{e.Amount}";
            });
        }

        public void UpdateSecondFertilizer(object source, FertilizerEventArgs e) {
            Dispatch(() => {
                fert2Name.Content = e.Name;
                fert2Counter.Content = $"x{e.Amount}";
            }); ;
        }

        public void UpdateThirdFertilizer(object source, FertilizerEventArgs e) {
            Dispatch(() => {
                fert3Name.Content = e.Name;
                fert3Counter.Content = $"x{e.Amount}";
            });
        }

        public void UpdateFourthFertilizer(object source, FertilizerEventArgs e) {
            Dispatch(() => {
                fert4Name.Content = e.Name;
                fert4Counter.Content = $"x{e.Amount}";
            });
        }

        public void UpdateHarvestBoxCounter(object source, HarvestBoxEventArgs e) {
            Dispatch(() => {
                HarvestBoxItemsCounter.Content = $"{e.Counter}/{e.Max}";
            });
        }

        private RadialGradientBrush DonutBrush(Color customColor) {
            RadialGradientBrush brush = new RadialGradientBrush();
            brush.Center = new Point(13, 13);
            brush.GradientOrigin = new Point(13, 13);
            brush.MappingMode = BrushMappingMode.Absolute;
            brush.RadiusX = 13;
            brush.RadiusY = 13;
            // Add the colors to make a donut
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.4));
            brush.GradientStops.Add(new GradientStop(customColor, 0.4));
            return brush;
        }

        public void ChangePrimaryMantleColor(object source, EventArgs e) {
            string newColor = UserSettings.PlayerConfig.Overlay.PrimaryMantle.Color;
            if (PrimaryMantleTimer.Fill.ToString() == newColor) {
                return;
            }
            Color primaryColor = (Color)ColorConverter.ConvertFromString(newColor);
            Brush primaryColorBrush = new SolidColorBrush(primaryColor);
            Dispatch(() => {
                PrimaryMantleTimer.Fill = DonutBrush(primaryColor);
                PrimaryMantleBorder.BorderBrush = primaryColorBrush;
            });
        }

        public void ChangeSecondaryMantlePosition(object source, EventArgs e) {
            double X = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[0];
            double Y = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[1];
            double Left = SecondaryMantleContainer.Margin.Left;
            double Top = SecondaryMantleContainer.Margin.Top;
            double Right = SecondaryMantleContainer.Margin.Right;
            double Bottom = SecondaryMantleContainer.Margin.Bottom;
            Dispatch(() => {
                if (X != Left || Y != Top) {
                    SecondaryMantleContainer.Margin = new Thickness(X, Y, Right, Bottom);
                }
                if (SecondaryMantleContainer.IsVisible && !UserSettings.PlayerConfig.Overlay.SecondaryMantle.Enabled) {
                    SecondaryMantleContainer.Visibility = Visibility.Hidden;
                } else if (UserSettings.PlayerConfig.Overlay.SecondaryMantle.Enabled) {
                    if (ctx.Player.SecondaryMantle.Timer > 0 || ctx.Player.SecondaryMantle.Cooldown > 0) {
                        SecondaryMantleContainer.Visibility = Visibility.Visible;
                    } else {
                        SecondaryMantleContainer.Visibility = Visibility.Hidden;
                    }
                }

            });
        }

        public void ChangeSecondaryMantleColor(object source, EventArgs e) {
            string newColor = UserSettings.PlayerConfig.Overlay.SecondaryMantle.Color;
            if (SecondaryMantleTimer.Fill.ToString() == newColor) {
                return;
            }
            Color secondaryColor = (Color)ColorConverter.ConvertFromString(newColor);
            Brush secondaryColorBrush = new SolidColorBrush(secondaryColor);
            Dispatch(() => {
                SecondaryMantleTimer.Fill = DonutBrush(secondaryColor);
                SecondaryMantleBorder.BorderBrush = secondaryColorBrush;
            });
        }
    }
}
