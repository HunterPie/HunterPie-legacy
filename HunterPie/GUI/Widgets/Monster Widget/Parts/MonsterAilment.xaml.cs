using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;
using Timer = System.Threading.Timer;

namespace HunterPie.GUI.Widgets.Monster_Widget.Parts
{
    /// <summary>
    /// Interaction logic for MonsterAilment.xaml
    /// </summary>
    public partial class MonsterAilment : UserControl
    {
        Ailment Context;
        Timer VisibilityTimer;
        bool IsAilmentGroupEnabled;

        public Brush AilmentGroupColor
        {
            get { return (Brush)GetValue(AilmentGroupColorProperty); }
            set { SetValue(AilmentGroupColorProperty, value); }
        }

        public static readonly DependencyProperty AilmentGroupColorProperty =
            DependencyProperty.Register("AilmentGroupColor", typeof(Brush), typeof(MonsterAilment));

        public MonsterAilment() => InitializeComponent();

        public void SetContext(Ailment ctx, double MaxBarSize)
        {
            Context = ctx;
            IsAilmentGroupEnabled = UserSettings.PlayerConfig.Overlay.MonstersComponent.EnabledAilmentGroups.Contains(Context.Group);
            AilmentGroupColor = UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableAilmentsBarColor ?
                    FindResource($"MONSTER_AILMENT_COLOR_{Context.Group}") as Brush :
                    FindResource("MONSTER_AILMENT_COLOR_UNKNOWN") as Brush;
            SetAilmentInformation(MaxBarSize);
            HookEvents();
            StartVisibilityTimer();
        }

        private void HookEvents()
        {
            Context.OnBuildupChange += OnBuildupChange;
            Context.OnDurationChange += OnDurationChange;
            Context.OnCounterChange += OnCounterChange;
        }

        public void UnhookEvents()
        {
            Context.OnBuildupChange -= OnBuildupChange;
            Context.OnDurationChange -= OnDurationChange;
            Context.OnCounterChange -= OnCounterChange;
            VisibilityTimer?.Dispose();
            Context = null;
        }

        private void Dispatch(Action f) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, f);

        #region Settings
        public void ApplySettings()
        {

            Visibility visibility;
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableMonsterAilments &&
                UserSettings.PlayerConfig.Overlay.MonstersComponent.EnabledAilmentGroups.Contains(Context.Group))
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.HidePartsAfterSeconds) visibility = Visibility.Collapsed;
            Dispatch(() => {
                Visibility = visibility;
                AilmentGroupColor = UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableAilmentsBarColor ?
                    FindResource($"MONSTER_AILMENT_COLOR_{Context.Group}") as Brush :
                    FindResource("MONSTER_AILMENT_COLOR_UNKNOWN") as Brush;
                string format = Context.Duration > 0 ? UserSettings.PlayerConfig.Overlay.MonstersComponent.AilmentTimerTextFormat :
                UserSettings.PlayerConfig.Overlay.MonstersComponent.AilmentBuildupTextFormat;
                AilmentText.Text = FormatAilmentString(format, AilmentBar.Value, AilmentBar.MaxValue);
            });
        }

        #endregion

        #region Visibility timer
        private void StartVisibilityTimer()
        {
            if (!UserSettings.PlayerConfig.Overlay.MonstersComponent.HidePartsAfterSeconds)
            {
                ApplySettings();
                return;
            }
            if (VisibilityTimer == null)
            {
                VisibilityTimer = new Timer(_ => HideUnactiveBar(), null, 10, 0);
            }
            else
            {
                VisibilityTimer.Change(UserSettings.PlayerConfig.Overlay.MonstersComponent.SecondsToHideParts * 1000, 0);
            }
        }

        private void HideUnactiveBar() => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            Visibility = Visibility.Collapsed;
        }));

        #endregion

        private void SetAilmentInformation(double NewSize)
        {
            AilmentName.Text = $"{Context.Name}";
            AilmentCounter.Text = $"{Context.Counter}";
            UpdateBarSize(NewSize);
        }

        private void OnCounterChange(object source, MonsterAilmentEventArgs args)
        {
            Visibility visibility;
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableMonsterAilments)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                AilmentCounter.Text = args.Counter.ToString();
                Visibility = visibility;
                StartVisibilityTimer();
            }));
        }

        private void OnDurationChange(object source, MonsterAilmentEventArgs args)
        {
            if (args.MaxDuration <= 0) { return; }
            Visibility visibility;
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableMonsterAilments && IsAilmentGroupEnabled)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }
            if (args.Duration > 0)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                {
                    AilmentBar.MaxValue = args.MaxDuration;
                    AilmentBar.Value = Math.Max(0, args.MaxDuration - args.Duration);
                    AilmentText.Text = FormatAilmentString(UserSettings.PlayerConfig.Overlay.MonstersComponent.AilmentTimerTextFormat, AilmentBar.Value, AilmentBar.MaxValue); ;
                    Visibility = visibility;
                    StartVisibilityTimer();
                }));
            }
            else { OnBuildupChange(source, args); } //ensures we switch back to showing buildup
        }

        private void OnBuildupChange(object source, MonsterAilmentEventArgs args)
        {
            if (args.MaxBuildup <= 0) { return; }
            Visibility visibility;
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableMonsterAilments && IsAilmentGroupEnabled)
            {
                visibility = Visibility.Visible;
            }
            else
            {
                visibility = Visibility.Collapsed;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                AilmentBar.MaxValue = Math.Max(1, args.MaxBuildup);
                // Get the min between them so the buildup doesnt overflow
                AilmentBar.Value = Math.Min(args.Buildup, args.MaxBuildup);
                AilmentText.Text = FormatAilmentString(UserSettings.PlayerConfig.Overlay.MonstersComponent.AilmentBuildupTextFormat, AilmentBar.Value, AilmentBar.MaxValue);
                Visibility = visibility;
                StartVisibilityTimer();
            }));
        }

        public void UpdateBarSize(double NewSize)
        {
            if (Context == null) return;
            AilmentBar.MaxSize = NewSize - 37;
            if (Context.Duration > 0)
            {
                AilmentBar.MaxValue = Math.Max(1, Context.MaxDuration);
                AilmentBar.Value = Math.Max(0, Context.Duration);
            }
            else
            {
                AilmentBar.MaxValue = Math.Max(1, Context.MaxBuildup);
                AilmentBar.Value = Math.Max(0, Context.MaxBuildup - Context.Buildup);
            }
            string format = Context.Duration > 0 ? UserSettings.PlayerConfig.Overlay.MonstersComponent.AilmentTimerTextFormat :
                UserSettings.PlayerConfig.Overlay.MonstersComponent.AilmentBuildupTextFormat;
            AilmentText.Text = FormatAilmentString(format, AilmentBar.Value, AilmentBar.MaxValue);
        }

        public void UpdateSize(double NewSize)
        {
            AilmentBar.MaxSize = NewSize - 37;
            AilmentBar.MaxValue = AilmentBar.MaxValue;
            AilmentBar.Value = AilmentBar.Value;
        }

        private string FormatAilmentString(string format, double current, double max)
        {
            double percentage = current / max;
            return format.Replace("{Current}", $"{current:0}")
                .Replace("{Max}", $"{max:0}")
                .Replace("{Percentage}", $"{percentage * 100:0}");
        }
    }
}
