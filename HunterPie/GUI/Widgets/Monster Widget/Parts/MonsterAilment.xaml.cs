using System;
using System.Windows.Controls;
using Timer = System.Threading.Timer;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Monster_Widget.Parts {
    /// <summary>
    /// Interaction logic for MonsterAilment.xaml
    /// </summary>
    public partial class MonsterAilment : UserControl {
        Ailment Context;
        Timer VisibilityTimer;

        public MonsterAilment() {
            InitializeComponent();
        }

        public void SetContext(Ailment ctx, double MaxBarSize) {
            Context = ctx;
            SetAilmentInformation(MaxBarSize);
            HookEvents();
            StartVisibilityTimer();
        }

        private void HookEvents() {
            Context.OnBuildupChange += OnBuildupChange;
            Context.OnDurationChange += OnDurationChange;
            Context.OnCounterChange += OnCounterChange;
        }

        public void UnhookEvents() {
            Context.OnBuildupChange -= OnBuildupChange;
            Context.OnDurationChange -= OnDurationChange;
            Context.OnCounterChange -= OnCounterChange;
            VisibilityTimer?.Dispose();
            Context = null;
        }

        #region Visibility timer
        private void StartVisibilityTimer() {
            if (!UserSettings.PlayerConfig.Overlay.MonstersComponent.HidePartsAfterSeconds) {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                    this.Visibility = System.Windows.Visibility.Visible;
                }));
                return;
            }
            if (VisibilityTimer == null) {
                VisibilityTimer = new Timer(_ => HideUnactiveBar(), null, 10, 0);
            } else {
                VisibilityTimer.Change(UserSettings.PlayerConfig.Overlay.MonstersComponent.SecondsToHideParts * 1000, 0);
            }
        }

        private void HideUnactiveBar() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.Visibility = System.Windows.Visibility.Collapsed;
            }));
        }

        #endregion

        private void SetAilmentInformation(double NewSize) {
            this.AilmentName.Text = $"{Context.Name}";
            this.AilmentBar.MaxSize = NewSize - 37;
            this.AilmentCounter.Text = $"{Context.Counter}";
            if (Context.Duration > 0) {
                AilmentBar.MaxHealth = Context.MaxDuration;
                AilmentBar.Health = Context.Duration;
            } else {
                AilmentBar.MaxHealth = Context.MaxBuildup;
                AilmentBar.Health = Context.MaxBuildup - Context.Buildup;
            }
            AilmentText.Text = $"{AilmentBar.Health:0}/{AilmentBar.MaxHealth:0}";
        }

        private void OnCounterChange(object source, MonsterAilmentEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.AilmentCounter.Text = args.Counter.ToString();
            }));
        }

        private void OnDurationChange(object source, MonsterAilmentEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                AilmentBar.MaxHealth = args.MaxDuration;
                AilmentBar.Health = args.MaxDuration - args.Duration;
                AilmentText.Text = $"{AilmentBar.Health:0}/{AilmentBar.MaxHealth:0}";
                StartVisibilityTimer();
            }));
        }

        private void OnBuildupChange(object source, MonsterAilmentEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                AilmentBar.MaxHealth = args.MaxBuildup;
                AilmentBar.Health = args.MaxBuildup - args.Buildup;
                AilmentText.Text = $"{AilmentBar.Health:0}/{AilmentBar.MaxHealth:0}";
                StartVisibilityTimer();
            }));
        }

        public void UpdateBarSize(double NewSize) {
            if (this.Context == null) return;
            this.AilmentBar.MaxSize = NewSize - 37;
            if (Context.Duration > 0) {
                AilmentBar.MaxHealth = Context.MaxDuration;
                AilmentBar.Health = Context.Duration;
            } else {
                AilmentBar.MaxHealth = Context.MaxBuildup;
                AilmentBar.Health = Context.MaxBuildup - Context.Buildup;
            }
            AilmentText.Text = $"{AilmentBar.Health:0}/{AilmentBar.MaxHealth:0}";
        }
    }
}
