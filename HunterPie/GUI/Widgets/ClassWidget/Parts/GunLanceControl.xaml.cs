using System;
using GunLance = HunterPie.Core.LPlayer.Jobs.GunLance;
using GunLanceEventArgs = HunterPie.Core.LPlayer.Jobs.GunLanceEventArgs;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    /// <summary>
    /// Interaction logic for GunLanceControl.xaml
    /// </summary>
    public partial class GunLanceControl : ClassControl
    {

        GunLance Context;

        public GunLanceControl()
        {
            InitializeComponent();
        }

        public void SetContext(GunLance ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.OnAmmoChange += OnAmmoChage;
            Context.OnBigAmmoChange += OnBigAmmoChange;
            Context.OnTotalAmmoChange += OnTotalAmmoChange;
            Context.OnTotalBigAmmoChange += OnTotalBigAmmoChange;
            Context.OnWyvernsFireTimerUpdate += OnWyvernsFireTimerUpdate;
            Context.OnWyvernstakeBlastTimerUpdate += OnWyvernstakeBlastTimerUpdate;
            Context.OnWyvernstakeStateChanged += OnWyvernstakeStateChanged;
        }

        public override void UnhookEvents()
        {
            Context.OnAmmoChange -= OnAmmoChage;
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
            
        }

        private void OnWyvernsFireTimerUpdate(object source, GunLanceEventArgs args)
        {
            
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

        private void OnAmmoChage(object source, GunLanceEventArgs args)
        {
            
        }

        
    }
}
