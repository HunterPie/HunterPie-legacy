using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HunterPie.Logger;
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
            get { return (int)GetValue(MaxChargeLevelProperty); }
            set { SetValue(MaxChargeLevelProperty, value); }
        }

        public static readonly DependencyProperty MaxChargeLevelProperty =
            DependencyProperty.Register("MaxChargeLevel", typeof(int), typeof(BowControl));

        public int ChargeLevel
        {
            get { return (int)GetValue(ChargeLevelProperty); }
            set { SetValue(ChargeLevelProperty, value); }
        }

        public static readonly DependencyProperty ChargeLevelProperty =
            DependencyProperty.Register("ChargeLevel", typeof(int), typeof(BowControl));

        public float ChargeProgress
        {
            get { return (float)GetValue(ChargeProgressProperty); }
            set { SetValue(ChargeProgressProperty, value); }
        }

        public static readonly DependencyProperty ChargeProgressProperty =
            DependencyProperty.Register("ChargeProgress", typeof(float), typeof(BowControl));



        public BowControl()
        {
            InitializeComponent();
        }


        private void UpdateInformation()
        {
            
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
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
        }

        public override void UnhookEvents()
        {
            Context.OnChargeProgressUpdate -= OnChargeProgressUpdate;
            Context.OnChargeLevelChange -= OnChargeLevelChange;
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

        private void OnChargeLevelChange(object source, BowEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                ChargeLevel = args.ChargeLevel;
                MaxChargeLevel = args.MaxChargeLevel;
            }));
        }

        private void OnChargeProgressUpdate(object source, BowEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                ChargeProgress = Math.Floor(args.ChargeProgress) >= args.MaxChargeLevel ? 1 : args.ChargeProgress % 1;
            }));
        }

    }
}
