using System;
using System.Windows;
using HunterPie.Core.LPlayer.Jobs;
using SwitchAxe = HunterPie.Core.LPlayer.Jobs.SwitchAxe;
using SwitchAxeEventArgs = HunterPie.Core.LPlayer.Jobs.SwitchAxeEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for SwitchAxeControl.xaml
    /// </summary>
    public partial class SwitchAxeControl : ClassControl
    {
        SwitchAxe Context;

        public double SwitchAxeBuffPercentage
        {
            get => (double)GetValue(SwitchAxeBuffPercentageProperty);
            set => SetValue(SwitchAxeBuffPercentageProperty, value);
        }

        public static readonly DependencyProperty SwitchAxeBuffPercentageProperty =
            DependencyProperty.Register("SwitchAxeBuffPercentage", typeof(double), typeof(SwitchAxeControl));

        public string SwitchAxeBuffTimer
        {
            get => (string)GetValue(SwitchAxeBuffTimerProperty);
            set => SetValue(SwitchAxeBuffTimerProperty, value);
        }

        public static readonly DependencyProperty SwitchAxeBuffTimerProperty =
            DependencyProperty.Register("SwitchAxeBuffTimer", typeof(string), typeof(SwitchAxeControl));

        public double SwitchAxeInnerGauge
        {
            get => (double)GetValue(SwitchAxeInnerGaugeProperty);
            set => SetValue(SwitchAxeInnerGaugeProperty, value);
        }

        public static readonly DependencyProperty SwitchAxeInnerGaugeProperty =
            DependencyProperty.Register("SwitchAxeInnerGauge", typeof(double), typeof(SwitchAxeControl));

        public bool IsUnderThirtyPercent
        {
            get => (bool)GetValue(IsUnderThirtyPercentProperty);
            set => SetValue(IsUnderThirtyPercentProperty, value);
        }

        public static readonly DependencyProperty IsUnderThirtyPercentProperty =
            DependencyProperty.Register("IsUnderThirtyPercent", typeof(bool), typeof(SwitchAxeControl));

        public double SwitchAxeOuterGauge
        {
            get => (double)GetValue(SwitchAxeOuterGaugeProperty);
            set => SetValue(SwitchAxeOuterGaugeProperty, value);
        }

        public static readonly DependencyProperty SwitchAxeOuterGaugeProperty =
            DependencyProperty.Register("SwitchAxeOuterGauge", typeof(double), typeof(SwitchAxeControl));

        public string SwitchAxeOuterText
        {
            get => (string)GetValue(SwitchAxeOuterTextProperty);
            set => SetValue(SwitchAxeOuterTextProperty, value);
        }

        public static readonly DependencyProperty SwitchAxeOuterTextProperty =
            DependencyProperty.Register("SwitchAxeOuterText", typeof(string), typeof(SwitchAxeControl));

        public bool IsChargeActive
        {
            get => (bool)GetValue(IsChargeActiveProperty);
            set => SetValue(IsChargeActiveProperty, value);
        }

        public static readonly DependencyProperty IsChargeActiveProperty =
            DependencyProperty.Register("IsChargeActive", typeof(bool), typeof(SwitchAxeControl));

        public SwitchAxeControl()
        {
            SwitchAxeBuffPercentage = 1;
            SwitchAxeOuterGauge = 1;
            SwitchAxeInnerGauge = 1;
            InitializeComponent();

        }

        public void SetContext(SwitchAxe ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnInnerGaugeChange += OnInnerGaugeUpdate;
            Context.OnOuterGaugeChange += OnOuterGaugeUpdate;
            Context.OnSwitchAxeBuffStateChange += OnSwitchAxeBuffStateChange;
            Context.OnSwitchAxeBuffTimerUpdate += OnSwitchAxeBuffUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnInnerGaugeChange -= OnInnerGaugeUpdate;
            Context.OnOuterGaugeChange -= OnOuterGaugeUpdate;
            Context.OnSwitchAxeBuffStateChange -= OnSwitchAxeBuffStateChange;
            Context.OnSwitchAxeBuffTimerUpdate -= OnSwitchAxeBuffUpdate;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
        }

        private void UpdateInformation()
        {
            var dummyArgs = new SwitchAxeEventArgs(Context);
            OnInnerGaugeUpdate(this, dummyArgs);
            OnOuterGaugeUpdate(this, dummyArgs);
            OnSwitchAxeBuffStateChange(this, dummyArgs);
            OnSwitchAxeBuffUpdate(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }


        private void OnSafijiivaCounterUpdate(object source, Core.LPlayer.Jobs.JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                                   {
                                                                                                       HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                                       SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                                   }));

        private void OnSwitchAxeBuffUpdate(object source, SwitchAxeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                    {
                                                                                        if (args.IsBuffActive)
                                                                                        {
                                                                                            SwitchAxeBuffPercentage = args.SwitchAxeBuffTimer / 45;
                                                                                            SwitchAxeBuffTimer = TimeSpan.FromSeconds(args.SwitchAxeBuffTimer).ToString("m\\:ss");
                                                                                        }
                                                                                    }));

        private void OnSwitchAxeBuffStateChange(object source, SwitchAxeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                         {
                                                                                             if (!args.IsBuffActive)
                                                                                             {
                                                                                                 SwitchAxeBuffPercentage = 0;
                                                                                                 SwitchAxeBuffTimer = "0:00";
                                                                                             }
                                                                                         }));

        private void OnOuterGaugeUpdate(object source, SwitchAxeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     IsChargeActive = args.SwordChargeTimer > 0;
                                                                                     if (IsChargeActive)
                                                                                     {
                                                                                         SwitchAxeOuterGauge = args.SwordChargeTimer / args.SwordChargeMaxTimer;
                                                                                         SwitchAxeOuterText = TimeSpan.FromSeconds(args.SwordChargeTimer).ToString(args.SwordChargeTimer > 60 ? "m\\:ss" : "ss");
                                                                                     }
                                                                                     else
                                                                                     {
                                                                                         SwitchAxeOuterGauge = args.OuterGauge / 100;
                                                                                         SwitchAxeOuterText = $"{SwitchAxeOuterGauge:P0}";
                                                                                     }
                                                                                 }));

        private void OnInnerGaugeUpdate(object source, SwitchAxeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     SwitchAxeInnerGauge = args.InnerGauge / 100;
                                                                                     IsUnderThirtyPercent = args.InnerGauge / 100 <= 0.3;
                                                                                 }));

        private void SAControl_Loaded(object sender, RoutedEventArgs e) => UpdateInformation();
    }
}
