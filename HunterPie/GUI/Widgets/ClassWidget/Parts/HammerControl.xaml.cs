using System;
using System.Collections.Generic;
using System.Linq;
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

        public HammerControl()
        {
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
            OnSafijiivaCounterUpdate(this, new JobEventArgs(Context));
        }

        private void HookEvents()
        {
            Context.OnChargeLevelChange += OnChargeLevelChange;
            Context.OnPowerChargeStateChange += OnPowerChargeStateChange;
            Context.OnSafijiivaCounterUpdate += OnSafijiivaCounterUpdate;
            Context.OnWeaponSheathStateChange += OnWeaponSheathStateChange;
        }


        public override void UnhookEvents()
        {
            Context.OnChargeLevelChange -= OnChargeLevelChange;
            Context.OnPowerChargeStateChange -= OnPowerChargeStateChange;
            Context.OnSafijiivaCounterUpdate -= OnSafijiivaCounterUpdate;
            Context.OnWeaponSheathStateChange -= OnWeaponSheathStateChange;
            Context = null;
        }

        #region Event callbacks

        private void OnWeaponSheathStateChange(object source, JobEventArgs args)
        {

        }

        private void OnSafijiivaCounterUpdate(object source, JobEventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                HasSafiBuff = args.SafijiivaRegenCounter != -1;
                SafiCounter = args.SafijiivaMaxHits - args.SafijiivaRegenCounter;
            }));
        }

        private void OnPowerChargeStateChange(object source, HammerEventArgs args)
        {

        }

        private void OnChargeLevelChange(object source, HammerEventArgs args)
        {

        }
        #endregion

    }
}
