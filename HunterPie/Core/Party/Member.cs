using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HunterPie.Core;
using HunterPie.Logger;

namespace HunterPie.Core {
    public class Member {

        private string _Name;
        private int _Damage;
        private int _Weapon = 255;

        public string Name {
            get { return _Name; }
            set {
                if (value == null && value == _Name && Damage > 0) {
                    _Name = "Player";
                    _OnSpawn();
                    return;
                }
                if (value != null && value != _Name) {
                    if (value == null && Damage > 0) {
                        _OnSpawn();
                    } else {
                        _Name = value;
                        _OnSpawn();
                    }
                }
                if (value == null && value != _Name && Damage == 0) {
                    _Name = value;
                    _OnSpawn();
                }
            }
        }
        public int Damage {
            get { return _Damage; }
            set {
                if (value != _Damage) {
                    _Damage = value;
                    _OnDamageChange();
                }
            }
        }
        public int Weapon {
            get { return _Weapon; }
            set {
                if (value != _Weapon) {
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

        public delegate void PartyMemberEvents(object source, PartyMemberEventArgs args);
        public event PartyMemberEvents OnDamageChange;
        public event PartyMemberEvents OnWeaponChange;
        public event PartyMemberEvents OnSpawn;

        protected virtual void _OnDamageChange() {
            OnDamageChange?.Invoke(this, new PartyMemberEventArgs(this));
        }

        protected virtual void _OnWeaponChange() {
            OnWeaponChange?.Invoke(this, new PartyMemberEventArgs(this));
        }

        protected virtual void _OnSpawn() {
            OnSpawn?.Invoke(this, new PartyMemberEventArgs(this));
        }

        private string GetWeaponIconNameByID(int id) {
            switch(id) {
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
