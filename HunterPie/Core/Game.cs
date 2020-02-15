using System;
using System.Threading;
using System.Collections.Generic;
using HunterPie.Logger;

namespace HunterPie.Core {
    public class Game {
        // Game classes
        public Player Player = new Player();
        public Monster FirstMonster = new Monster(1);
        public Monster SecondMonster = new Monster(2);
        public Monster ThirdMonster = new Monster(3);
        public Monster HuntedMonster {
            get {
                if (FirstMonster.isTarget) {
                    return FirstMonster;
                } else if (SecondMonster.isTarget) {
                    return SecondMonster;
                } else if (ThirdMonster.isTarget) {
                    return ThirdMonster;
                } else {
                    return null;
                }
            }
        }
        private DateTime _clock = DateTime.UtcNow;
        private DateTime Clock {
            get => _clock;
            set {
                if (value != _clock) {
                    _clock = value;
                    _onClockChange();
                }
            }
        }
        public DateTime Time { get; private set; }
        public bool IsActive { get; private set; }

        // Threading
        ThreadStart ScanGameThreadingRef;
        Thread ScanGameThreading;

        // Clock event
        
        public delegate void ClockEvent(object source, EventArgs args);
        /* This Event is dispatched every 10 seconds to update the rich presence */
        public event ClockEvent OnClockChange;

        protected virtual void _onClockChange() {
            OnClockChange?.Invoke(this, EventArgs.Empty);
        }

        public void StartScanning() {
            StartGameScanner();
            HookEvents();
            Player.StartScanning();
            FirstMonster.StartThreadingScan();
            SecondMonster.StartThreadingScan();
            ThirdMonster.StartThreadingScan();
            Debugger.Warn("Starting Game scanner");
            IsActive = true;
        }

        public void StopScanning() {
            Debugger.Warn("Stopping Game scanner");
            UnhookEvents();
            FirstMonster.StopThread();
            SecondMonster.StopThread();
            ThirdMonster.StopThread();
            Player.StopScanning();
            ScanGameThreading.Abort();
            IsActive = false;
        }

        private void HookEvents() {
            Player.OnZoneChange += OnZoneChange;
        }

        private void UnhookEvents() {
            Player.OnZoneChange -= OnZoneChange;
        }

        public void OnZoneChange(object source, EventArgs e) {
            Time = DateTime.UtcNow;
        }

        private void StartGameScanner() {
            ScanGameThreadingRef = new ThreadStart(GameScanner);
            ScanGameThreading = new Thread(ScanGameThreadingRef) {
                Name = "Scanner_Game"
            };
            ScanGameThreading.Start();
        }

        private void GameScanner() {
            
            while (Memory.Scanner.GameIsRunning) {
                PredictTarget();
                if (DateTime.UtcNow - Clock >= new TimeSpan(0, 0, 10)) {
                    Clock = DateTime.UtcNow;
                }
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            GameScanner();
        }

        private void PredictTarget() {
            float minimum = Math.Min(FirstMonster.HPPercentage, SecondMonster.HPPercentage);
            minimum = Math.Min(minimum, ThirdMonster.HPPercentage);
            if (minimum == 1) {
                FirstMonster.isTarget = false;
                SecondMonster.isTarget = false;
                ThirdMonster.isTarget = false;
                return;
            }
            if (minimum == FirstMonster.HPPercentage) {
                FirstMonster.isTarget = true;
                SecondMonster.isTarget = false;
                ThirdMonster.isTarget = false;
            } else if (minimum == SecondMonster.HPPercentage) {
                FirstMonster.isTarget = false;
                SecondMonster.isTarget = true;
                ThirdMonster.isTarget = false;
            } else if (minimum == ThirdMonster.HPPercentage) {
                FirstMonster.isTarget = false;
                SecondMonster.isTarget = false;
                ThirdMonster.isTarget = true;
            } else {
                FirstMonster.isTarget = false;
                SecondMonster.isTarget = false;
                ThirdMonster.isTarget = false;
            }
        }

    }
}
