using HunterPie.Core.LPlayer;

namespace HunterPie.Core
{
    public class Abnormality
    {
        private int duration;
        private byte stack;

        public Abnormality(AbnormalityInfo info)
        {
            Type = info.Type;
            Id = info.Id;
            InternalID = info.InternalId;
            IsDebuff = info.IsDebuff;
            IsInfinite = info.IsInfinite;
            Icon = info.IconName;
            IsPercentageBuff = info.IsPercentageBuff;
            MaxTimer = info.MaxTimer;
        }

        public string Name => GStrings.GetAbnormalityByID(Type, Id, Stack);
        public string Icon { get; private set; }
        public string Type { get; private set; }
        public int Id { get; private set; }

        public byte Stack
        {
            get => stack;
            private set
            {
                if (stack != value)
                {
                    stack = value;
                    _OnStackChange();
                }
            }
        }

        public string InternalID { get; private set; }
        public bool IsInfinite { get; private set; }
        public int Duration
        {
            get => duration;
            private set
            {
                if (value <= 0 && !IsInfinite)
                {
                    _OnAbnormalityEnd();
                    return;
                }
                if (value != duration)
                {
                    duration = value;
                    _OnAbnormalityUpdate();
                }
            }
        }
        public float MaxDuration { get; private set; }
        public float DurationPercentage { get; private set; }
        public bool IsDebuff { get; private set; }

        public bool IsPercentageBuff { get; private set; }
        public float MaxTimer { get; private set; }

        #region Events

        public delegate void AbnormalityEvents(object source, AbnormalityEventArgs args);
        public event AbnormalityEvents OnAbnormalityStart;
        public event AbnormalityEvents OnStackChange;
        public event AbnormalityEvents OnAbnormalityUpdate;
        public event AbnormalityEvents OnAbnormalityEnd;

        protected virtual void _OnAbnormalityStart() => OnAbnormalityStart?.Invoke(this, new AbnormalityEventArgs(this));

        protected virtual void _OnAbnormalityUpdate() => OnAbnormalityUpdate?.Invoke(this, new AbnormalityEventArgs(this));

        protected virtual void _OnAbnormalityEnd() => OnAbnormalityEnd?.Invoke(this, new AbnormalityEventArgs(this));

        protected virtual void _OnStackChange() => OnStackChange?.Invoke(this, new AbnormalityEventArgs(this));

        #endregion

        #region Methods

        public void UpdateAbnormalityInfo(float newDuration, byte newStack)
        {
            Stack = newStack;
            MaxDuration = MaxDuration < newDuration ? newDuration : MaxDuration;
            DurationPercentage = MaxDuration > 0 ? newDuration / MaxDuration : 1;
            Duration = (int)newDuration;
        }

        public void ResetDuration()
        {
            IsInfinite = false;
            Duration = 0;
        }

        #endregion
    }
}
