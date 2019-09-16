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
using System.Windows.Shapes;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit;

namespace HunterPie.GUI {
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class Overlay : Window {

        double w_Height = Screen.PrimaryScreen.Bounds.Height;
        double w_Width = Screen.PrimaryScreen.Bounds.Width;

        public Overlay() {
            InitializeComponent();
            SetOverlaySize();
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
            Debugger.Warn($"Changed Monster component position to X:{X} Y:{Y}");
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
            Debugger.Warn($"Changed primary mantle position to X: {X} Y:{Y}");
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

        public void ChangePrimaryMantleColor(string newColor) {
            if (PrimaryMantleTimer.Fill.ToString() == newColor) {
                return;
            }
            Color primaryColor = (Color)ColorConverter.ConvertFromString(newColor);
            Brush primaryColorBrush = new SolidColorBrush(primaryColor);
            PrimaryMantleTimer.Fill = primaryColorBrush;
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
            Debugger.Warn($"Changed primary mantle position to X: {X} Y:{Y}");
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
            SecondaryMantleTimer.Fill = secondaryColorBrush;
            SecondaryMantleBorder.BorderBrush = secondaryColorBrush;
        }

        public void HideMonster(StackPanel Monster) {
            Monster.Visibility = Visibility.Hidden;
        }

        public void ShowMonster(StackPanel Monster) {
            Monster.Visibility = Visibility.Visible;
        }

        private string FormatHPText(float[] HP) {
            return $"{HP[0]}/{HP[1]} ({(HP[0] / HP[1]) * 100:F2}%)";
        }

        public void UpdateFirstMonsterInformation(float[] HP, string Name) {
            fMonsterHpBar.Value = (int)HP[0];
            fMonsterHpBar.Maximum = (int)HP[1];
            fMonsterName.Content = Name.ToUpper();
            string HPText = FormatHPText(HP);
            fMonsterHpText.Content = HPText;
        }

        public void UpdateSecondMonsterInformation(float[] HP, string Name) {
            sMonsterHpBar.Value = (int)HP[0];
            sMonsterHpBar.Maximum = (int)HP[1];
            sMonsterName.Content = Name.ToUpper();
            string HPText = FormatHPText(HP);
            sMonsterHpText.Content = HPText;
        }

        public void UpdateThirdMonsterInformation(float[] HP, string Name) {
            tMonsterHpBar.Value = (int)HP[0];
            tMonsterHpBar.Maximum = (int)HP[1];
            tMonsterName.Content = Name.ToUpper();
            string HPText = FormatHPText(HP);
            tMonsterHpText.Content = HPText;
        }

        public void HideMonstersContainer() {
            if (MonstersContainer.IsVisible) {
                MonstersContainer.Visibility = Visibility.Hidden;
            }
        }

        public void ShowMonstersContainer() {
            if (!MonstersContainer.IsVisible) {
                MonstersContainer.Visibility = Visibility.Visible;
            }
        }

        public void HideEverything() {
            HideMonstersContainer();
        }
    }
}
