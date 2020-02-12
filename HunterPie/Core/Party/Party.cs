using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core {
    public class Party {
        List<Member> Members = new List<Member>();
        public Member this[int index] {
            get { return Members[index]; }
            set { Members[index] = value; }
        }
        public int Size {
            get {
                int x = 0;
                foreach (Member member in Members) {
                    if (member.IsInParty) x++;
                }
                return x;
            }
        }
        public int MaxSize = 4;

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

        
    }
}
