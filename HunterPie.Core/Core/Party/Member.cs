using HunterPie.Core.Events;

namespace HunterPie.Core
{
    public struct MemberInfo
    {
        public string Name;
        public short HR;
        public short MR;
        public byte WeaponId;
        public bool IsLocalPlayer;
        public bool IsLeader;
        public int Damage;
        public float DamagePercentage;
    }
    public class Member
    {
        private string name = "";
        private int damage;
        private byte weapon = 255;

        // TODO: Fix this spaghetti
        public string Name
        {
            get => name;
            private set
            {
                if (name != value)
                {
                    name = value;
                    if (IsInParty)
                    {
                        Dispatch(OnSpawn);
                    } else
                    {
                        Dispatch(OnDespawn);
                    }
                }
            }
        }
        public float DamagePercentage { get; set; }
        public int Damage
        {
            get => damage;
            private set
            {
                if (value != damage)
                {
                    damage = value;
                    Dispatch(OnDamageChange);
                }
            }
        }
        public byte Weapon
        {
            get => weapon;
            private set
            {
                if (value != weapon)
                {
                    if (weapon != 255 && value == 255)
                        return;

                    weapon = value;
                    WeaponIconName = GetWeaponIconNameByID(value);
                    Dispatch(OnWeaponChange);
                }
            }
        }
        public string WeaponIconName { get; private set; }
        public bool IsPartyLeader { get; internal set; }
        public bool IsInParty { get; private set; }
        public short HR { get; private set; }
        public short MR { get; private set; }
        public bool IsMe { get; private set; }

        public delegate void PartyMemberEvents(object source, PartyMemberEventArgs args);
        public event PartyMemberEvents OnDamageChange;
        public event PartyMemberEvents OnWeaponChange;
        public event PartyMemberEvents OnSpawn;
        public event PartyMemberEvents OnDespawn;

        private void Dispatch(PartyMemberEvents e) => e?.Invoke(this, new PartyMemberEventArgs(this));

        public void SetPlayerInfo(MemberInfo info)
        {
            if (!string.IsNullOrEmpty(info.Name))
            {
                Weapon = info.WeaponId;
            }
            IsInParty = !(string.IsNullOrEmpty(info.Name) && info.Damage == 0);
            DamagePercentage = info.DamagePercentage;
            HR = info.HR;
            MR = info.MR;
            IsMe = info.IsLocalPlayer;
            IsPartyLeader = info.IsLeader;
            Damage = info.Damage;

            // When players leave party after quest ends
            if (IsInParty && string.IsNullOrEmpty(info.Name))
                info.Name = string.IsNullOrEmpty(Name) ? "Player" : Name;

            Name = info.Name;
        }

        private string GetWeaponIconNameByID(int id)
        {
            switch (id)
            {
                case 0:
                    return "ICON_GREATSWORD";
                case 1:
                    return "ICON_SWORDANDSHIELD";
                case 2:
                    return "ICON_DUALBLADES";
                case 3:
                    return "ICON_LONGSWORD";
                case 4:
                    return "ICON_HAMMER";
                case 5:
                    return "ICON_HUNTINGHORN";
                case 6:
                    return "ICON_LANCE";
                case 7:
                    return "ICON_GUNLANCE";
                case 8:
                    return "ICON_SWITCHAXE";
                case 9:
                    return "ICON_CHARGEBLADE";
                case 10:
                    return "ICON_INSECTGLAIVE";
                case 11:
                    return "ICON_BOW";
                case 12:
                    return "ICON_HEAVYBOWGUN";
                case 13:
                    return "ICON_LIGHTBOWGUN";
                default:
                    return null;
            }
        }

    }
}
