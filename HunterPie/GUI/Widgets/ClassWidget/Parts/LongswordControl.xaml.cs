using System;
using System.Linq;
using System.Windows;
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;
using Longsword = HunterPie.Core.LPlayer.Jobs.Longsword;
using LongswordEventArgs = HunterPie.Core.LPlayer.Jobs.LongswordEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for LongswordControl.xaml
    /// </summary>
    public partial class LongswordControl : ClassControl
    {
        readonly string[] OuterGaugeColors = new string[4]
        {
            "#FF323232",
            "#FFF3F3F3",
            "#FFFCBA03",
            "#FFB93030"
        };

        Longsword Context;

        public double GaugeWidth
        {
            get => (double)GetValue(GaugeWidthProperty);
            set => SetValue(GaugeWidthProperty, value);
        }

        public static readonly DependencyProperty GaugeWidthProperty =
            DependencyProperty.Register("GaugeWidth", typeof(double), typeof(LongswordControl));

        public string OuterGaugeColor
        {
            get => (string)GetValue(OuterGaugeColorProperty);
            set => SetValue(OuterGaugeColorProperty, value);
        }

        public static readonly DependencyProperty OuterGaugeColorProperty =
            DependencyProperty.Register("OuterGaugeColor", typeof(string), typeof(LongswordControl));

        public float OuterGaugePercentage
        {
            get => (float)GetValue(OuterGaugePercentageProperty);
            set => SetValue(OuterGaugePercentageProperty, value);
        }

        public static readonly DependencyProperty OuterGaugePercentageProperty =
            DependencyProperty.Register("OuterGaugePercentage", typeof(float), typeof(LongswordControl));

        public bool GaugeHasPower
        {
            get => (bool)GetValue(GaugeHasPowerProperty);
            set => SetValue(GaugeHasPowerProperty, value);
        }

        public static readonly DependencyProperty GaugeHasPowerProperty =
            DependencyProperty.Register("GaugeHasPower", typeof(bool), typeof(LongswordControl));

        public bool GaugeIsBlinking
        {
            get => (bool)GetValue(GaugeIsBlinkingProperty);
            set => SetValue(GaugeIsBlinkingProperty, value);
        }

        public static readonly DependencyProperty GaugeIsBlinkingProperty =
            DependencyProperty.Register("GaugeIsBlinking", typeof(bool), typeof(LongswordControl));

        public string GaugeBlinkDuration
        {
            get => (string)GetValue(GaugeBlinkDurationProperty);
            set => SetValue(GaugeBlinkDurationProperty, value);
        }

        public static readonly DependencyProperty GaugeBlinkDurationProperty =
            DependencyProperty.Register("GaugeBlinkDuration", typeof(string), typeof(LongswordControl));



        public LongswordControl()
        {
            GaugeHasPower = true;
            OuterGaugePercentage = 1;
            InitializeComponent();
        }

        private void UpdateInformation()
        {
            LongswordEventArgs dummyArgs = new LongswordEventArgs(Context);
            OnChargeLevelChange(this, dummyArgs);
            OnInnerGaugeChange(this, dummyArgs);
            OnOuterGaugeChange(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        public void SetContext(Longsword ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnChargeLevelChange += OnChargeLevelChange;
            Context.OnInnerGaugeChange += OnInnerGaugeChange;
            Context.OnOuterGaugeChange += OnOuterGaugeChange;
            Context.OnSpiritGaugeBlinkDurationUpdate += OnSpiritGaugeBlinkDurationUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }


        public override void UnhookEvents()
        {
            Context.OnChargeLevelChange -= OnChargeLevelChange;
            Context.OnInnerGaugeChange -= OnInnerGaugeChange;
            Context.OnOuterGaugeChange -= OnOuterGaugeChange;
            Context.OnSpiritGaugeBlinkDurationUpdate -= OnSpiritGaugeBlinkDurationUpdate;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context = null;
        }


        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));


        private void OnSpiritGaugeBlinkDurationUpdate(object source, LongswordEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                               {
                                                                                                   GaugeIsBlinking = args.SpiritGaugeBlinkDuration > 0;
                                                                                                   GaugeBlinkDuration = TimeSpan.FromSeconds(args.SpiritGaugeBlinkDuration).ToString(args.SpiritGaugeBlinkDuration > 60 ? "m\\:ss" : "ss");
                                                                                               }));

        private void OnOuterGaugeChange(object source, LongswordEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     OuterGaugePercentage = args.OuterGauge;
                                                                                     OuterGaugeColor = OuterGaugeColors.ElementAtOrDefault(args.ChargeLevel);
                                                                                 }));

        private void OnInnerGaugeChange(object source, LongswordEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     GaugeHasPower = args.InnerGauge > 0;
                                                                                     GaugeWidth = args.InnerGauge * 85;
                                                                                 }));

        private void OnChargeLevelChange(object source, LongswordEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      OuterGaugePercentage = args.OuterGauge;
                                                                                      OuterGaugeColor = OuterGaugeColors.ElementAtOrDefault(args.ChargeLevel);
                                                                                  }));

        private void LSControl_Loaded(object sender, RoutedEventArgs e) => UpdateInformation();
    }
}
