using System.Windows;
using HunterPie.Core.Jobs;
using System.Windows.Threading;
using System;
using System.Linq;
using HunterPie.Logger;
using HunterPie.Core.Events;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for GreatswordControl.xaml
    /// </summary>
    public partial class GreatswordControl : ClassControl
    {

        public float ChargeTimer
        {
            get { return (float)GetValue(ChargeTimerProperty); }
            set { SetValue(ChargeTimerProperty, value); }
        }
        public static readonly DependencyProperty ChargeTimerProperty =
            DependencyProperty.Register("ChargeTimer", typeof(float), typeof(GreatswordControl));

        public uint ChargeLevel
        {
            get { return (uint)GetValue(ChargeLevelProperty); }
            set { SetValue(ChargeLevelProperty, value); }
        }
        public static readonly DependencyProperty ChargeLevelProperty =
            DependencyProperty.Register("ChargeLevel", typeof(uint), typeof(GreatswordControl));

        public bool IsChargeMaxedOut
        {
            get { return (bool)GetValue(IsChargeMaxedOutProperty); }
            set { SetValue(IsChargeMaxedOutProperty, value); }
        }
        public static readonly DependencyProperty IsChargeMaxedOutProperty =
            DependencyProperty.Register("IsChargeMaxedOut", typeof(bool), typeof(GreatswordControl));

        public bool IsOvercharged
        {
            get { return (bool)GetValue(IsOverchargedProperty); }
            set { SetValue(IsOverchargedProperty, value); }
        }
        public static readonly DependencyProperty IsOverchargedProperty =
            DependencyProperty.Register("IsOvercharged", typeof(bool), typeof(GreatswordControl));


        Greatsword context { get; set; }

        public GreatswordControl()
        {
            InitializeComponent();
        }

        public void SetContext(Greatsword ctx)
        {
            context = ctx;
            HookEvents();
            InitializeData();
        }

        private void InitializeData()
        {
            JobEventArgs jobArgs = new JobEventArgs(context);
            GreatswordEventArgs args = new GreatswordEventArgs(context);
            OnChargeLevelChange(this, args);
            OnChargeTimerChange(this, args);
            OnWeaponSheathStateChange(this, jobArgs);
            OnSafijiivaCounterUpdate(this, jobArgs);
        }

        private void HookEvents()
        {
            context.OnChargeLevelChange += OnChargeLevelChange;
            context.OnChargeTimerChange += OnChargeTimerChange;
            context.OnWeaponSheathStateChange += OnWeaponSheathStateChange;
            context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            context.OnChargeLevelChange -= OnChargeLevelChange;
            context.OnChargeTimerChange -= OnChargeTimerChange;
            context.OnWeaponSheathStateChange -= OnWeaponSheathStateChange;
            context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            base.UnhookEvents();
        }

        #region Events
        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                HasSafiBuff = args.SafijiivaRegenCounter != -1;
                SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
            }));
        }

        private void OnWeaponSheathStateChange(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                IsWeaponSheathed = args.IsWeaponSheathed;
            }));
        }

        private void OnChargeTimerChange(object source, GreatswordEventArgs args)
        {
            float minCharge = Greatsword.ChargeTimes.ElementAtOrDefault((int)args.ChargeLevel - 1);
            float maxCharge = Greatsword.ChargeTimes.ElementAtOrDefault((int)args.ChargeLevel);

            if (maxCharge == 0f)
            {
                maxCharge = 1f;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                IsOvercharged = args.IsOvercharged;
                if (args.ChargeLevel >= 3)
                {
                    ChargeTimer = 1;
                    IsChargeMaxedOut = true && !IsOvercharged;
                } else
                {
                    ChargeTimer = (args.ChargeTimer - minCharge) / (maxCharge - minCharge);
                    IsChargeMaxedOut = false;
                }
                
            }));
        }

        private void OnChargeLevelChange(object source, GreatswordEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                ChargeLevel = args.ChargeLevel;
            }));
        }
        #endregion
    }
}
