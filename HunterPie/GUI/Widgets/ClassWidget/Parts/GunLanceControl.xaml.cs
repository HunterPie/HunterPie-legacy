using System;
using System.Windows;
using HunterPie.Logger;
using GunLance = HunterPie.Core.LPlayer.Jobs.GunLance;
using GunLanceEventArgs = HunterPie.Core.LPlayer.Jobs.GunLanceEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for GunLanceControl.xaml
    /// </summary>
    public partial class GunLanceControl : ClassControl
    {

        public double WyvernstakeTimerPercentage
        {
            get { return (double)GetValue(WyvernstakeTimerPercentageProperty); }
            set { SetValue(WyvernstakeTimerPercentageProperty, value); }
        }

        public static readonly DependencyProperty WyvernstakeTimerPercentageProperty =
            DependencyProperty.Register("WyvernstakeTimerPercentage", typeof(double), typeof(GunLanceControl));

        public string WyvernstakeTimer
        {
            get { return (string)GetValue(WyvernstakeTimerProperty); }
            set { SetValue(WyvernstakeTimerProperty, value); }
        }

        public static readonly DependencyProperty WyvernstakeTimerProperty =
            DependencyProperty.Register("WyvernstakeTimer", typeof(string), typeof(GunLanceControl));

        public double WyvernboomPercentage
        {
            get { return (double)GetValue(WyvernboomPercentageProperty); }
            set { SetValue(WyvernboomPercentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WyvernboomPercentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WyvernboomPercentageProperty =
            DependencyProperty.Register("WyvernboomPercentage", typeof(double), typeof(GunLanceControl));


        GunLance Context;

        public GunLanceControl()
        {
            InitializeComponent();
        }

        public void SetContext(GunLance ctx)
        {
            Context = ctx;
            UpdateInformation();
            HookEvents();
        }

        private void UpdateInformation()
        {
            GunLanceEventArgs dummyArgs = new GunLanceEventArgs(Context);
            OnAmmoChange(this, dummyArgs);
            OnBigAmmoChange(this, dummyArgs);
            OnTotalAmmoChange(this, dummyArgs);
            OnTotalBigAmmoChange(this, dummyArgs);
            OnWyvernsFireTimerUpdate(this, dummyArgs);
            OnWyvernstakeBlastTimerUpdate(this, dummyArgs);
            OnWyvernstakeStateChanged(this, dummyArgs);
        }

        private void HookEvents()
        {
            Context.OnAmmoChange += OnAmmoChange;
            Context.OnBigAmmoChange += OnBigAmmoChange;
            Context.OnTotalAmmoChange += OnTotalAmmoChange;
            Context.OnTotalBigAmmoChange += OnTotalBigAmmoChange;
            Context.OnWyvernsFireTimerUpdate += OnWyvernsFireTimerUpdate;
            Context.OnWyvernstakeBlastTimerUpdate += OnWyvernstakeBlastTimerUpdate;
            Context.OnWyvernstakeStateChanged += OnWyvernstakeStateChanged;
        }

        public override void UnhookEvents()
        {
            Context.OnAmmoChange -= OnAmmoChange;
            Context.OnBigAmmoChange -= OnBigAmmoChange;
            Context.OnTotalAmmoChange -= OnTotalAmmoChange;
            Context.OnTotalBigAmmoChange -= OnTotalBigAmmoChange;
            Context.OnWyvernsFireTimerUpdate -= OnWyvernsFireTimerUpdate;
            Context.OnWyvernstakeBlastTimerUpdate -= OnWyvernstakeBlastTimerUpdate;
            Context.OnWyvernstakeStateChanged -= OnWyvernstakeStateChanged;
            Context = null;
            base.UnhookEvents();
        }

        private void OnWyvernstakeStateChanged(object source, GunLanceEventArgs args)
        {
            
        }

        private void OnWyvernstakeBlastTimerUpdate(object source, GunLanceEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                WyvernstakeTimerPercentage = args.WyvernstakeBlastTimer / 120;
                WyvernstakeTimer = args.WyvernstakeBlastTimer > 60 ? TimeSpan.FromSeconds(args.WyvernstakeBlastTimer).ToString("m\\:ss") :
                TimeSpan.FromSeconds(args.WyvernstakeBlastTimer).ToString("ss");
            }));
        }

        private void OnWyvernsFireTimerUpdate(object source, GunLanceEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                WyvernboomPercentage = 1 - args.WyvernsFireTimer / 120;
            }));
        }

        private void OnTotalBigAmmoChange(object source, GunLanceEventArgs args)
        {
            
        }

        private void OnTotalAmmoChange(object source, GunLanceEventArgs args)
        {
            
        }

        private void OnBigAmmoChange(object source, GunLanceEventArgs args)
        {
            
        }

        private void OnAmmoChange(object source, GunLanceEventArgs args)
        {
            
        }

        
    }
}
