using System;
using System.Windows;
using HunterPie.Core.LPlayer.Jobs;
using InsectGlaive = HunterPie.Core.LPlayer.Jobs.InsectGlaive;
using InsectGlaiveEventArgs = HunterPie.Core.LPlayer.Jobs.InsectGlaiveEventArgs;
using KinsectChargeBuff = HunterPie.Core.LPlayer.Jobs.KinsectChargeBuff;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for InsectGlaiveControl.xaml
    /// </summary>
    public partial class InsectGlaiveControl : ClassControl
    {
        const string RedBuffColor = "#CCFF3B3B";
        const string WhiteBuffColor = "#CCF3F3F3";
        const string OrangeBuffColor = "#CCFB7D25";
        const string GreenBuffColor = "#CC57FA20";

        public string FirstBuffQueued
        {
            get => (string)GetValue(FirstBuffQueuedProperty);
            set => SetValue(FirstBuffQueuedProperty, value);
        }

        public static readonly DependencyProperty FirstBuffQueuedProperty =
            DependencyProperty.Register("FirstBuffQueued", typeof(string), typeof(InsectGlaiveControl));

        public string SecondBuffQueued
        {
            get => (string)GetValue(SecondBuffQueuedProperty);
            set => SetValue(SecondBuffQueuedProperty, value);
        }

        public static readonly DependencyProperty SecondBuffQueuedProperty =
            DependencyProperty.Register("SecondBuffQueued", typeof(string), typeof(InsectGlaiveControl));

        public string KinsectChargeImage
        {
            get => (string)GetValue(KinsectChargeImageProperty);
            set => SetValue(KinsectChargeImageProperty, value);
        }

        public static readonly DependencyProperty KinsectChargeImageProperty =
            DependencyProperty.Register("KinsectChargeImage", typeof(string), typeof(InsectGlaiveControl));

        public string ChargeBuffTimer
        {
            get => (string)GetValue(ChargeBuffTimerProperty);
            set => SetValue(ChargeBuffTimerProperty, value);
        }

        public static readonly DependencyProperty ChargeBuffTimerProperty =
            DependencyProperty.Register("ChargeBuffTimer", typeof(string), typeof(InsectGlaiveControl));

        public double StaminaBarSize
        {
            get => (double)GetValue(StaminaBarSizeProperty);
            set => SetValue(StaminaBarSizeProperty, value);
        }

        public static readonly DependencyProperty StaminaBarSizeProperty =
            DependencyProperty.Register("StaminaBarSize", typeof(double), typeof(InsectGlaiveControl));

        public string RedBuff
        {
            get => (string)GetValue(RedBuffProperty);
            set => SetValue(RedBuffProperty, value);
        }

        public static readonly DependencyProperty RedBuffProperty =
            DependencyProperty.Register("RedBuff", typeof(string), typeof(InsectGlaiveControl));

        public bool RedBuffActive
        {
            get => (bool)GetValue(RedBuffActiveProperty);
            set => SetValue(RedBuffActiveProperty, value);
        }

        public static readonly DependencyProperty RedBuffActiveProperty =
            DependencyProperty.Register("RedBuffActive", typeof(bool), typeof(InsectGlaiveControl));

        public string WhiteBuff
        {
            get => (string)GetValue(WhiteBuffProperty);
            set => SetValue(WhiteBuffProperty, value);
        }

        public static readonly DependencyProperty WhiteBuffProperty =
            DependencyProperty.Register("WhiteBuff", typeof(string), typeof(InsectGlaiveControl));

        public bool WhiteBuffActive
        {
            get => (bool)GetValue(WhiteBuffActiveProperty);
            set => SetValue(WhiteBuffActiveProperty, value);
        }

        public static readonly DependencyProperty WhiteBuffActiveProperty =
            DependencyProperty.Register("WhiteBuffActive", typeof(bool), typeof(InsectGlaiveControl));

        public string OrangeBuff
        {
            get => (string)GetValue(OrangeBuffProperty);
            set => SetValue(OrangeBuffProperty, value);
        }

        public static readonly DependencyProperty OrangeBuffProperty =
            DependencyProperty.Register("OrangeBuff", typeof(string), typeof(InsectGlaiveControl));

        public bool OrangeBuffActive
        {
            get => (bool)GetValue(OrangeBuffActiveProperty);
            set => SetValue(OrangeBuffActiveProperty, value);
        }

        public static readonly DependencyProperty OrangeBuffActiveProperty =
            DependencyProperty.Register("OrangeBuffActive", typeof(bool), typeof(InsectGlaiveControl));

        InsectGlaive Context;

        public InsectGlaiveControl() => InitializeComponent();

        public void SetContext(InsectGlaive ctx)
        {
            Context = ctx;
            UpdateInformation();
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnRedBuffUpdate += OnRedBuffUpdate;
            Context.OnWhiteBuffUpdate += OnWhiteBuffUpdate;
            Context.OnOrangeBuffUpdate += OnOrangeBuffUpdate;
            Context.OnKinsectChargeBuffChange += OnKinsectChargeBuffChange;
            Context.OnKinsectChargeBuffUpdate += OnKinsectChargeBuffUpdate;
            Context.OnKinsectStaminaUpdate += OnKinsectStaminaUpdate;
            Context.BuffQueueChanged += OnBuffQueueChanged;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnRedBuffUpdate -= OnRedBuffUpdate;
            Context.OnWhiteBuffUpdate -= OnWhiteBuffUpdate;
            Context.OnOrangeBuffUpdate -= OnOrangeBuffUpdate;
            Context.OnKinsectChargeBuffChange -= OnKinsectChargeBuffChange;
            Context.OnKinsectChargeBuffUpdate -= OnKinsectChargeBuffUpdate;
            Context.OnKinsectStaminaUpdate -= OnKinsectStaminaUpdate;
            Context.BuffQueueChanged -= OnBuffQueueChanged;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context = null;
        }

        private void UpdateInformation()
        {
            InsectGlaiveEventArgs dummyArgs = new InsectGlaiveEventArgs(Context);
            OnRedBuffUpdate(this, dummyArgs);
            OnWhiteBuffUpdate(this, dummyArgs);
            OnOrangeBuffUpdate(this, dummyArgs);
            OnKinsectChargeBuffChange(this, dummyArgs);
            OnKinsectChargeBuffUpdate(this, dummyArgs);
            OnKinsectStaminaUpdate(this, dummyArgs);
            OnBuffQueueChanged(this, dummyArgs);
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }


        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     HasSafiBuff = args.SafijiivaRegenCounter != -1;
                                                                                     SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
                                                                                 }));

        private void OnBuffQueueChanged(object source, InsectGlaiveEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                    {
                                                                                        switch (args.BuffQueueSize)
                                                                                        {
                                                                                            case 0:
                                                                                                SecondBuffQueued = FirstBuffQueued = "#CC464646";
                                                                                                break;
                                                                                            case 1:
                                                                                                SecondBuffQueued = FirstBuffQueued = GetKinsectBuffColorByID(args.FirstBuffQueued);
                                                                                                break;
                                                                                            case 2:
                                                                                                FirstBuffQueued = GetKinsectBuffColorByID(args.FirstBuffQueued);
                                                                                                SecondBuffQueued = GetKinsectBuffColorByID(args.SecondBuffQueued);
                                                                                                break;
                                                                                        }
                                                                                    }));

        private void OnKinsectStaminaUpdate(object source, InsectGlaiveEventArgs args)
        {
            float maxSize = args.KinsectChargeType == KinsectChargeBuff.Yellow || args.KinsectChargeType == KinsectChargeBuff.Both ? 200 : 100;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                StaminaBarSize = 120 * (args.KinsectStamina / maxSize);
            }));
        }

        private void OnKinsectChargeBuffUpdate(object source, InsectGlaiveEventArgs args)
        {
            float buffTimer = 0;
            switch (args.KinsectChargeType)
            {
                case KinsectChargeBuff.Red:
                case KinsectChargeBuff.Both:
                    buffTimer = args.RedKinsectTimer;
                    break;
                case KinsectChargeBuff.Yellow:
                    buffTimer = args.YellowKinsectTimer;
                    break;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                ChargeBuffTimer = buffTimer > 60 ? TimeSpan.FromSeconds(buffTimer).ToString("m\\:ss") : TimeSpan.FromSeconds(buffTimer).ToString("ss");
            }));
        }

        private void OnKinsectChargeBuffChange(object source, InsectGlaiveEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                           {
                                                                                               switch (args.KinsectChargeType)
                                                                                               {
                                                                                                   case KinsectChargeBuff.None:
                                                                                                       KinsectChargeImage = null;
                                                                                                       break;
                                                                                                   case KinsectChargeBuff.Red:
                                                                                                       KinsectChargeImage = "pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/KinsectRedCharge.png";
                                                                                                       break;
                                                                                                   case KinsectChargeBuff.Yellow:
                                                                                                       KinsectChargeImage = "pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/KinsectYellowCharge.png";
                                                                                                       break;
                                                                                                   case KinsectChargeBuff.Both:
                                                                                                       KinsectChargeImage = "pack://siteoforigin:,,,/HunterPie.Resources/UI/Class/KinsectBothCharges.png";
                                                                                                       break;
                                                                                               }
                                                                                           }));

        private void OnOrangeBuffUpdate(object source, InsectGlaiveEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                    {
                                                                                        OrangeBuff = args.OrangeBuff > 60 ? TimeSpan.FromSeconds(args.OrangeBuff).ToString("m\\:ss") :
                                                                                        TimeSpan.FromSeconds(args.OrangeBuff).ToString("ss");
                                                                                        OrangeBuffActive = args.OrangeBuff > 0;
                                                                                    }));

        private void OnWhiteBuffUpdate(object source, InsectGlaiveEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                   {
                                                                                       WhiteBuff = args.WhiteBuff > 60 ? TimeSpan.FromSeconds(args.WhiteBuff).ToString("m\\:ss") :
                                                                                       TimeSpan.FromSeconds(args.WhiteBuff).ToString("ss");
                                                                                       WhiteBuffActive = args.WhiteBuff > 0;
                                                                                   }));

        private void OnRedBuffUpdate(object source, InsectGlaiveEventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                                                                                 {
                                                                                     RedBuff = args.RedBuff > 60 ? TimeSpan.FromSeconds(args.RedBuff).ToString("m\\:ss") :
                                                                                     TimeSpan.FromSeconds(args.RedBuff).ToString("ss");
                                                                                     RedBuffActive = args.RedBuff > 0;
                                                                                 }));

        private string GetKinsectBuffColorByID(int ID)
        {
            if (ID == 0) return RedBuffColor;
            if (ID == 1) return WhiteBuffColor;
            if (ID == 2) return OrangeBuffColor;
            if (ID == 3) return GreenBuffColor;
            return "#CC464646";
        }

    }
}
