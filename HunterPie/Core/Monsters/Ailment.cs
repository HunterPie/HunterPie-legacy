using System.Linq;
using HunterPie.Core.Definitions;

namespace HunterPie.Core
{
    public class Ailment
    {

        private float buildup;
        private float duration;
        private uint counter;

        public long Address { get; private set; }
        public string Name => GStrings.GetAilmentNameByID(MonsterData.AilmentsInfo.ElementAtOrDefault((int)Id)?.Id ?? Id.ToString());
        public string Group => MonsterData.AilmentsInfo.ElementAtOrDefault((int)Id)?.Group ?? "DEFAULT";
        public uint Id { get; set; }
        public float Buildup
        {
            get => buildup;
            set
            {
                if (value != buildup)
                {
                    buildup = value;
                    Dispatch(OnBuildupChange);
                }
            }
        }
        public float MaxBuildup { get; private set; }
        public float Duration
        {
            get => duration;
            set
            {
                if (value != duration)
                {
                    duration = value;
                    Dispatch(OnDurationChange);
                }
            }
        }
        public float MaxDuration { get; private set; }
        public uint Counter
        {
            get => counter;
            set
            {
                if (value != counter)
                {
                    counter = value;
                    Dispatch(OnCounterChange);
                }
            }
        }

        #region Events
        public delegate void MonsterAilmentEvents(object source, MonsterAilmentEventArgs args);
        public event MonsterAilmentEvents OnBuildupChange;
        public event MonsterAilmentEvents OnDurationChange;
        public event MonsterAilmentEvents OnCounterChange;

        private void Dispatch(MonsterAilmentEvents e) => e?.Invoke(this, new MonsterAilmentEventArgs(this));
        #endregion

        public Ailment(long address) => Address = address;

        public void SetAilmentInfo(sMonsterAilment AilmentData)
        {
            Id = AilmentData.Id;
            MaxDuration = AilmentData.MaxDuration;
            Duration = AilmentData.Duration;
            MaxBuildup = AilmentData.MaxBuildup;
            Buildup = AilmentData.Buildup;
            Counter = AilmentData.Counter;
        }

        public override string ToString() => $"Ailment: {Name} ({Id}) | Duration: {Duration}/{MaxDuration} | Buildup: {Buildup}/{MaxBuildup} | {Counter}";

        private void UnhookEvents(MonsterAilmentEvents eventHandler)
        {
            if (eventHandler == null) return;
            foreach (MonsterAilmentEvents d in eventHandler.GetInvocationList())
            {
                eventHandler -= d;
            }
        }

        public void Destroy()
        {
            UnhookEvents(OnBuildupChange);
            UnhookEvents(OnCounterChange);
            UnhookEvents(OnDurationChange);
        }
    }
}
