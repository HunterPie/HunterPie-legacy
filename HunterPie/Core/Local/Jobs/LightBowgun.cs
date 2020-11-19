using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Jobs
{
    public struct LightBowgunInformation
    {
        public float SpecialAmmoRegen;
        public int GroundAmmo;
        public sEquippedAmmo EquippedAmmo;
    }
    public class LightBowgunEventArgs : EventArgs
    {
        public float SpecialAmmoRegen { get; }
        public int GroundAmmo { get; }
        public int Ammo { get; }
        public sEquippedAmmo EquippedAmmo;
        public IReadOnlyCollection<sAmmo> Ammos;

        public LightBowgunEventArgs(LightBowgun weapon)
        {
            SpecialAmmoRegen = weapon.SpecialAmmoRegen;
            GroundAmmo = weapon.GroundAmmo;
            Ammo = weapon.Ammo;
            EquippedAmmo = weapon.EquippedAmmo;
            Ammos = weapon.Ammos;
        }
    }
    public class LightBowgun : Job
    {
        public const float GroundAmmoMaxTimer = 60.0f;

        private float specialAmmoRegen;
        private int groundAmmo;
        private int ammo;
        private sEquippedAmmo equippedAmmo;
        private sAmmo[] ammos;

        public override int SafijiivaMaxHits => 10;
        public override Classes Type => Classes.LightBowgun;
        public override bool IsMelee => false;

        public IReadOnlyCollection<sAmmo> Ammos => ammos;
        public int GroundAmmo
        {
            get => groundAmmo;
            set
            {
                if (value != groundAmmo)
                {
                    groundAmmo = value;
                    Dispatch(OnGroundAmmoCountChange);
                }
            }
        }
        public float SpecialAmmoRegen
        {
            get => specialAmmoRegen;
            private set
            {
                if (value != specialAmmoRegen)
                {
                    specialAmmoRegen = value;
                    Dispatch(OnSpecialAmmoRegenUpdate);
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

        internal void UpdateInformation(LightBowgunInformation rawData, sAmmo[] ammoList)
        {
            ammos = ammoList;
            GroundAmmo = rawData.GroundAmmo;
            SpecialAmmoRegen = rawData.SpecialAmmoRegen;
            EquippedAmmo = rawData.EquippedAmmo;

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

        public delegate void LightBowgunEvents(object source, LightBowgunEventArgs args);
        public event LightBowgunEvents OnSpecialAmmoRegenUpdate;
        public event LightBowgunEvents OnGroundAmmoCountChange;

        public event LightBowgunEvents OnEquippedAmmoChange;
        public event LightBowgunEvents OnAmmoCountChange;

        private void Dispatch(LightBowgunEvents e) => e?.Invoke(this, new LightBowgunEventArgs(this));
    }
}
