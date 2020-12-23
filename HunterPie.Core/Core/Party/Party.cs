using System;
using System.Collections.Generic;
using System.Linq;

namespace HunterPie.Core
{
    public class Party
    {
        private TimeSpan epoch;

        public List<Member> Members = new List<Member>();
        public TimeSpan Epoch
        {
            get => epoch;
            set
            {
                if (epoch != value)
                {
                    // Reset the time difference when the timer resets
                    if (value < epoch)
                    {
                        TimeDifference = TimeSpan.Zero;
                        Dispatch(OnTimerReset);
                    }
                    epoch = value;
                }
            }
        }
        public TimeSpan TimeDifference = TimeSpan.Zero;
        public bool ShowDPS = true;
        public string PartyHash => $"{Members[0].Name}{Members[1].Name}{Members[2].Name}{Members[3].Name}";
        private int totalDamage;
        public int TotalDamage
        {
            get => totalDamage;
            set
            {
                if (totalDamage != value)
                {
                    if (value > 0 && totalDamage == 0)
                    {
                        TimeDifference = Epoch;
                    } else if (value == 0 && totalDamage > 0)
                    {
                        TimeDifference = TimeSpan.Zero;
                    }
                    totalDamage = value;
                    Dispatch(OnTotalDamageChange);
                }

            }
        }
        public int MaxLobbySize = 16;
        public int LobbySize { get; set; }
        public bool IsLocalHost => this[0].IsMe || Size == 0;
        public Member this[int index]
        {
            get => Members[index];
            set => Members[index] = value;
        }

        public int Size
        {
            get
            {
                return Members.Where(m => m.IsInParty).Count();
            }
        }
        public int MaxSize => 4;

        public Party()
        {
            // Populates party with empty players
            for (int i = 0; i < MaxSize; i++) AddMember(new Member());
        }

        ~Party()
        {
            Members.Clear();
            Members = null;
        }

        public void AddMember(Member pMember) => Members.Add(pMember);

        public void AddMemberAt(Member pMember, int index) => Members.Insert(index, pMember);

        public void RemoveMemberAt(int index) => Members.RemoveAt(index);

        public void Clear() => Members.Clear();

        // Timer event
        public delegate void PartyEvents(object source, EventArgs args);
        public event PartyEvents OnTotalDamageChange;
        public event PartyEvents OnTimerReset;

        private void Dispatch(PartyEvents e) => e?.Invoke(this, EventArgs.Empty);
    }
}
