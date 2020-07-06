using System;
using System.Windows;
using HunterPie.Core.LPlayer.Jobs;
using ChargeBlade = HunterPie.Core.LPlayer.Jobs.ChargeBlade;
using ChargeBladeEventArgs = HunterPie.Core.LPlayer.Jobs.ChargeBladeEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for ChargeBladeControl.xaml
    /// </summary>
    public partial class ChargeBladeControl : ClassControl
    {

        public int Vials
        {
            get => (int)GetValue(VialsProperty);
            set => SetValue(VialsProperty, value);
        }

        // Using a DependencyProperty as the backing store for Vials.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VialsProperty =
            DependencyProperty.Register("Vials", typeof(int), typeof(ChargeBladeControl));

        public double HiddenGaugeSize
        {
            get => (double)GetValue(HiddenGaugeSizeProperty);
            set => SetValue(HiddenGaugeSizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for HiddenGaugeSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiddenGaugeSizeProperty =
            DependencyProperty.Register("HiddenGaugeSize", typeof(double), typeof(ChargeBladeControl));

        public string HiddenGaugeColor
        {
            get => (string)GetValue(HiddenGaugeColorProperty);
            set => SetValue(HiddenGaugeColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for HiddenGaugeColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HiddenGaugeColorProperty =
            DependencyProperty.Register("HiddenGaugeColor", typeof(string), typeof(ChargeBladeControl));

        public string SwordBuff
        {
            get => (string)GetValue(SwordBuffProperty);
            set => SetValue(SwordBuffProperty, value);
        }

        // Using a DependencyProperty as the backing store for SwordBuff.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwordBuffProperty =
            DependencyProperty.Register("SwordBuff", typeof(string), typeof(ChargeBladeControl));

        public double SwordBuffOpacity
        {
            get => (double)GetValue(SwordBuffOpacityProperty);
            set => SetValue(SwordBuffOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for SwordBuffOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwordBuffOpacityProperty =
            DependencyProperty.Register("SwordBuffOpacity", typeof(double), typeof(ChargeBladeControl));


        public string ShieldBuff
        {
            get => (string)GetValue(ShieldBuffProperty);
            set => SetValue(ShieldBuffProperty, value);
        }

        // Using a DependencyProperty as the backing store for ShieldBuff.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShieldBuffProperty =
            DependencyProperty.Register("ShieldBuff", typeof(string), typeof(ChargeBladeControl));

        public double ShieldBuffOpacity
        {
            get => (double)GetValue(ShieldBuffOpacityProperty);
            set => SetValue(ShieldBuffOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for ShieldBuffOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShieldBuffOpacityProperty =
            DependencyProperty.Register("ShieldBuffOpacity", typeof(double), typeof(ChargeBladeControl));

        public string PoweraxeBuff
        {
            get => (string)GetValue(PoweraxeBuffProperty);
            set => SetValue(PoweraxeBuffProperty, value);
        }

        public static readonly DependencyProperty PoweraxeBuffProperty =
            DependencyProperty.Register("PoweraxeBuff", typeof(string), typeof(ChargeBladeControl));

        public double PoweraxeOpacity
        {
            get => (double)GetValue(PoweraxeOpacityProperty);
            set => SetValue(PoweraxeOpacityProperty, value);
        }

        public static readonly DependencyProperty PoweraxeOpacityProperty =
            DependencyProperty.Register("PoweraxeOpacity", typeof(double), typeof(ChargeBladeControl));


        ChargeBlade Context;

        public ChargeBladeControl() => InitializeComponent();

        public void SetContext(ChargeBlade ctx)
        {
            Context = ctx;
            UpdateInformation();
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnShieldBuffChange += OnShieldBuffUpdate;
            Context.OnSwordBuffChange += OnSwordBuffUpdate;
            Context.OnVialChargeGaugeChange += OnVialChargeGaugeUpdate;
            Context.OnVialsChange += OnVialsChange;
            Context.OnPoweraxeBuffChange += OnPowerchargeUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnShieldBuffChange -= OnShieldBuffUpdate;
            Context.OnSwordBuffChange -= OnSwordBuffUpdate;
            Context.OnVialChargeGaugeChange -= OnVialChargeGaugeUpdate;
            Context.OnVialsChange -= OnVialsChange;
            Context.OnPoweraxeBuffChange -= OnPowerchargeUpdate;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context = null;
        }

        private void UpdateInformation()
        {
            // In case HunterPie is started mid-game, update everything without waiting for events
            ChargeBladeEventArgs dummyArgs = new ChargeBladeEventArgs(Context);
            OnPowerchargeUpdate(this, dummyArgs);
            OnVialsChange(this, dummyArgs);
            OnVialChargeGaugeUpdate(this, dummyArgs);
            OnSwordBuffUpdate(this, dummyArgs);
            OnShieldBuffUpdate(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }


        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));

        private void OnPowerchargeUpdate(object source, ChargeBladeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                    {
                                                                                        PoweraxeBuff = args.PoweraxeTimer > 0 ? $"{TimeSpan.FromSeconds(args.PoweraxeTimer):ss}" : null;
                                                                                        PoweraxeOpacity = args.PoweraxeTimer > 0 ? 1 : 0;
                                                                                    }));

        private void OnVialsChange(object source, ChargeBladeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                              {
                                                                                  Vials = args.Vials;
                                                                              }));

        private void OnVialChargeGaugeUpdate(object source, ChargeBladeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                        {
                                                                                            HiddenGaugeSize = 77 * args.VialChargeGauge / 100;
                                                                                            HiddenGaugeColor = args.VialChargeGauge < 30 ? "#FFD6CBB8" :
                                                                                            args.VialChargeGauge > 70 ? "#FFFF0202" :
                                                                                            args.VialChargeGauge > 45 ? "#FFA41515" : "#FFD68800";

                                                                                        }));

        private void OnSwordBuffUpdate(object source, ChargeBladeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      SwordBuff = args.SwordBuffTimer > 0 ? $"{TimeSpan.FromSeconds(args.SwordBuffTimer):ss}" : null;
                                                                                      SwordBuffOpacity = args.SwordBuffTimer > 0 ? 1 : 0;
                                                                                  }));

        private void OnShieldBuffUpdate(object source, ChargeBladeEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                   {
                                                                                       ShieldBuff = args.ShieldBuffTimer > 0 ? $"{TimeSpan.FromSeconds(args.ShieldBuffTimer):m\\:ss}" : null;
                                                                                       ShieldBuffOpacity = args.ShieldBuffTimer > 0 ? 1 : 0;
                                                                                   }));
    }
}
