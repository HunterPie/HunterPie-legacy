using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HunterPie.Core;

namespace HunterPie.Core {
    public class Member {
        private int _Damage;
        private int _Weapon;

        public string Name { get; set; }
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
                    _Weapon = value;
                    _OnWeaponChange();
                }
            }
        }
        public bool IsPartyLeader { get; set; }
        public bool IsInParty { get; set; }

        public delegate void PartyMemberEvents(object source, PartyMemberEventArgs args);
        public event PartyMemberEvents OnDamageChange;
        public event PartyMemberEvents OnWeaponChange;

        protected virtual void _OnDamageChange() {
            OnDamageChange?.Invoke(this, new PartyMemberEventArgs(this));
        }

        protected virtual void _OnWeaponChange() {
            OnWeaponChange?.Invoke(this, new PartyMemberEventArgs(this));
        }


    }
}
