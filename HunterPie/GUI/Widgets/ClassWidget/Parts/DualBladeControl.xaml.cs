using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            get { return (bool)GetValue(IsDemonModeActiveProperty); }
            set { SetValue(IsDemonModeActiveProperty, value); }
        }
        public static readonly DependencyProperty IsDemonModeActiveProperty =
            DependencyProperty.Register("IsDemonModeActive", typeof(bool), typeof(DualBladeControl));

        public float GaugePercentage
        {
            get { return (float)GetValue(GaugePercentageProperty); }
            set { SetValue(GaugePercentageProperty, value); }
        }
        public static readonly DependencyProperty GaugePercentageProperty =
            DependencyProperty.Register("GaugePercentage", typeof(float), typeof(DualBladeControl));

        public float DemonGauge
        {
            get { return (float)GetValue(DemonGaugeProperty); }
            set { SetValue(DemonGaugeProperty, value); }
        }
        public static readonly DependencyProperty DemonGaugeProperty =
            DependencyProperty.Register("DemonGauge", typeof(float), typeof(DualBladeControl));



        public DualBladeControl()
        {
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
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

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
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        #region Game Events

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                HasSafiBuff = args.SafijiivaRegenCounter != -1;
                SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
            }));
        }

        private void OnDemonModeToggle(object source, DualBladesEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                IsDemonModeActive = args.InDemonMode;
            }));
        }

        private void OnDemonGaugeChange(object source, DualBladesEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                GaugePercentage = 103 * args.DemonGauge;
                DemonGauge = args.DemonGauge;
            }));
        }

        #endregion
    }
}
