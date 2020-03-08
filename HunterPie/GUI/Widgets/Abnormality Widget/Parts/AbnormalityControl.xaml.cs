using System;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.Abnormality_Widget.Parts {
    /// <summary>
    /// Interaction logic for Abnormality.xaml
    /// </summary>
    public partial class AbnormalityControl : UserControl {

        Brush Debuff_Color = new SolidColorBrush(Color.FromArgb(0xFF, 0x97, 0x32, 0x32)) ;
        Brush Buff_Color = new SolidColorBrush(Color.FromArgb(0xFF, 0x32, 0x97, 0x45)); ///; // #FF329745

        Abnormality Context;

        public AbnormalityControl(Abnormality Abnorm) {
            InitializeComponent();
            SetAbnormalityInfo(Abnorm);
            Context = Abnorm;
            HookEvents();
        }

        private void HookEvents() {
            Context.OnAbnormalityUpdate += OnAbnormalityUpdate;
            Context.OnAbnormalityEnd += OnAbnormalityEnd;
            Context.OnStackChange += OnAbnormalityStackChange;
        }

        private void OnAbnormalityStackChange(object source, AbnormalityEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                ImageSource NewIcon = TryFindResource($"{args.Abnormality.Icon}+") as ImageSource;
                if (NewIcon == null) NewIcon = FindResource($"{args.Abnormality.Icon}") as ImageSource;
                this.AbnormalityIcon.Source = NewIcon;
            }));
        }

        private void SetAbnormalityInfo(Abnormality Abnorm) {
            float angle;
            if (Abnorm.IsInfinite || Abnorm.MaxDuration == 0) angle = 90;
            else { angle = ConvertPercentageIntoAngle(Abnorm.Duration / Abnorm.MaxDuration); }
            this.AbnormalityDurationArc.EndAngle = angle;
            ImageSource AbnormIcon;
            if (Abnorm.Stack >= 1) {
                AbnormIcon = TryFindResource($"{Abnorm.Icon}+") as ImageSource ?? FindResource($"{Abnorm.Icon}") as ImageSource;
            } else {
                AbnormIcon = FindResource(Abnorm.Icon) as ImageSource;
            }
            this.AbnormalityIcon.Source = AbnormIcon;
            this.AbnormalityDurationArc.Stroke = Abnorm.IsDebuff ? Debuff_Color : Buff_Color;
        } 

        private void OnAbnormalityEnd(object source, AbnormalityEventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                args.Abnormality.OnAbnormalityUpdate -= OnAbnormalityUpdate;
                args.Abnormality.OnAbnormalityEnd -= OnAbnormalityEnd;
                args.Abnormality.OnStackChange -= OnAbnormalityStackChange;
                Context = null;
            }));
        }

        private void OnAbnormalityUpdate(object source, AbnormalityEventArgs args) {
            float angle;
            if (args.Abnormality.IsInfinite || args.Abnormality.MaxDuration == 0) angle = 90;
            else { angle = ConvertPercentageIntoAngle(args.Abnormality.Duration / args.Abnormality.MaxDuration); }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.AbnormalityDurationArc.EndAngle = angle;
            }));
        }

        // Helper

        private float ConvertPercentageIntoAngle(float percentage) {
            float max = -269.999f;
            float angle = 90 - (360 * percentage);
            if (angle < max) angle = max;
            return angle;
        }
    }
}
