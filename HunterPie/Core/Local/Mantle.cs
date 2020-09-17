using HunterPie.Core.Events;

namespace HunterPie.Core
{
    public class Mantle
    {
        private int _id = -1;
        private float _cooldown;
        private float _timer;
        private float _staticCooldown;
        private float _staticTimer;

        public string Name => GStrings.GetMantleNameByID(ID);
        public int ID
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    Dispatch(OnMantleChange);
                }
            }
        }
        public float Cooldown
        {
            get => _cooldown; set
            {
                if (_cooldown != value)
                {
                    _cooldown = value;
                    Dispatch(OnMantleCooldownUpdate);
                }
            }
        }
        public float Timer
        {
            get => _timer; set
            {
                if (_timer != value)
                {
                    _timer = value;
                    Dispatch(OnMantleTimerUpdate);
                }
            }
        }
        public float staticCooldown
        {
            get => _staticCooldown; set
            {
                if (_staticCooldown != value) _staticCooldown = value;
            }
        }
        public float staticTimer
        {
            get => _staticTimer; set
            {
                if (_staticTimer != value) _staticTimer = value;
            }
        }

        public void SetCooldown(float cd, float staticCd)
        {
            if (staticCd < cd)
            {
                staticCooldown = 0;
                Cooldown = 0;
                return;
            }
            // Set the static cooldown first to trigger the event after everything is ready
            staticCooldown = staticCd;
            Cooldown = cd;
        }

        public void SetTimer(float Timer, float staticTimer)
        {
            if (staticTimer < Timer)
            {
                this.staticTimer = 0;
                this.Timer = 0;
                return;
            }
            // Same for timer
            this.staticTimer = staticTimer;
            this.Timer = Timer;
        }

        public void SetID(int newID) => ID = newID;

        // Events

        public delegate void MantleEvents(object source, MantleEventArgs args);
        public event MantleEvents OnMantleCooldownUpdate;
        public event MantleEvents OnMantleTimerUpdate;
        public event MantleEvents OnMantleChange;

        protected virtual void Dispatch(MantleEvents e) => e?.Invoke(this, new MantleEventArgs(this));

    }
}
