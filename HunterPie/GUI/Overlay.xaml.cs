using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit;
using HunterPie.Core;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace HunterPie.GUI {
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window {

        double w_Height = Screen.PrimaryScreen.Bounds.Height;
        double w_Width = Screen.PrimaryScreen.Bounds.Width;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int style);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public Overlay() {
            InitializeComponent();
            SetOverlaySize();
            makeOverlayClickThrough();
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

        public void Dispatch(Action todo) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, todo);
        } 

        public void HideOverlay() {
            if (this.IsVisible) {
                this.Hide();
            }
        }

        public void ShowOverlay() {
            if (!this.IsVisible) {
                this.Show();
            }
        }

        private void SetOverlaySize() {
            OverlayWnd.Width = w_Width * 2;
            OverlayWnd.Height = w_Height;
            OverlayGrid.Width = OverlayWnd.Width;
            OverlayGrid.Height = OverlayWnd.Height;
        }

        public void ChangeMonsterComponentPosition(double X, double Y) {
            double Left = MonstersContainer.Margin.Left;
            double Top = MonstersContainer.Margin.Top;
            if (X == Left && Y == Top) {
                return;
            }
            double Right = MonstersContainer.Margin.Right;
            double Bottom = MonstersContainer.Margin.Bottom;
            MonstersContainer.Margin = new Thickness(X, Y, Right, Bottom);
            //Debugger.Warn($"Changed Monster component position to X:{X} Y:{Y}");
        }

        // First monster
        public void OnFirstMonsterSpawn(object source, MonsterEventArgs e) {
            Dispatch(() => {
                fMonsterBox.Visibility = Visibility.Visible;
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

        public void ChangePrimaryMantlePosition(double X, double Y) {
            double Left = PrimaryMantleContainer.Margin.Left;
            double Top = PrimaryMantleContainer.Margin.Top;
            if (X == Left && Y == Top) {
                return;
            }
            double Right = PrimaryMantleContainer.Margin.Right;
            double Bottom = PrimaryMantleContainer.Margin.Bottom;
            PrimaryMantleContainer.Margin = new Thickness(X, Y, Right, Bottom);
            //Debugger.Warn($"Changed primary mantle position to X: {X} Y:{Y}");
        }

        public void UpdatePrimaryMantleTimer(double percentage) {
            if (percentage == PrimaryMantleTimer.Slice) {
                return;
            }
            PrimaryMantleTimer.Slice = percentage;
        }

        public void UpdatePrimaryMantleText(string newText) {
            if (newText == PrimaryMantleName.Content.ToString()) {
                return;
            }
            PrimaryMantleName.Content = newText;
        }

        public void HidePrimaryMantle() {
            if (PrimaryMantleContainer.IsVisible) {
                PrimaryMantleContainer.Visibility = Visibility.Hidden;
            }
        }

        public void ShowPrimaryMantle() {
            if (!PrimaryMantleContainer.IsVisible) {
                PrimaryMantleContainer.Visibility = Visibility.Visible;
            }
        }

        public void ChangeHarvestBoxPosition(double X, double Y) {
            double Left = HarvestBoxComponent.Margin.Left;
            double Top = HarvestBoxComponent.Margin.Top;
            if (X == Left && Y == Top) {
                return;
            }
            double Right = HarvestBoxComponent.Margin.Right;
            double Bottom = HarvestBoxComponent.Margin.Bottom;
            HarvestBoxComponent.Margin = new Thickness(X, Y, Right, Bottom);
            //Debugger.Warn($"Changed harvest box position to X: {X} Y:{Y}");
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

        public void UpdateFirstFertilizer(string Name, int Amount) {
            fert1Name.Content = Name;
            fert1Counter.Content = $"x{Amount}";
        }

        public void UpdateSecondFertilizer(string Name, int Amount) {
            fert2Name.Content = Name;
            fert2Counter.Content = $"x{Amount}";
        }

        public void UpdateThirdFertilizer(string Name, int Amount) {
            fert3Name.Content = Name;
            fert3Counter.Content = $"x{Amount}";
        }

        public void UpdateFourthFertilizer(string Name, int Amount) {
            fert4Name.Content = Name;
            fert4Counter.Content = $"x{Amount}";
        }

        public void UpdateHarvestBoxCounter(int counter, int max) {
            HarvestBoxItemsCounter.Content = $"{counter}/{max}";
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

        public void ChangePrimaryMantleColor(string newColor) {
            if (PrimaryMantleTimer.Fill.ToString() == newColor) {
                return;
            }
            Color primaryColor = (Color)ColorConverter.ConvertFromString(newColor);
            Brush primaryColorBrush = new SolidColorBrush(primaryColor);
            
            PrimaryMantleTimer.Fill = DonutBrush(primaryColor);
            PrimaryMantleBorder.BorderBrush = primaryColorBrush;
        }

        public void ChangeSecondaryMantlePosition(double X, double Y) {
            double Left = SecondaryMantleContainer.Margin.Left;
            double Top = SecondaryMantleContainer.Margin.Top;
            if (X == Left && Y == Top) {
                return;
            }
            double Right = SecondaryMantleContainer.Margin.Right;
            double Bottom = SecondaryMantleContainer.Margin.Bottom;
            SecondaryMantleContainer.Margin = new Thickness(X, Y, Right, Bottom);
            //Debugger.Warn($"Changed primary mantle position to X: {X} Y:{Y}");
        }

        public void UpdateSecondaryMantleTimer(double percentage) {
            if (percentage == SecondaryMantleTimer.Slice) {
                return;
            }
            SecondaryMantleTimer.Slice = percentage;
        }

        public void UpdateSecondaryMantleText(string newText) {
            if (newText == SecondaryMantleName.Content.ToString()) {
                return;
            }
            SecondaryMantleName.Content = newText;
        }

        public void HideSecondaryMantle() {
            if (SecondaryMantleContainer.IsVisible) {
                SecondaryMantleContainer.Visibility = Visibility.Hidden;
            }
        }

        public void ShowSecondaryMantle() {
            if (!SecondaryMantleContainer.IsVisible) {
                SecondaryMantleContainer.Visibility = Visibility.Visible;
            }
        }

        public void ChangeSecondaryMantleColor(string newColor) {
            if (SecondaryMantleTimer.Fill.ToString() == newColor) {
                return;
            }
            Color secondaryColor = (Color)ColorConverter.ConvertFromString(newColor);
            Brush secondaryColorBrush = new SolidColorBrush(secondaryColor);

            SecondaryMantleTimer.Fill = DonutBrush(secondaryColor);
            SecondaryMantleBorder.BorderBrush = secondaryColorBrush;
        }

        public void HideEverything() {
            HideMonstersContainer();
        }
    }
}
