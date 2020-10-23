using HunterPie.Core.Definitions;
using HunterPie.Core.Monsters;
using HunterPie.Core.Events;

namespace HunterPie.Core
{
    public class Part
    {
        private readonly MonsterInfo monsterInfo;
        private readonly PartInfo partInfo;
        private readonly int id; // Part index

        private float health;
        private float totalHealth;
        private int brokenCounter;
        private float tDuration;

        public Part(MonsterInfo monsterInfo, PartInfo partInfo, int index)
        {
            this.monsterInfo = monsterInfo;
            this.partInfo = partInfo;
            id = index;
        }

        public long Address { get; set; } // So we don't need to re-scan the address everytime

        public string Name => GStrings.GetMonsterPartByID(partInfo.Id);

        public int[] BreakThresholds => partInfo.BreakThresholds;

        public int BrokenCounter
        {
            get => brokenCounter;
            set
            {
                if (value != brokenCounter)
                {
                    brokenCounter = value;
                    NotifyBrokenCounterChanged();
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
                    NotifyHealthChanged();
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
                    NotifyTenderizeStateChangd();
                }
            }
        }
        public float TenderizeMaxDuration { get; private set; }
        #region Events

        public delegate void MonsterPartEvents(object source, MonsterPartEventArgs args);

        public event MonsterPartEvents OnHealthChange;
        public event MonsterPartEvents OnBrokenCounterChange;
        public event MonsterPartEvents OnTenderizeStateChange;

        protected virtual void NotifyHealthChanged() => OnHealthChange?.Invoke(this, new MonsterPartEventArgs(this));
        protected virtual void NotifyBrokenCounterChanged()
        {
            OnBrokenCounterChange?.Invoke(this, new MonsterPartEventArgs(this));
            Logger.Debugger.Debug($"Broken {GStrings.GetMonsterNameByID(monsterInfo.Em)} ({monsterInfo.Id}) part {Name} ({id}), {TotalHealth} hp for {brokenCounter} time");
        }
        protected virtual void NotifyTenderizeStateChangd() => OnTenderizeStateChange?.Invoke(this, new MonsterPartEventArgs(this));
        #endregion

        public void SetPartInfo(sMonsterPartData data)
        {
            TotalHealth = data.MaxHealth;
            BrokenCounter = data.Counter;
            Health = data.Health;
        }

        public void SetTenderizeInfo(sTenderizedPart data)
        {
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
