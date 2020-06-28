using System;
using System.Collections.Generic;

namespace HunterPie.Core
{
    /* Abnormalities */
    public class AbnormalityEventArgs : EventArgs
    {
        public Abnormality Abnormality;

        public AbnormalityEventArgs(Abnormality abnorm) => Abnormality = abnorm;
    }
    /* Activities */
    public class SteamFuelEventArgs : EventArgs
    {
        public int Available;
        public int Max;

        public SteamFuelEventArgs(int available, int max)
        {
            Available = available;
            Max = max;
        }
    }
    public class DaysLeftEventArgs : EventArgs
    {
        public byte Days;
        // Generic name
        // Argosy
        // Tailraiders means
        public bool Modifier;

        public DaysLeftEventArgs(byte Days, bool Modifier = false)
        {
            this.Days = Days;
            this.Modifier = Modifier;
        }
    }
    /* Party and Members */
    public class PartyMemberEventArgs : EventArgs
    {
        public string Name;
        public int Damage;
        public string Weapon;
        public bool IsInParty;

        public PartyMemberEventArgs(Member m)
        {
            Name = m.Name;
            Damage = m.Damage;
            Weapon = m.WeaponIconName;
            IsInParty = m.IsInParty;
        }
    }

    /* HB and Fertilizers */
    public class FertilizerEventArgs : EventArgs
    {
        public int ID;
        public string Name;
        public int Amount;

        public FertilizerEventArgs(Fertilizer m)
        {
            ID = m.ID;
            Name = m.Name;
            Amount = m.Amount;
        }
    }

    public class HarvestBoxEventArgs : EventArgs
    {
        public int Counter;
        public int Max;

        public HarvestBoxEventArgs(HarvestBox m)
        {
            Counter = m.Counter;
            Max = m.Max;
        }

    }

    /* Monster Events */
    public class MonsterAilmentEventArgs : EventArgs
    {
        public string Name;
        public float Duration;
        public float MaxDuration;
        public float Buildup;
        public float MaxBuildup;
        public uint Counter;

        public MonsterAilmentEventArgs(Ailment mAilment)
        {
            Name = mAilment.Name;
            Duration = mAilment.Duration;
            MaxDuration = mAilment.MaxDuration;
            Buildup = mAilment.Buildup;
            MaxBuildup = mAilment.MaxBuildup;
            Counter = mAilment.Counter;
        }
    }


    public class MonsterPartEventArgs : EventArgs
    {
        public MonsterPartEventArgs(Part part)
        {
            Health = part.Health;
            TotalHealth = part.TotalHealth;
            BrokenCounter = part.BrokenCounter;
        }

        public float Health { get; }
        public float TotalHealth { get; }
        public int BrokenCounter { get; }
    }

    public class MonsterSpawnEventArgs : EventArgs
    {
        public string Name;
        public string ID;
        public string Crown;
        public float Health;
        public float MaxHealth;
        public bool IsTarget;
        public Dictionary<string, int> Weaknesses;

        public MonsterSpawnEventArgs(Monster m)
        {
            Name = m.Name;
            ID = m.Id;
            Crown = m.Crown;
            Health = m.Health;
            MaxHealth = m.MaxHealth;
            IsTarget = m.IsTarget;
            Weaknesses = m.Weaknesses;
        }
    }

    public class MonsterUpdateEventArgs : EventArgs
    {
        public float Health;
        public float MaxHealth;
        public float Stamina;
        public float MaxStamina;
        public float Enrage;

        public MonsterUpdateEventArgs(Monster m)
        {
            Health = m.Health;
            MaxHealth = m.MaxHealth;
            Enrage = m.EnrageTimer;
            Stamina = m.Stamina;
            MaxStamina = m.MaxStamina;
        }
    }

}
