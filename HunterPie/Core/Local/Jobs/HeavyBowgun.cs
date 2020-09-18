using System;
using System.Collections.Generic;
using HunterPie.Core.Definitions;
using HunterPie.Logger;
using Newtonsoft.Json;

namespace HunterPie.Core.Jobs
{
    public struct HeavyBowgunInformation
    {
        public float WyvernsnipeTimer;
        public float WyvernheartTimer;
        public float ScopeZoomMultiplier;
        public bool HasScopeEquipped;
        public sEquippedAmmo EquippedAmmo;
    }
    public class HeavyBowgunEventArgs : EventArgs
    {
        public float WyvernsnipeTimer { get; }
        public float WyvernheartTimer { get; }

        public bool HasScopeEquipped { get; }
        public float ScopeZoomMultiplier { get; }
        public sEquippedAmmo EquippedAmmo { get; }
        public int Ammo { get; }

        public IReadOnlyCollection<sAmmo> Ammos { get; }

        public HeavyBowgunEventArgs(HeavyBowgun weapon)
        {
            WyvernsnipeTimer = weapon.WyvernsnipeTimer;
            WyvernheartTimer = weapon.WyvernheartTimer;

            HasScopeEquipped = weapon.HasScopeEquipped;
            ScopeZoomMultiplier = weapon.ScopeZoomMultiplier;
            EquippedAmmo = weapon.EquippedAmmo;
            Ammo = weapon.Ammo;
            Ammos = weapon.Ammos;
        }
    }
    public class HeavyBowgun : Job
    {
        private float wyvernsnipeTimer;
        private float wyvernheartTimer;
        private float scopeZoomMultiplier;
        private bool hasScopeEquipped;
        private int ammo;
        private sEquippedAmmo equippedAmmo;
        private sAmmo[] ammos;

        public IReadOnlyCollection<sAmmo> Ammos => ammos;
        public float ScopeZoomMultiplier
        {
            get => scopeZoomMultiplier;
            private set
            {
                if (value != scopeZoomMultiplier)
                {
                    scopeZoomMultiplier = value;
                    Dispatch(OnScopeMultiplierChange);
                }
            }
        }
        public bool HasScopeEquipped
        {
            get => hasScopeEquipped;
            private set
            {
                if (value != hasScopeEquipped)
                {
                    hasScopeEquipped = value;
                    Dispatch(OnScopeStateChange);
                }
            }
        }
        public float WyvernsnipeTimer
        {
            get => wyvernsnipeTimer;
            private set
            {
                if (value != wyvernsnipeTimer)
                {
                    wyvernsnipeTimer = value;
                    Dispatch(OnWyvernsnipeUpdate);
                }
            }
        }
        public float WyvernheartTimer
        {
            get => wyvernheartTimer;
            private set
            {
                if (value != wyvernheartTimer)
                {
                    wyvernheartTimer = value;
                    Dispatch(OnWyvernheartUpdate);
                }
            }
        }
        public sEquippedAmmo EquippedAmmo
        {
            get => equippedAmmo;
            private set
            {
                if (!equippedAmmo.Equals(value))
                {
                    equippedAmmo = value;
                    Dispatch(OnEquippedAmmoChange);
                }
            }
        }
        public int Ammo
        {
            get => ammo;
            private set
            {
                if (ammo != value)
                {
                    ammo = value;
                    Dispatch(OnAmmoCountChange);
                }
            }
        }
        public override int SafijiivaMaxHits => 7;

        public delegate void HeavyBowgunEvents(object source, HeavyBowgunEventArgs args);
        public event HeavyBowgunEvents OnWyvernsnipeUpdate;
        public event HeavyBowgunEvents OnWyvernheartUpdate;

        public event HeavyBowgunEvents OnEquippedAmmoChange;
        public event HeavyBowgunEvents OnAmmoCountChange;

        public event HeavyBowgunEvents OnScopeMultiplierChange;
        public event HeavyBowgunEvents OnScopeStateChange;

        private void Dispatch(HeavyBowgunEvents e) => e?.Invoke(this, new HeavyBowgunEventArgs(this));

        internal void UpdateInformation(HeavyBowgunInformation rawData, sAmmo[] ammoList)
        {
            ammos = ammoList;

            HasScopeEquipped = rawData.HasScopeEquipped;
            ScopeZoomMultiplier = rawData.ScopeZoomMultiplier;

            WyvernheartTimer = rawData.WyvernheartTimer;
            WyvernsnipeTimer = rawData.WyvernsnipeTimer;

            equippedAmmo = rawData.EquippedAmmo;

            UpdateAmmoCount();
        }

        private void UpdateAmmoCount()
        {
            if (equippedAmmo.index < ammos.Length)
            {
                Ammo = ammos[equippedAmmo.index].Ammo;
            } else
            {
                Ammo = 0;
            }
        }
    }
}
