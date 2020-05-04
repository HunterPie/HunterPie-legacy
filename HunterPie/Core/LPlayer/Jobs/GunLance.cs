using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class GunLanceEventArgs : EventArgs
    {
        public int TotalAmmo;
        public int Ammo;
        public int TotalBigAmmo;
        public int BigAmmo;
        public float WyvernblastTimer;

        public GunLanceEventArgs(GunLance weapon)
        {
            TotalAmmo = weapon.TotalAmmo;
            Ammo = weapon.Ammo;
            TotalBigAmmo = weapon.TotalBigAmmo;
            BigAmmo = weapon.BigAmmo;
            WyvernblastTimer = weapon.WyvernblastTimer;
        }
    }
    public class GunLance
    {
        private int totalAmmo;
        private int ammo;
        private int totalBigAmmo;
        private int bigAmmo;
        private float wyvernblastTimer;

        public int TotalAmmo
        {
            get => totalAmmo;
            set
            {
                if (value != totalAmmo)
                {
                    totalAmmo = value;
                    Dispatch(OnTotalAmmoChange);
                }
            }
        }
        public int Ammo
        {
            get => ammo;
            set
            {
                if (value != ammo)
                {
                    ammo = value;
                    Dispatch(OnAmmoChange);
                }
            }
        }
        public int TotalBigAmmo
        {
            get => totalBigAmmo;
            set
            {
                if (value != totalBigAmmo)
                {
                    totalBigAmmo = value;
                    Dispatch(OnTotalBigAmmoChange);
                }
            }
        }
        public int BigAmmo
        {
            get => bigAmmo;
            set
            {
                if (value != bigAmmo)
                {
                    bigAmmo = value;
                    Dispatch(OnBigAmmoChange);
                }
            }
        }
        public float WyvernblastTimer
        {
            get => wyvernblastTimer;
            set
            {
                if (value != wyvernblastTimer)
                {
                    wyvernblastTimer = value;
                    Dispatch(OnWyvernblastTimerChange);
                }
            }
        }

        public delegate void GunLanceEvents(object source, GunLanceEventArgs args);
        public event GunLanceEvents OnTotalAmmoChange;
        public event GunLanceEvents OnAmmoChange;
        public event GunLanceEvents OnTotalBigAmmoChange;
        public event GunLanceEvents OnBigAmmoChange;
        public event GunLanceEvents OnWyvernblastTimerChange;

        public void Dispatch(GunLanceEvents e) => e?.Invoke(this, new GunLanceEventArgs(this));

    }
}
