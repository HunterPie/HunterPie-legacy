using System;
using System.Threading;
using System.Windows.Forms;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Utils;

namespace HunterPie.Core
{

    public class GameImpl : Game
    {
        public override Monster HuntedMonster
        {
            get
            {
                if (FirstMonster.IsTarget) {
                    return FirstMonster;
                } else if (SecondMonster.IsTarget) {
                    return SecondMonster;
                } else if (ThirdMonster.IsTarget) {
                    return ThirdMonster;
                } else {
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

        // Threading
        private ThreadStart scanGameThreadingRef;
        private Thread scanGameThreading;

        private readonly bool[] aliveMonsters = new bool[3];

        public override void CreateInstances()
        {
            Player = new Player();
            FirstMonster = new Monster(1);
            SecondMonster = new Monster(2);
            ThirdMonster = new Monster(3);

            Monsters[0] = FirstMonster;
            Monsters[1] = SecondMonster;
            Monsters[2] = ThirdMonster;
#if !DEBUG
        }
#endif
#if DEBUG
            foreach (Monster m in Monsters)
            {
                m.OnMonsterDeath += M_OnMonsterDeath;
                m.OnMonsterDespawn += M_OnMonsterDespawn;
            }
        }

        private void M_OnMonsterDespawn(object source, EventArgs args)
        {
            Monster m = (Monster)source;
            Debugger.Warn($"{m.Name} Despawn -> {m.ActionName}");
        }

        private void M_OnMonsterDeath(object source, EventArgs args)
        {
            Monster m = (Monster)source;
            Debugger.Warn($"{m.Name} Death -> {m.ActionName}");
        }
#endif
        public override void DestroyInstances()
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

        public override void UnhookEvents()
        {
            if (Player is null)
            {
                return;
            }
            Player.OnZoneChange -= OnZoneChange;
        }

        public override void OnZoneChange(object source, EventArgs e)
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
            long address = Kernel.Read<long>(Address.BASE + Address.WORLD_DATA_OFFSET);
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
