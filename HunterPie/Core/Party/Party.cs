using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core {
    public class Party {
        public List<Member> Members = new List<Member>();
        public TimeSpan Epoch;
        public bool ShowDPS = true;
        private int _TotalDamage;
        public int TotalDamage {
            get { return _TotalDamage; }
            set {
                if (_TotalDamage != value) {
                    _TotalDamage = value;
                    _OnTotalDamageChange();
                }
                
            }
        }

        public Member this[int index] {
            get { return Members[index]; }
            set { Members[index] = value; }
        }
        
        public int Size {
            get {
                int x = 0;
                foreach (Member member in Members) {
                    if (member.IsInParty == true) x++;
                }
                return x;
            }
        }
        public int MaxSize = 4;

        public Party() {
            // Populates party with empty players
            for (int i = 0; i < MaxSize; i++) AddMember(new Member());
        }

        public void AddMember(Member pMember) {
            Members.Add(pMember);
        }

        public void AddMemberAt(Member pMember, int index) {
            Members.Insert(index, pMember);
        }

        public void RemoveMemberAt(int index) {
            Members.RemoveAt(index);
        }
        
        public void Clear() {
            Members.Clear();
        }

        // Timer event
        public delegate void PartyEvents(object source, EventArgs args);
        public event PartyEvents OnTotalDamageChange;

        protected virtual void _OnTotalDamageChange() {
            OnTotalDamageChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
