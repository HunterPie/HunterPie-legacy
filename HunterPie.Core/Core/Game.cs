using System;
using System.Runtime.CompilerServices;
using System.Threading;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Utils;
using HunterPie.Core.Native;
using System.Runtime.InteropServices;

// TODO: Probably overkill, but add a public key to this
[assembly: InternalsVisibleTo("HunterPie")]
[assembly: InternalsVisibleTo("HunterPie.CoreTests")]
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

        public readonly Monster[] Monsters = new Monster[3];

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

        #region Game World data

        private float worldTime;
        private DayTime dayTime;

        public float WorldTime
        {
            get => worldTime;
            set
            {
                if (value != worldTime)
                {
                    worldTime = value;
                    Dispatch(OnWorldTimeUpdate);
                }
            }
        }

        public DayTime DayTime
        {
            get => dayTime;
            set
            {
                if (value != dayTime)
                {
                    dayTime = value;
                    Dispatch(OnWorldDayTimeUpdate);
                }
            }
        }

        public delegate void WorldEvent(object source, WorldEventArgs args);
        public event WorldEvent OnWorldTimeUpdate;
        public event WorldEvent OnWorldDayTimeUpdate;

        protected virtual void Dispatch(WorldEvent e) => e?.Invoke(this, new WorldEventArgs(this));
        #endregion

        internal void CreateInstances()
        {
            Player = new Player();
            FirstMonster = new Monster(1);
            SecondMonster = new Monster(2);
            ThirdMonster = new Monster(3);

            Monsters[0] = FirstMonster;
            Monsters[1] = SecondMonster;
            Monsters[2] = ThirdMonster;

        }

        internal void DestroyInstances()
        {
            Player = null;
            FirstMonster = null;
            SecondMonster = null;
            ThirdMonster = null;

            for (int i = 0; i < Monsters.Length; i++)
            {
                Monsters[i] = null;
            }
        }

        internal void StartScanning()
        {
            GMD.InitializeGMDs();
            MusicSkillData.Load();

            StartGameScanner();
            HookEvents();
            Player.StartScanning();
            FirstMonster.StartThreadingScan();
            SecondMonster.StartThreadingScan();
            ThirdMonster.StartThreadingScan();
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_GAME_SCANNER_INITIALIZED']"));
            IsActive = true;

        }

        internal void StopScanning()
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

        private void UnhookEvents()
        {
            if (Player is null)
            {
                return;
            }
            Player.OnZoneChange -= OnZoneChange;
        }

        private void OnZoneChange(object source, EventArgs e)
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
            scanGameThreading.SetApartmentState(ApartmentState.STA);
            scanGameThreading.Start();
        }

        private void GameScanner()
        {

            for (int i = 0; i < 57; i++)
            {
                string correct = GStrings.GetAbnormalityByID("HUNTINGHORN", i, 0);
                string maybe = GMD.GetMusicSkillNameById(i);
            }

            while (Kernel.GameIsRunning)
            {
                if ((DateTime.UtcNow - Clock).TotalSeconds >= 10)
                    Clock = DateTime.UtcNow;

                SyncMonsterAndPartyState();
                SyncMonstersStates();
                GetWorldCurrentTime();

                Thread.Sleep(UserSettings.PlayerConfig.Overlay.GameScanDelay);
            }
            Thread.Sleep(1000);
            GameScanner();
        }

        private void GetWorldCurrentTime()
        {
            long address = Kernel.Read<long>(Address.GetAddress("BASE") + Address.GetAddress("WORLD_DATA_OFFSET"));
            float time = Kernel.Read<float>(address + 0x38);
            
            if (time.IsWithin(17, 18.99f))
            {
                // Evening - 17:00 -> 18:59
                DayTime = DayTime.Evening;
            } else if (time.IsWithin(5, 6.99f))
            {
                // Morning - 5:00 -> 6:59
                DayTime = DayTime.Morning;
            } else if (time.IsWithin(7, 16.99f))
            {
                // Afternoon - 7:00 -> 16:59
                DayTime = DayTime.Afternoon;
            } else
            {
                // Night - 19:00 -> 4:59
                DayTime = DayTime.Night;
            }
            WorldTime = time;
        }

        private void SyncMonstersStates()
        {
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
        }

        private void SyncMonsterAndPartyState()
        {
            foreach (Monster monster in Monsters)
            {
                monster.IsLocalHost = Player.PlayerParty.IsLocalHost;
            }
        }
    }
}
