using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class ChargeBladeEventArgs : EventArgs
    {
        public float VialChargeGauge;
        public float ShieldBuffTimer;
        public float SwordBuffTimer;
        public float PoweraxeTimer;
        public int Vials;

        public ChargeBladeEventArgs(ChargeBlade weapon)
        {
            VialChargeGauge = weapon.VialChargeGauge;
            ShieldBuffTimer = weapon.ShieldBuffTimer;
            SwordBuffTimer = weapon.SwordBuffTimer;
            PoweraxeTimer = weapon.PoweraxeTimer;
            Vials = weapon.Vials;
        }
    }
    public class ChargeBlade : Job
    {
        private float vialChargeGauge;
        private float shieldBuffTimer;
        private float swordBuffTimer;
        private int vials;
        private float poweraxeTimer;

        public float VialChargeGauge
        {
            get => vialChargeGauge;
            set
            {
                if (value > TimeSpan.MaxValue.TotalSeconds) return;
                if (value != vialChargeGauge)
                {
                    vialChargeGauge = value;
                    Dispatch(OnVialChargeGaugeChange);
                }
            }
        }
        public float ShieldBuffTimer
        {
            get => shieldBuffTimer;
            set
            {
                if (value > TimeSpan.MaxValue.TotalSeconds) return;
                if (value != shieldBuffTimer)
                {
                    shieldBuffTimer = value;
                    Dispatch(OnShieldBuffChange);
                }
            }
        }
        public float SwordBuffTimer
        {
            get => swordBuffTimer;
            set
            {
                if (value > TimeSpan.MaxValue.TotalSeconds) return;
                if (value != swordBuffTimer)
                {
                    swordBuffTimer = value;
                    Dispatch(OnSwordBuffChange);
                }
            }
        }
        public int Vials
        {
            get => vials;
            set
            {
                if (value != vials)
                {
                    vials = value;
                    Dispatch(OnVialsChange);
                }
            }
        }
        public float PoweraxeTimer
        {
            get => poweraxeTimer;
            set
            {
                if (value > TimeSpan.MaxValue.TotalSeconds) return;
                if (value != poweraxeTimer)
                {
                    poweraxeTimer = value;
                    Dispatch(OnPoweraxeBuffChange);
                }
            }
        }
        public override int SafijiivaMaxHits => 8;

        public delegate void ChargeBladeEvents(object source, ChargeBladeEventArgs args);
        public event ChargeBladeEvents OnVialChargeGaugeChange;
        public event ChargeBladeEvents OnShieldBuffChange;
        public event ChargeBladeEvents OnSwordBuffChange;
        public event ChargeBladeEvents OnVialsChange;
        public event ChargeBladeEvents OnPoweraxeBuffChange;

        private void Dispatch(ChargeBladeEvents e) => e?.Invoke(this, new ChargeBladeEventArgs(this));
    }
}
