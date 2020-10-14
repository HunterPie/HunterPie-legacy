using System;
using System.Threading;
using HunterPie.Logger;
using HunterPie.Memory;

namespace HunterPie.Core
{

    public class Game
    {
        // Game classes
        public Player Player { get; private set; }

        public Monster FirstMonster { get; private set; }
        public Monster SecondMonster { get; private set; }
        public Monster ThirdMonster { get; private set; }

        public Monster HuntedMonster
        {
            get
            {
                if (FirstMonster.IsTarget)
                {
                    return FirstMonster;
                }
                else if (SecondMonster.IsTarget)
                {
                    return SecondMonster;
                }
                else if (ThirdMonster.IsTarget)
                {
                    return ThirdMonster;
                }
                else
                {
                    return null;
                }
            }
        }
        private DateTime clock = DateTime.UtcNow;
        private DateTime Clock
        {
            get => clock;
            set
            {
                if (value != clock)
                {
                    clock = value;
                    _onClockChange();
                }
            }
        }
        public DateTime? Time { get; private set; }
        public bool IsActive { get; private set; }

        // Game window information
        /// <summary>
        /// Whether the game window is focused or not
        /// </summary>
        public static bool IsWindowFocused => Kernel.IsForegroundWindow;

        /// <summary>
        /// The current game build version
        /// </summary>
        public static int Version => Kernel.GameVersion;

        // Threading
        private ThreadStart scanGameThreadingRef;
        private Thread scanGameThreading;

        private readonly bool[] aliveMonsters = new bool[3];

        // Clock event
        public delegate void ClockEvent(object source, EventArgs args);

        /* This Event is dispatched every 10 seconds to update the rich presence */
        public event ClockEvent OnClockChange;

        protected virtual void _onClockChange() => OnClockChange?.Invoke(this, EventArgs.Empty);

        public void CreateInstances()
        {
            Player = new Player();
            FirstMonster = new Monster(1);
            SecondMonster = new Monster(2);
            ThirdMonster = new Monster(3);
        }

        public void DestroyInstances()
        {
            Player = null;
            FirstMonster = null;
            SecondMonster = null;
            ThirdMonster = null;
        }

        public void StartScanning()
        {
            StartGameScanner();
            HookEvents();
            Player.StartScanning();
            FirstMonster.StartThreadingScan();
            SecondMonster.StartThreadingScan();
            ThirdMonster.StartThreadingScan();
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_GAME_SCANNER_INITIALIZED']"));
            IsActive = true;
        }

        public void StopScanning()
        {
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_GAME_SCANNER_STOP']"));
            UnhookEvents();
            FirstMonster?.StopThread();
            SecondMonster?.StopThread();
            ThirdMonster?.StopThread();
            Player?.StopScanning();
            scanGameThreading?.Abort();
            IsActive = false;
        }

        private void HookEvents()
        {
            if (Player is null)
            {
                return;
            }
            Player.OnZoneChange += OnZoneChange;
        }

        public void UnhookEvents()
        {
            if (Player is null)
            {
                return;
            }
            Player.OnZoneChange -= OnZoneChange;
        }

        public void OnZoneChange(object source, EventArgs e)
        {
            if (Player.InPeaceZone) Time = null;
            else { Time = DateTime.UtcNow; }

        }

        private void StartGameScanner()
        {
            scanGameThreadingRef = new ThreadStart(GameScanner);
            scanGameThreading = new Thread(scanGameThreadingRef)
            {
                Name = "Scanner_Game"
            };
            scanGameThreading.Start();
        }

        private void GameScanner()
        {

            while (Kernel.GameIsRunning)
            {
                if ((DateTime.UtcNow - Clock).TotalSeconds >= 10) Clock = DateTime.UtcNow;

                // Since monsters are independent, we still need to sync them with eachother
                // to use the lockon

                aliveMonsters[0] = FirstMonster.IsActuallyAlive && !FirstMonster.IsCaptured;
                aliveMonsters[1] = SecondMonster.IsActuallyAlive && !SecondMonster.IsCaptured;
                aliveMonsters[2] = ThirdMonster.IsActuallyAlive && !ThirdMonster.IsCaptured;

                for (int i = 0; i < 3; i++)
                {
                    FirstMonster.AliveMonsters[i] = aliveMonsters[i];
                    SecondMonster.AliveMonsters[i] = aliveMonsters[i];
                    ThirdMonster.AliveMonsters[i] = aliveMonsters[i];
                }

                Thread.Sleep(UserSettings.PlayerConfig.Overlay.GameScanDelay);
            }
            Thread.Sleep(1000);
            GameScanner();
        }

    }
}
