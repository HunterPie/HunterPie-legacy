using HunterPie.Core.Definitions;
using HunterPie.Core.Monsters;
using HunterPie.Core.Events;
using System.Linq;

namespace HunterPie.Core
{
    public class Part
    {
        private readonly PartInfo partInfo;
        private readonly int id; // Part index

        private float health;
        private float totalHealth;
        private int brokenCounter;
        private float tDuration;

        // For debugging purposes
        public sMonsterPartData cMonsterPartData { get; private set; }
        public sTenderizedPart cTenderizedPart { get; private set; }

        public Part(Monster owner, PartInfo partInfo, int index)
        {
            Owner = owner;
            this.partInfo = partInfo;
            id = index;
            HasBreakConditions = BreakThresholds.Where(p => p.HasConditions).Count() > 0;
        }

        public Monster Owner { get; private set; }

        public long Address { get; set; } // So we don't need to re-scan the address everytime

        public string Name => GStrings.GetMonsterPartByID(partInfo.Id);

        public ThresholdInfo[] BreakThresholds => partInfo.BreakThresholds;

        public bool HasBreakConditions { get; private set; }

        public int BrokenCounter
        {
            get => brokenCounter;
            set
            {
                if (value != brokenCounter)
                {
                    brokenCounter = value;
                    Dispatch(OnBrokenCounterChange);
                }
            }
        }

        public float Health
        {
            get => health;
            set
            {
                if (value != health)
                {
                    health = value;
                    Dispatch(OnHealthChange);
                }
            }
        }

        public float TotalHealth
        {
            get => totalHealth;
            set
            {
                if (value != totalHealth)
                {
                    totalHealth = value;
                }
            }
        }

        public bool IsRemovable { get; set; }
        public string Group { get; set; }

        public uint[] TenderizedIds { get; set; }
        public float TenderizeDuration
        {
            get => tDuration;
            set
            {
                if (value != tDuration)
                {
                    tDuration = value;
                    Dispatch(OnTenderizeStateChange);
                }
            }
        }
        public float TenderizeMaxDuration { get; private set; }
        #region Events

        public delegate void MonsterPartEvents(object source, MonsterPartEventArgs args);

        public event MonsterPartEvents OnHealthChange;
        public event MonsterPartEvents OnBrokenCounterChange;
        public event MonsterPartEvents OnTenderizeStateChange;

        private void Dispatch(MonsterPartEvents e) =>
            e?.Invoke(this, new MonsterPartEventArgs(this));

        #endregion

        public void SetPartInfo(sMonsterPartData data, bool IsPartyHost)
        {
            cMonsterPartData = data;

            TotalHealth = data.MaxHealth;
            BrokenCounter = data.Counter;

            if (IsPartyHost)
            {
                Health = data.Health;
            }
        }

        public void SetTenderizeInfo(sTenderizedPart data)
        {
            cTenderizedPart = data;

            float tenderize = data.Duration + data.ExtraDuration;
            TenderizeMaxDuration = data.MaxDuration + data.MaxExtraDuration;
            // Reset the tenderize duration when it reaches the maximum duration
            TenderizeDuration = tenderize == TenderizeMaxDuration ? 0 : tenderize;
        }

        private void UnhookEvents(MonsterPartEvents eventHandler)
        {
            if (eventHandler == null) return;
            foreach (MonsterPartEvents d in eventHandler.GetInvocationList())
            {
                eventHandler -= d;
            }
        }

        public void Destroy()
        {
            UnhookEvents(OnHealthChange);
            UnhookEvents(OnBrokenCounterChange);
            UnhookEvents(OnTenderizeStateChange);
        }

        public override string ToString() => $"Name: {Name} | ID: {id} | HP: {Health}/{TotalHealth} | Counter: {BrokenCounter}";
    }
}
