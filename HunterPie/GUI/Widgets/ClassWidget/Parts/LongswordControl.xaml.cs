using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        string[] OuterGaugeColors = new string[4]
        {
            "#FF323232",
            "#FFF3F3F3",
            "#FFFCBA03",
            "#FFB93030"
        };

        Longsword Context;

        public double GaugeWidth
        {
            get { return (double)GetValue(GaugeWidthProperty); }
            set { SetValue(GaugeWidthProperty, value); }
        }

        public static readonly DependencyProperty GaugeWidthProperty =
            DependencyProperty.Register("GaugeWidth", typeof(double), typeof(LongswordControl));

        public string OuterGaugeColor
        {
            get { return (string)GetValue(OuterGaugeColorProperty); }
            set { SetValue(OuterGaugeColorProperty, value); }
        }

        public static readonly DependencyProperty OuterGaugeColorProperty =
            DependencyProperty.Register("OuterGaugeColor", typeof(string), typeof(LongswordControl));

        public float OuterGaugePercentage
        {
            get { return (float)GetValue(OuterGaugePercentageProperty); }
            set { SetValue(OuterGaugePercentageProperty, value); }
        }

        public static readonly DependencyProperty OuterGaugePercentageProperty =
            DependencyProperty.Register("OuterGaugePercentage", typeof(float), typeof(LongswordControl));

        public LongswordControl()
        {
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
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnChargeLevelChange -= OnChargeLevelChange;
            Context.OnInnerGaugeChange -= OnInnerGaugeChange;
            Context.OnOuterGaugeChange -= OnOuterGaugeChange;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context = null;
            base.UnhookEvents();
        }


        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                HasSafiBuff = args.SafijiivaRegenCounter != -1;
                SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
            }));
        }

        private void OnOuterGaugeChange(object source, LongswordEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                OuterGaugePercentage = args.OuterGauge;
                OuterGaugeColor = OuterGaugeColors.ElementAtOrDefault(args.ChargeLevel);
            }));
        }

        private void OnInnerGaugeChange(object source, LongswordEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                GaugeWidth = args.InnerGauge * 85;
            }));
        }

        private void OnChargeLevelChange(object source, LongswordEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                OuterGaugePercentage = args.OuterGauge;
                OuterGaugeColor = OuterGaugeColors.ElementAtOrDefault(args.ChargeLevel);
            }));
        }

        private void LSControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInformation();
        }
    }
}
