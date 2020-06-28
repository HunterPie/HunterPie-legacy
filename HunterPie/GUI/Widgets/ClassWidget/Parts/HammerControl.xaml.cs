using System;
using System.Windows;
using Hammer = HunterPie.Core.LPlayer.Jobs.Hammer;
using HammerEventArgs = HunterPie.Core.LPlayer.Jobs.HammerEventArgs;
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for HammerControl.xaml
    /// </summary>
    public partial class HammerControl : ClassControl
    {

        Hammer Context;

        public int ChargeLevel
        {
            get => (int)GetValue(ChargeLevelProperty);
            set => SetValue(ChargeLevelProperty, value);
        }

        public static readonly DependencyProperty ChargeLevelProperty =
            DependencyProperty.Register("ChargeLevel", typeof(int), typeof(HammerControl));

        public float ChargeProgress
        {
            get => (float)GetValue(ChargeProgressProperty);
            set => SetValue(ChargeProgressProperty, value);
        }

        public static readonly DependencyProperty ChargeProgressProperty =
            DependencyProperty.Register("ChargeProgress", typeof(float), typeof(HammerControl));

        public bool IsPowerCharged
        {
            get => (bool)GetValue(IsPowerChargedProperty);
            set => SetValue(IsPowerChargedProperty, value);
        }

        public static readonly DependencyProperty IsPowerChargedProperty =
            DependencyProperty.Register("IsPowerCharged", typeof(bool), typeof(HammerControl));

        public bool IsChargeMaxedOut
        {
            get => (bool)GetValue(IsChargeMaxedOutProperty);
            set => SetValue(IsChargeMaxedOutProperty, value);
        }

        public static readonly DependencyProperty IsChargeMaxedOutProperty =
            DependencyProperty.Register("IsChargeMaxedOut", typeof(bool), typeof(HammerControl));

        public HammerControl()
        {
            ChargeProgress = 1;
            InitializeComponent();
        }

        public void SetContext(Hammer context)
        {
            Context = context;
            HookEvents();
        }

        private void UpdateInformation()
        {
            HammerEventArgs dummyArgs = new HammerEventArgs(Context);
            OnChargeLevelChange(this, dummyArgs);
            OnPowerChargeStateChange(this, dummyArgs);
            OnChargeProgressUpdate(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
            OnWeaponSheathStateChange(this, new JobEventArgs(Context));
        }

        private void HookEvents()
        {
            Context.OnChargeLevelChange += OnChargeLevelChange;
            Context.OnPowerChargeStateChange += OnPowerChargeStateChange;
            Context.OnChargeProgressUpdate += OnChargeProgressUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
            Context.OnWeaponSheathStateChange += OnWeaponSheathStateChange;
        }

        public override void UnhookEvents()
        {
            Context.OnChargeLevelChange -= OnChargeLevelChange;
            Context.OnPowerChargeStateChange -= OnPowerChargeStateChange;
            Context.OnChargeProgressUpdate -= OnChargeProgressUpdate;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context.OnWeaponSheathStateChange -= OnWeaponSheathStateChange;
            Context = null;
        }

        #region Event callbacks

        private void OnWeaponSheathStateChange(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      IsWeaponSheathed = args.IsWeaponSheathed;
                                                                                  }));

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));

        private void OnChargeProgressUpdate(object source, HammerEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      ChargeProgress = args.ChargeLevel >= 3 ? 1 : args.ChargeProgress % 1;
                                                                                  }));

        private void OnPowerChargeStateChange(object source, HammerEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                    {
                                                                                        IsPowerCharged = args.IsPowerCharged;
                                                                                    }));

        private void OnChargeLevelChange(object source, HammerEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                               {
                                                                                   ChargeLevel = args.ChargeLevel;
                                                                                   IsChargeMaxedOut = ChargeLevel >= 3;
                                                                               }));
        #endregion

        private void HControl_Loaded(object sender, RoutedEventArgs e) => UpdateInformation();
    }
}
