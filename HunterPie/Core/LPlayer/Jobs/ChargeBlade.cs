using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class ChargeBladeEventArgs : EventArgs
    {
        public float VialChargeGauge;
        public float ShieldBuffTimer;
        public float SwordBuffTimer;
        public int Vials;

        public ChargeBladeEventArgs(ChargeBlade weapon)
        {
            VialChargeGauge = weapon.VialChargeGauge;
            ShieldBuffTimer = weapon.ShieldBuffTimer;
            SwordBuffTimer = weapon.SwordBuffTimer;
            Vials = weapon.Vials;
        }
    }
    public class ChargeBlade
    {
        private float vialChargeGauge;
        private float shieldBuffTimer;
        private float swordBuffTimer;
        private int vials;

        public float VialChargeGauge
        {
            get => vialChargeGauge;
            set
            {
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

        public delegate void ChargeBladeEvents(object source, ChargeBladeEventArgs args);
        public event ChargeBladeEvents OnVialChargeGaugeChange;
        public event ChargeBladeEvents OnShieldBuffChange;
        public event ChargeBladeEvents OnSwordBuffChange;
        public event ChargeBladeEvents OnVialsChange;

        private void Dispatch(ChargeBladeEvents e) => e?.Invoke(this, new ChargeBladeEventArgs(this));
    }
}
