using System;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MantleTimer.xaml
    /// </summary>
    public partial class MantleTimer : UserControl {
        private Mantle Context;

        public MantleTimer() {
            InitializeComponent();
        }

        public void SetContext(Mantle ctx) {
            this.Context = ctx;
            HookEvents();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, function);
        }

        private void HookEvents() {
            Context.OnMantleCooldownUpdate += OnCooldownChange;
            Context.OnMantleTimerUpdate += OnTimerChange;
        }

        public void UnhookEvents() {
            Context.OnMantleCooldownUpdate -= OnCooldownChange;
            Context.OnMantleTimerUpdate -= OnTimerChange;
            Context = null;
        }

        private void OnTimerChange(object source, MantleEventArgs args) {
            if (args.Timer <= 0) {
                Dispatch(() => {
                    MantleContainer.Visibility = Visibility.Hidden;
                });
                return;
            }
            string FormatMantleName = $"({(int)args.Timer}) {args.Name.ToUpper()}";
            Dispatch(() => {
                if (this.IsVisible && !MantleContainer.IsVisible) MantleContainer.Visibility = Visibility.Visible;
                MantleName.Content = FormatMantleName;
                MantleCooldown.Slice = args.Timer / args.staticTimer;
            });
        }

        private void OnCooldownChange(object source, MantleEventArgs args) {
            if (args.Cooldown <= 0) {
                Dispatch(() => {
                    MantleContainer.Visibility = Visibility.Hidden;
                });
                return;
            }
            Dispatch(() => {
                if (this.IsVisible && !MantleContainer.IsVisible) MantleContainer.Visibility = Visibility.Visible;
                string FormatMantleName = $"({(int)args.Cooldown}) {args.Name.ToUpper()}";
                MantleName.Content = FormatMantleName;
                MantleCooldown.Slice = args.Cooldown / args.staticCooldown;
            });
        }
    }
}
