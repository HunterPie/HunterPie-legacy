using System;

namespace HunterPie.Core {
    public class MantleEventArgs {
        public string Name;
        public int ID;
        public float Timer;
        public float staticTimer;
        public float Cooldown;
        public float staticCooldown;

        public MantleEventArgs(Mantle m) {
            this.Name = m.Name;
            this.ID = m.ID;
            this.Timer = m.Timer;
            this.staticTimer = m.staticTimer;
            this.Cooldown = m.Cooldown;
            this.staticCooldown = m.staticCooldown;
        }
    }

    public class Mantle {
        private string _name;
        private int _id;
        private float _cooldown;
        private float _timer;
        private float _staticCooldown;
        private float _staticTimer;

        public string Name {
            get {
                return _name;
            } set {
                if (_name != value) {
                    _name = value;
                }
            }
        }
        public int ID {
            get {
                return _id;
            } set {
                if (_id != value) {
                    _id = value;
                    this.onMantleChange();
                }
            }
        }
        public float Cooldown {
            get {
                return _cooldown;
            } set {
                if (_cooldown != value) {
                    _cooldown = value;
                    this.onMantleCooldownUpdate();
                }
            }
        }
        public float Timer {
            get {
                return _timer;
            } set {
                if (_timer != value) {
                    _timer = value;
                    this.onMantleTimerUpdate();
                }
            }
        }
        public float staticCooldown {
            get {
                return _staticCooldown;
            } set {
                if (_staticCooldown != value) _staticCooldown = value;
            }
        }
        public float staticTimer {
            get {
                return _staticTimer;
            } set {
                if (_staticTimer != value) _staticTimer = value;
            }
        }

        public void SetCooldown(float cd, float staticCd) {
            // Set the static cooldown first to trigger the event after everything is ready
            this.staticCooldown = staticCd;
            this.Cooldown = cd;
        }

        public void SetTimer(float Timer, float staticTimer) {
            // Same for timer
            this.staticTimer = staticTimer;
            this.Timer = Timer;
        }

        public void SetID(int newID) {
            this.ID = newID;
        }

        public void SetName(string newName) {
            this.Name = newName;
        }

        // Events

        public delegate void MantleEvents(object source, MantleEventArgs args);
        public event MantleEvents MantleCooldown;
        public event MantleEvents MantleTimer;
        public event MantleEvents MantleChange;

        protected virtual void onMantleCooldownUpdate() {
            MantleEventArgs args = new MantleEventArgs(this);
            MantleCooldown?.Invoke(this, args);
        }

        protected virtual void onMantleTimerUpdate() {
            MantleEventArgs args = new MantleEventArgs(this);
            MantleTimer?.Invoke(this, args);
        }

        protected virtual void onMantleChange() {
            MantleEventArgs args = new MantleEventArgs(this);
            MantleChange?.Invoke(this, args);
        }

    }
}
