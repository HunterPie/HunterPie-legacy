using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace HunterPie.Core {
    class Game {
        // Game classes
        public Player Player = new Player();
        public Monster FirstMonster = new Monster(1);
        public Monster SecondMonster = new Monster(2);
        public Monster ThirdMonster = new Monster(3);

        // Threading
        ThreadStart ScanGameThreadingRef;
        Thread ScanGameThreading;

        public void StartScanning() {
            StartGameScanner();
            Player.StartScanning();
            FirstMonster.StartThreadingScan();
            SecondMonster.StartThreadingScan();
            ThirdMonster.StartThreadingScan();
            Debugger.Warn("Starting Game scanner");
        }

        public void StopScanning() {
            FirstMonster.StopThread();
            SecondMonster.StopThread();
            ThirdMonster.StopThread();
            Player.StopScanning();
        }

        private void StartGameScanner() {
            ScanGameThreadingRef = new ThreadStart(GameScanner);
            ScanGameThreading = new Thread(ScanGameThreadingRef);
            ScanGameThreading.Name = "Scanner_Game";
            ScanGameThreading.Start();
        }

        private void GameScanner() {
            while (Memory.Scanner.GameIsRunning) {
                PredictTarget();
                Thread.Sleep(1000);
            }
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
