using System;
using System.Collections.Generic;

namespace HunterPie.Core {
    /* Abnormalities */
    public class AbnormalityEventArgs : EventArgs {
        public Abnormality Abnormality;

        public AbnormalityEventArgs(Abnormality abnorm) {
            this.Abnormality = abnorm;
        }
    }
    /* Activities */
    public class SteamFuelEventArgs : EventArgs {
        public int Available;
        public int Max;

        public SteamFuelEventArgs(int available, int max) {
            this.Available = available;
            this.Max = max;
        }
    }
    public class DaysLeftEventArgs : EventArgs {
        public byte Days;
        // Generic name
        // Argosy
        // Tailraiders means
        public bool Modifier;

        public DaysLeftEventArgs(byte Days, bool Modifier = false) {
            this.Days = Days;
            this.Modifier = Modifier;
        }
    }
    /* Keyboard input */
    public class KeyboardInputEventArgs : EventArgs {
        public int Key { get; private set; }
        public KeyboardHookHelper.KeyboardMessage KeyMessage { get; private set; }

        public KeyboardInputEventArgs(int KeyCode, KeyboardHookHelper.KeyboardMessage Message) {
            this.Key = KeyCode;
            this.KeyMessage = Message;
        }

    }
    /* Party and Members */
    public class PartyMemberEventArgs : EventArgs {
        public string Name;
        public int Damage;
        public string Weapon;
        public bool IsInParty;

        public PartyMemberEventArgs(Member m) {
            this.Name = m.Name;
            this.Damage = m.Damage;
            this.Weapon = m.WeaponIconName;
            this.IsInParty = m.IsInParty;
        }
    }

    /* HB and Fertilizers */
    public class FertilizerEventArgs : EventArgs {
        public int ID;
        public string Name;
        public int Amount;

        public FertilizerEventArgs(Fertilizer m) {
            this.ID = m.ID;
            this.Name = m.Name;
            this.Amount = m.Amount;
        }
    }

    public class HarvestBoxEventArgs : EventArgs {
        public int Counter;
        public int Max;

        public HarvestBoxEventArgs(HarvestBox m) {
            this.Counter = m.Counter;
            this.Max = m.Max;
        }

    }

    /* Monster Events */
    public class MonsterAilmentEventArgs : EventArgs {
        public string Name;
        public float Duration;
        public float MaxDuration;
        public float Buildup;
        public float MaxBuildup;
        public byte Counter;

        public MonsterAilmentEventArgs(Ailment mAilment) {
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
        public byte BrokenCounter { get; }
    }

    public class MonsterSpawnEventArgs : EventArgs {
        public string Name;
        public string ID;
        public string Crown;
        public float CurrentHP;
        public float TotalHP;
        public bool IsTarget;
        public Dictionary<string, int> Weaknesses;

        public MonsterSpawnEventArgs(Monster m) {
            this.Name = m.Name;
            this.ID = m.Id;
            this.Crown = m.Crown;
            this.CurrentHP = m.CurrentHP;
            this.TotalHP = m.TotalHP;
            this.IsTarget = m.IsTarget;
            this.Weaknesses = m.Weaknesses;
        }
    }

    public class MonsterUpdateEventArgs : EventArgs {
        public float CurrentHP;
        public float TotalHP;
        public float Stamina;
        public float MaxStamina;
        public float Enrage;

        public MonsterUpdateEventArgs(Monster m) {
            this.CurrentHP = m.CurrentHP;
            this.TotalHP = m.TotalHP;
            this.Enrage = m.EnrageTimer;
            this.Stamina = m.Stamina;
            this.MaxStamina = m.MaxStamina;
        }
    }

}
