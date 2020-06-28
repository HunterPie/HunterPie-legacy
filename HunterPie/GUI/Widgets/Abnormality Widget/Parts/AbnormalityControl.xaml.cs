using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Abnormality_Widget.Parts
{
    /// <summary>
    /// Interaction logic for Abnormality.xaml
    /// </summary>
    public partial class AbnormalityControl : UserControl, IEquatable<AbnormalityControl>, IComparable<AbnormalityControl>
    {

        // TODO: Refactor this code

        readonly Brush Debuff_Color = new SolidColorBrush(Color.FromArgb(0xFF, 0x97, 0x32, 0x32));
        readonly Brush Buff_Color = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0x97, 0x45));

        public Abnormality Context { get; private set; }
        public bool ShowAbnormalityTimerText { get; set; }
        public byte AbnormalityTimerTextFormat { get; set; }
        public bool ShowAbnormalityName { get; set; }
        public double BaseWidth;
        public double BaseHeight;

        public AbnormalityControl() => InitializeComponent();

        public void Initialize(Abnormality Abnorm)
        {
            SetAbnormalityInfo(Abnorm);
            Context = Abnorm;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnAbnormalityUpdate += OnAbnormalityUpdate;
            Context.OnAbnormalityEnd += OnAbnormalityEnd;
            Context.OnStackChange += OnAbnormalityStackChange;
        }

        private void OnAbnormalityStackChange(object source, AbnormalityEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            ImageSource NewIcon = TryFindResource($"{args.Abnormality.Icon}" + string.Concat(Enumerable.Repeat("+", args.Abnormality.Stack))) as ImageSource;
            if (NewIcon == null) NewIcon = FindResource($"{args.Abnormality.Icon}") as ImageSource;
            AbnormalityIcon.Source = NewIcon;
        }));

        private void SetAbnormalityInfo(Abnormality Abnorm)
        {
            float angle;
            if (Abnorm.IsInfinite || Abnorm.MaxDuration == 0) angle = 90;
            else { angle = ConvertPercentageIntoAngle(Abnorm.Duration / (Abnorm.IsPercentageBuff ? Abnorm.MaxTimer : Abnorm.MaxDuration)); }
            AbnormalityDurationArc.EndAngle = angle;
            if (ShowAbnormalityName)
            {
                AbnormalityName.Text = Abnorm.Name;
                AbnormalityName.Visibility = System.Windows.Visibility.Visible;
            }
            ImageSource AbnormIcon;
            if (Abnorm.Stack >= 1)
            {
                AbnormIcon = TryFindResource($"{Abnorm.Icon}" + string.Concat(Enumerable.Repeat("+", Abnorm.Stack))) as ImageSource ?? FindResource($"{Abnorm.Icon}") as ImageSource;
            }
            else
            {
                AbnormIcon = FindResource(Abnorm.Icon) as ImageSource;
            }
            AbnormalityIcon.Source = AbnormIcon;
            AbnormalityDurationArc.Stroke = Abnorm.IsDebuff ? Debuff_Color : Buff_Color;
            if (Abnorm.IsInfinite || !ShowAbnormalityTimerText)
            {
                TimeLeftText.Visibility = System.Windows.Visibility.Collapsed;
                Height = 36;
            }
            else
            {
                TimeLeftText.Text = Abnorm.IsPercentageBuff ? $"{Abnorm.Duration / Abnorm.MaxTimer:P0}" : FormatToMinutes(Abnorm.Duration);
            }

        }

        private void OnAbnormalityEnd(object source, AbnormalityEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            args.Abnormality.OnAbnormalityUpdate -= OnAbnormalityUpdate;
            args.Abnormality.OnAbnormalityEnd -= OnAbnormalityEnd;
            args.Abnormality.OnStackChange -= OnAbnormalityStackChange;
            Context = null;
        }));

        private void OnAbnormalityUpdate(object source, AbnormalityEventArgs args)
        {
            float angle;
            if (args.Abnormality.IsInfinite || args.Abnormality.MaxDuration == 0) angle = 90;
            else { angle = ConvertPercentageIntoAngle(args.Abnormality.Duration / (args.Abnormality.IsPercentageBuff ? args.Abnormality.MaxTimer : args.Abnormality.MaxDuration)); }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                AbnormalityExpireWarning.Visibility = args.Abnormality.DurationPercentage <= 0.1 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                AbnormalityDurationArc.EndAngle = angle;
                if (args.Abnormality.IsInfinite || !ShowAbnormalityTimerText)
                {
                    TimeLeftText.Visibility = System.Windows.Visibility.Collapsed;
                    Height = 36;
                }
                else
                {
                    TimeLeftText.Text = args.Abnormality.IsPercentageBuff ? $"{args.Abnormality.Duration / args.Abnormality.MaxTimer:P0}" : FormatToMinutes(args.Abnormality.Duration);
                }
            }));
        }

        // Helper

        private float ConvertPercentageIntoAngle(float percentage)
        {
            float max = -269.999f;
            float angle = 90 - (360 * percentage);
            if (angle < max) angle = max;
            return angle;
        }

        private string FormatToMinutes(int seconds)
        {
            TimeSpan TotalSeconds = TimeSpan.FromSeconds(seconds);
            switch (AbnormalityTimerTextFormat)
            {
                case 0:
                    return TotalSeconds.TotalSeconds >= 60 ? TotalSeconds.ToString(@"m\:ss") : TotalSeconds.ToString(@"ss");
                case 1:
                    return TotalSeconds.TotalSeconds >= 60 ? TotalSeconds.ToString(@"m\mss\s") : TotalSeconds.ToString(@"ss\s");
                default:
                    return TotalSeconds.TotalSeconds >= 60 ? TotalSeconds.ToString(@"m\:ss") : TotalSeconds.ToString(@"ss");
            }
        }

        public bool Equals(AbnormalityControl other)
        {
            if (other == null) return false;
            float ThisDurationPercentage = Context.Duration / Context.MaxDuration;
            float OtherDurationPercentage = other.Context.Duration / other.Context.MaxDuration;
            return ThisDurationPercentage.Equals(OtherDurationPercentage);
        }

        public int CompareTo(AbnormalityControl other)
        {
            float ThisDurationPercentage = Context.MaxDuration > 0 ? Context.Duration / Context.MaxDuration : 2;
            float OtherDurationPercentage = other.Context.MaxDuration > 0 ? other.Context.Duration / other.Context.MaxDuration : 2;
            if (ThisDurationPercentage > OtherDurationPercentage) return -1;
            else if (ThisDurationPercentage < OtherDurationPercentage) return 1;
            else if (Equals(other)) return 0;
            else return 0;
        }

    }
}
