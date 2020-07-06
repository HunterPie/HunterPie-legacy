using System;
using System.Windows;
using DualBlades = HunterPie.Core.LPlayer.Jobs.DualBlades;
using DualBladesEventArgs = HunterPie.Core.LPlayer.Jobs.DualBladesEventArgs;
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for DualBladeControl.xaml
    /// </summary>
    public partial class DualBladeControl : ClassControl
    {

        DualBlades Context;

        public bool IsDemonModeActive
        {
            get => (bool)GetValue(IsDemonModeActiveProperty);
            set => SetValue(IsDemonModeActiveProperty, value);
        }
        public static readonly DependencyProperty IsDemonModeActiveProperty =
            DependencyProperty.Register("IsDemonModeActive", typeof(bool), typeof(DualBladeControl));

        public float GaugePercentage
        {
            get => (float)GetValue(GaugePercentageProperty);
            set => SetValue(GaugePercentageProperty, value);
        }
        public static readonly DependencyProperty GaugePercentageProperty =
            DependencyProperty.Register("GaugePercentage", typeof(float), typeof(DualBladeControl));

        public float DemonGauge
        {
            get => (float)GetValue(DemonGaugeProperty);
            set => SetValue(DemonGaugeProperty, value);
        }
        public static readonly DependencyProperty DemonGaugeProperty =
            DependencyProperty.Register("DemonGauge", typeof(float), typeof(DualBladeControl));

        public bool IsReducing
        {
            get => (bool)GetValue(IsReducingProperty);
            set => SetValue(IsReducingProperty, value);
        }
        public static readonly DependencyProperty IsReducingProperty =
            DependencyProperty.Register("IsReducing", typeof(bool), typeof(DualBladeControl));



        public DualBladeControl()
        {
            IsReducing = false;
            InitializeComponent();
        }

        public void SetContext(DualBlades ctx)
        {
            Context = ctx;
            UpdateInformation();
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnDemonGaugeChange += OnDemonGaugeChange;
            Context.OnDemonModeToggle += OnDemonModeToggle;
            Context.OnDemonGaugeReduce += OnDemonGaugeReduce;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        private void OnDemonGaugeReduce(object source, DualBladesEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      IsReducing = args.IsReducing;
                                                                                  }));

        public override void UnhookEvents()
        {
            Context.OnDemonGaugeChange -= OnDemonGaugeChange;
            Context.OnDemonModeToggle -= OnDemonModeToggle;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
        }

        private void UpdateInformation()
        {
            DualBladesEventArgs dummyArgs = new DualBladesEventArgs(Context);
            OnDemonGaugeChange(this, dummyArgs);
            OnDemonModeToggle(this, dummyArgs);
            OnDemonGaugeReduce(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        #region Game Events

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));

        private void OnDemonModeToggle(object source, DualBladesEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     IsDemonModeActive = args.InDemonMode;
                                                                                 }));

        private void OnDemonGaugeChange(object source, DualBladesEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      GaugePercentage = 102 * args.DemonGauge;
                                                                                      DemonGauge = args.DemonGauge;
                                                                                  }));

        #endregion
    }
}
