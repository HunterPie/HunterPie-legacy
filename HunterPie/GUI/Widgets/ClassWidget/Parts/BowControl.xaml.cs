using System;
using System.Windows;
using Bow = HunterPie.Core.LPlayer.Jobs.Bow;
using BowEventArgs = HunterPie.Core.LPlayer.Jobs.BowEventArgs;
using JobEventArgs = HunterPie.Core.LPlayer.Jobs.JobEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for BowControl.xaml
    /// </summary>
    public partial class BowControl : ClassControl
    {

        Bow Context;

        public int MaxChargeLevel
        {
            get => (int)GetValue(MaxChargeLevelProperty);
            set => SetValue(MaxChargeLevelProperty, value);
        }

        public static readonly DependencyProperty MaxChargeLevelProperty =
            DependencyProperty.Register("MaxChargeLevel", typeof(int), typeof(BowControl));

        public int ChargeLevel
        {
            get => (int)GetValue(ChargeLevelProperty);
            set => SetValue(ChargeLevelProperty, value);
        }

        public static readonly DependencyProperty ChargeLevelProperty =
            DependencyProperty.Register("ChargeLevel", typeof(int), typeof(BowControl));

        public float ChargeProgress
        {
            get => (float)GetValue(ChargeProgressProperty);
            set => SetValue(ChargeProgressProperty, value);
        }

        public static readonly DependencyProperty ChargeProgressProperty =
            DependencyProperty.Register("ChargeProgress", typeof(float), typeof(BowControl));

        public BowControl() => InitializeComponent();


        private void UpdateInformation()
        {
            var dummyArgs = new BowEventArgs(Context);
            OnChargeProgressUpdate(this, dummyArgs);
            OnChargeLevelChange(this, dummyArgs);
            OnChargeLevelMaxUpdate(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
            OnWeaponSheathStateChange(this, new JobEventArgs(Context));
        }

        public void SetContext(Bow ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnChargeProgressUpdate += OnChargeProgressUpdate;
            Context.OnChargeLevelChange += OnChargeLevelChange;
            Context.OnChargeLevelMaxUpdate += OnChargeLevelMaxUpdate;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
            Context.OnWeaponSheathStateChange += OnWeaponSheathStateChange;
        }

        public override void UnhookEvents()
        {
            Context.OnChargeProgressUpdate -= OnChargeProgressUpdate;
            Context.OnChargeLevelChange -= OnChargeLevelChange;
            Context.OnChargeLevelMaxUpdate -= OnChargeLevelMaxUpdate;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context.OnWeaponSheathStateChange -= OnWeaponSheathStateChange;
            Context = null;
        }

        private void OnWeaponSheathStateChange(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                  {
                                                                                      IsWeaponSheathed = args.IsWeaponSheathed;
                                                                                  }));

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));

        private void OnChargeLevelMaxUpdate(object source, BowEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                               {
                                                                                   MaxChargeLevel = args.MaxChargeLevel + 1;
                                                                               }));

        private void OnChargeLevelChange(object source, BowEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                            {
                                                                                ChargeLevel = args.ChargeLevel + 1;
                                                                            }));

        private void OnChargeProgressUpdate(object source, BowEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                               {
                                                                                   ChargeProgress = Math.Floor(args.ChargeProgress) >= args.MaxChargeLevel ? 1 : args.ChargeProgress % 1;
                                                                               }));

        private void BControl_Loaded(object sender, RoutedEventArgs e) => UpdateInformation();
    }
}
