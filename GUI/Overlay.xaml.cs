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
            OverlayWnd.Width = w_Width;
            OverlayWnd.Height = w_Height;
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
