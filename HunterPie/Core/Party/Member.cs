namespace HunterPie.Core
{
    public class Member
    {

        private string _Name = "";
        private int _Damage;
        private byte _Weapon = 255;

        public string Name
        {
            get => _Name;
            set
            {
                if (value == "" && "" == _Name && Damage > 0)
                {
                    _Name = "Player";
                    _OnSpawn();
                    return;
                }
                else if (value != "" && value != _Name)
                {
                    if (value == "" && Damage > 0)
                    {
                        _OnSpawn();
                    }
                    else
                    {
                        _Name = value;
                        _OnSpawn();
                    }
                }
                else if (value == "" && value != _Name && Damage == 0)
                {
                    _Name = value;
                    _OnSpawn();
                }
            }
        }
        public float DamagePercentage { get; set; }
        public int Damage
        {
            get => _Damage;
            set
            {
                if (value != _Damage)
                {
                    _Damage = value;
                    _OnDamageChange();
                }
            }
        }
        public byte Weapon
        {
            get => _Weapon;
            set
            {
                if (value != _Weapon)
                {
                    if (_Weapon != 255 && value == 255) return;
                    _Weapon = value;
                    WeaponIconName = GetWeaponIconNameByID(value);
                    _OnWeaponChange();
                }
            }
        }
        public string WeaponIconName;
        public bool IsPartyLeader { get; set; }
        public bool IsInParty { get; set; }
        public short HR { get; set; }
        public short MR { get; set; }
        public bool IsMe { get; set; }

        public delegate void PartyMemberEvents(object source, PartyMemberEventArgs args);
        public event PartyMemberEvents OnDamageChange;
        public event PartyMemberEvents OnWeaponChange;
        public event PartyMemberEvents OnSpawn;

        protected virtual void _OnDamageChange() => OnDamageChange?.Invoke(this, new PartyMemberEventArgs(this));

        protected virtual void _OnWeaponChange() => OnWeaponChange?.Invoke(this, new PartyMemberEventArgs(this));

        protected virtual void _OnSpawn() => OnSpawn?.Invoke(this, new PartyMemberEventArgs(this));

        public void SetPlayerInfo(string name, byte weapon_id, int damage, float damagePercentage)
        {
            if (string.IsNullOrEmpty(name) && weapon_id == 0)
            {
                Weapon = Weapon;
            }
            else
            {
                Weapon = weapon_id;
            }
            if (string.IsNullOrEmpty(name) && damage == 0) IsInParty = false;
            else { IsInParty = true; }
            DamagePercentage = damagePercentage;
            Damage = damage;
            Name = name;
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
