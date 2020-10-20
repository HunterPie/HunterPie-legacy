using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;
using Timer = System.Threading.Timer;
using HunterPie.Core.Events;
using static HunterPie.Core.UserSettings.Config;

namespace HunterPie.GUI.Widgets.Monster_Widget.Parts
{
    /// <summary>
    /// Interaction logic for MonsterAilment.xaml
    /// </summary>
    public partial class MonsterAilment : UserControl
    {
        Ailment Context;
        Timer VisibilityTimer;
        bool IsGroupEnabled;
        Monsterscomponent ComponentSettings => UserSettings.PlayerConfig.Overlay.MonstersComponent;

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
            IsGroupEnabled = ComponentSettings.EnabledAilmentGroups.Contains(Context.Group);
            AilmentGroupColor = ComponentSettings.EnableAilmentsBarColor ?
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
            bool isHidden = ComponentSettings.HideAilmentsAfterSeconds;
            bool enabled = ComponentSettings.EnableMonsterAilments;

            bool visible = !isHidden && enabled && IsGroupEnabled;
            Dispatch(() =>
            {
                Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                AilmentGroupColor = ComponentSettings.EnableAilmentsBarColor ?
                    FindResource($"MONSTER_AILMENT_COLOR_{Context.Group}") as Brush :
                    FindResource("MONSTER_AILMENT_COLOR_UNKNOWN") as Brush;
                string format = Context.Duration > 0 ? ComponentSettings.AilmentTimerTextFormat :
                ComponentSettings.AilmentBuildupTextFormat;
                AilmentText.Text = FormatAilmentString(format, AilmentBar.Value, AilmentBar.MaxValue);
            });
        }

        #endregion

        #region Visibility timer
        private void StartVisibilityTimer()
        {
            if (!ComponentSettings.HideAilmentsAfterSeconds)
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
                VisibilityTimer.Change(ComponentSettings.SecondsToHideParts * 1000, 0);
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
            bool visible = ComponentSettings.EnableMonsterAilments;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                AilmentCounter.Text = args.Counter.ToString();
                Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                StartVisibilityTimer();
            }));
        }

        private void OnDurationChange(object source, MonsterAilmentEventArgs args)
        {
            if (args.MaxDuration <= 0) return;

            bool enabled = ComponentSettings.EnableMonsterAilments;
            bool visible = enabled && IsGroupEnabled;

            if (args.Duration > 0)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                {
                    AilmentBar.MaxValue = args.MaxDuration;
                    AilmentBar.Value = Math.Max(0, args.MaxDuration - args.Duration);
                    AilmentText.Text = FormatAilmentString(ComponentSettings.AilmentTimerTextFormat, AilmentBar.Value, AilmentBar.MaxValue); ;
                    Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                    StartVisibilityTimer();
                }));
            }
            else { OnBuildupChange(source, args); } //ensures we switch back to showing buildup
        }

        private void OnBuildupChange(object source, MonsterAilmentEventArgs args)
        {
            if (args.MaxBuildup <= 0) return;

            bool enabled = ComponentSettings.EnableMonsterAilments;
            bool visible = enabled && IsGroupEnabled;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                AilmentBar.MaxValue = Math.Max(1, args.MaxBuildup);
                // Get the min between them so the buildup doesnt overflow
                AilmentBar.Value = Math.Min(args.Buildup, args.MaxBuildup);
                AilmentText.Text = FormatAilmentString(ComponentSettings.AilmentBuildupTextFormat, AilmentBar.Value, AilmentBar.MaxValue);
                Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
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
            string format = Context.Duration > 0 ? ComponentSettings.AilmentTimerTextFormat :
                ComponentSettings.AilmentBuildupTextFormat;
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
