using System;

namespace HunterPie.Core {
    class Mantle {
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
            this.Cooldown = cd;
            this.staticCooldown = staticCd;
        }

        public void SetTimer(float Timer, float staticTimer) {
            this.Timer = Timer;
            this.staticTimer = staticTimer;
        }

        public void SetID(int newID) {
            this.ID = newID;
        }

        public void SetName(string newName) {
            this.Name = newName;
        }

        // Events

        public delegate void MantleEvents(object source, EventArgs args);
        public event MantleEvents MantleCooldown;
        public event MantleEvents MantleTimer;
        public event MantleEvents MantleChange;

        protected virtual void onMantleCooldownUpdate() {
            MantleCooldown?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void onMantleTimerUpdate() {
            MantleTimer?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void onMantleChange() {
            MantleChange?.Invoke(this, EventArgs.Empty);
        }

    }
}
