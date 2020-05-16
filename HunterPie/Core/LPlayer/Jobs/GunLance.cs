using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class GunLanceEventArgs : EventArgs
    {
        public int TotalAmmo { get; }
        public int Ammo { get; }
        public int TotalBigAmmo { get; }
        public int BigAmmo { get; }
        public float WyvernsFireTimer { get; }
        public float WyvernstakeBlastTimer { get; }
        public float WyvernstakeMax { get; }
        public float WyvernstakeNextMax { get; }
        public bool HasWyvernstakeLoaded { get; }

        public GunLanceEventArgs(GunLance weapon)
        {
            TotalAmmo = weapon.TotalAmmo;
            Ammo = weapon.Ammo;
            TotalBigAmmo = weapon.TotalBigAmmo;
            BigAmmo = weapon.BigAmmo;
            WyvernsFireTimer = weapon.WyvernsFireTimer;
            WyvernstakeBlastTimer = weapon.WyvernstakeBlastTimer;
            HasWyvernstakeLoaded = weapon.HasWyvernstakeLoaded;
            WyvernstakeMax = weapon.WyvernstakeMax;
            WyvernstakeNextMax = weapon.WyvernstakeNextMax;
        }
    }
    public class GunLance : Job
    {
        private int totalAmmo;
        private int ammo;
        private int totalBigAmmo;
        private int bigAmmo;
        private float wyvernsFireTimer;
        private float wyvernstakeBlastTimer;
        private bool hasWyvernstakeLoaded;

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
        public float WyvernsFireTimer
        {
            get => wyvernsFireTimer;
            set
            {
                if (value != wyvernsFireTimer)
                {
                    wyvernsFireTimer = value;
                    Dispatch(OnWyvernsFireTimerUpdate);
                }
            }
        }
        public float WyvernstakeBlastTimer
        {
            get => wyvernstakeBlastTimer;
            set
            {
                if (value != wyvernstakeBlastTimer)
                {
                    wyvernstakeBlastTimer = value;
                    Dispatch(OnWyvernstakeBlastTimerUpdate);
                }
            }
        }
        public float WyvernstakeMax { get; set; }
        public float WyvernstakeNextMax { get; set; }
        public bool HasWyvernstakeLoaded
        {
            get => hasWyvernstakeLoaded;
            set
            {
                if (value != hasWyvernstakeLoaded)
                {
                    hasWyvernstakeLoaded = value;
                    Dispatch(OnWyvernstakeStateChanged);
                }
            }
        }
        public override int SafijiivaMaxHits => 8;

        public delegate void GunLanceEvents(object source, GunLanceEventArgs args);
        public event GunLanceEvents OnTotalAmmoChange;
        public event GunLanceEvents OnAmmoChange;
        public event GunLanceEvents OnTotalBigAmmoChange;
        public event GunLanceEvents OnBigAmmoChange;
        public event GunLanceEvents OnWyvernsFireTimerUpdate;
        public event GunLanceEvents OnWyvernstakeBlastTimerUpdate;
        public event GunLanceEvents OnWyvernstakeStateChanged;

        public void Dispatch(GunLanceEvents e) => e?.Invoke(this, new GunLanceEventArgs(this));

    }
}
