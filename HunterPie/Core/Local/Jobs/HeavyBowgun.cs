using System;
using System.Collections.Generic;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Jobs
{
    public struct HeavyBowgunInformation
    {
        public float WyvernsnipeTimer;
        public float WyvernheartTimer;
        public float ScopeZoomMultiplier;
        public bool HasScopeEquipped;
        public sEquippedAmmo EquippedAmmo;
        public HBGSpecialType SpecialAmmoType;
        public int FocusLevel;
    }
    public class HeavyBowgunEventArgs : EventArgs
    {
        public HBGSpecialType SpecialAmmoType { get; }
        public float WyvernsnipeTimer { get; }
        public float WyvernheartTimer { get; }

        public float WyvernsnipeMaxTimer { get; }
        public float WyvernheartMaxAmmo { get; }

        public bool IsTimer { get; }

        public bool HasScopeEquipped { get; }
        public float ScopeZoomMultiplier { get; }
        public sEquippedAmmo EquippedAmmo { get; }
        public int Ammo { get; }

        public IReadOnlyCollection<sAmmo> Ammos { get; }

        public HeavyBowgunEventArgs(HeavyBowgun weapon)
        {
            SpecialAmmoType = weapon.SpecialAmmoType;

            WyvernheartTimer = weapon.WyvernheartTimer;
            WyvernheartMaxAmmo = HeavyBowgun.WyvernheartMaxAmmo;

            WyvernsnipeTimer = weapon.WyvernsnipeTimer / weapon.FocusMultiplier;
            WyvernsnipeMaxTimer = HeavyBowgun.WyvernsniperMaxTimer / weapon.FocusMultiplier;

            IsTimer = weapon.IsTimer;

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
                    IsTimer = value > wyvernsnipeTimer;
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
                    IsTimer = value > wyvernheartTimer;
                    wyvernheartTimer = value;
                    Dispatch(OnWyvernheartUpdate);
                }
            }
        }
        public bool IsTimer { get; private set; }
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
        public float FocusMultiplier { get; private set; } = 1;
        public HBGSpecialType SpecialAmmoType { get; private set; }
        public override int SafijiivaMaxHits => 7;

        public const float WyvernheartMaxAmmo = 50;
        public const float WyvernsniperMaxTimer = 80;

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

            SpecialAmmoType = rawData.SpecialAmmoType;
            WyvernheartTimer = rawData.WyvernheartTimer;
            WyvernsnipeTimer = rawData.WyvernsnipeTimer;

            EquippedAmmo = rawData.EquippedAmmo;

            CalculateFocus(rawData.FocusLevel);
            UpdateAmmoCount();
        }

        private void UpdateAmmoCount()
        {
            if (equippedAmmo.index < ammos.Length && equippedAmmo.index >= 0)
            {
                Ammo = ammos[equippedAmmo.index].Ammo;
            } else
            {
                Ammo = 0;
            }
        }

        private void CalculateFocus(int level)
        {
            switch (level)
            {
                case 1:
                    FocusMultiplier = 1.05f;
                    break;
                case 2:
                    FocusMultiplier = 1.1f;
                    break;
                case 3:
                    FocusMultiplier = 1.15f;
                    break;
                default:
                    FocusMultiplier = 1;
                    break;
            }
        }
    }
}
