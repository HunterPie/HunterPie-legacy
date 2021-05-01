using System;
using System.Runtime.CompilerServices;
using System.Threading;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Utils;
using System.Linq;
using HunterPie.Native.Connection;
using HunterPie.Native.Connection.Packets;

namespace HunterPie.Core
{
    public class Game
    {

        #region Public
        /// <summary>
        /// Player object
        /// </summary>
        public Player Player { get; private set; }

        /// <summary>
        /// First monster in the map
        /// </summary>
        public Monster FirstMonster { get; private set; }

        /// <summary>
        /// Second monster in the map
        /// </summary>
        public Monster SecondMonster { get; private set; }

        /// <summary>
        /// Third monster in the map
        /// </summary>
        public Monster ThirdMonster { get; private set; }

        /// <summary>
        /// Current monster target by the local player
        /// </summary>
        public Monster HuntedMonster
        {
            get => Monsters.FirstOrDefault(m => m.IsTarget);
        }

        /// <summary>
        /// Array with the <see cref="FirstMonster"/>, <see cref="SecondMonster"/> and <see cref="ThirdMonster"/>
        /// </summary>
        public readonly Monster[] Monsters = new Monster[3];


        public DateTime? Time { get; private set; }

        /// <summary>
        /// Whether the game scanner is currently active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Whether the game window is focused or not
        /// </summary>
        public static bool IsWindowFocused => Kernel.IsForegroundWindow;

        /// <summary>
        /// The current game build version
        /// </summary>
        public static int Version => Kernel.GameVersion;

        /// <summary>
        /// How many frames have elapsed since the game start
        /// </summary>
        public static ulong ElapsedFrames
        {
            get
            {
                long address = Address.GetAddress("BASE") + Address.GetAddress("GAME_FRAME_COUNT_OFFSET");
                return Kernel.Read<ulong>(address);
            }
        }

        private static long renderTimeAddress = Kernel.NULLPTR;

        /// <summary>
        /// Current render time
        /// </summary>
        public static float RenderTime
        {
            get
            {
                if (renderTimeAddress == Kernel.NULLPTR)
                {
                    long address = Address.GetAddress("BASE") + Address.GetAddress("GAME_RENDER_INFO_OFFSET");
                    renderTimeAddress = Kernel.Read<long>(address);
                }

                if (renderTimeAddress == Kernel.NULLPTR)
                    return 16.4f;

                return Kernel.Read<float>(renderTimeAddress + 0x58) / Kernel.Read<float>(renderTimeAddress + 0x94);

            }
        }

        /// <summary>
        /// Whether the mouse is visible in game
        /// </summary>
        public static bool IsGUIOpen
        {
            get
            {
                long sMhGUIPtr = Address.GetAddress("BASE") + Address.GetAddress("GAME_MOUSE_INFO_OFFSET");
                long canControlPlayerPtr = Kernel.ReadMultilevelPtr(sMhGUIPtr, Address.GetOffsets("cMhMouseOffsets"));

                return Kernel.Read<byte>(canControlPlayerPtr) == 0;
            }
        }

        /// <summary>
        /// Attempts to focus the game window
        /// </summary>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public static bool Focus()
        {
            return WindowsHelper.SetForegroundWindow(Kernel.WindowHandle);
        }



        /// <summary>
        /// Current world time (e.g: 10.3 represents 10:18 AM)
        /// </summary>
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

        /// <summary>
        /// Current world <see cref="Enums.DayTime"/>
        /// </summary>
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

        public delegate void ClockEvent(object source, EventArgs args);
        public delegate void WorldEvent(object source, WorldEventArgs args);

        /// <summary>
        /// Dispatched everytime the World time changes
        /// </summary>
        public event WorldEvent OnWorldTimeUpdate;

        /// <summary>
        /// Dispatched everytime the World DayTime changes
        /// </summary>
        public event WorldEvent OnWorldDayTimeUpdate;

        /// <summary>
        /// Dispatched every 10 seconds
        /// </summary>
        public event ClockEvent OnClockChange;

        /// <summary>
        /// Parses the float world time to a DateTime
        /// </summary>
        /// <param name="worldTime">World Time</param>
        /// <returns>DateTime</returns>
        public static DateTime ParseWorldTimeToDateTime(float worldTime)
        {
            return new DateTime(0, 0, 0, (int)Math.Abs(worldTime), (int)(60 * (worldTime % 1)), 0);
        }

        #endregion

        #region Private

        // Threading
        private ThreadStart scanGameThreadingRef;
        private Thread scanGameThreading;

        private readonly bool[] aliveMonsters = new bool[3];
        private void _onClockChange() => OnClockChange?.Invoke(this, EventArgs.Empty);

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

        #region Game World data

        private float worldTime;
        private DayTime dayTime;

        private void Dispatch(WorldEvent e) => e?.Invoke(this, new WorldEventArgs(this));
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
            Client.Instance.OnDamageDealResponse += OnDamageDeal;
        }

        private void UnhookEvents()
        {
            if (Player is null)
            {
                return;
            }
            Player.OnZoneChange -= OnZoneChange;
            Client.Instance.OnDamageDealResponse -= OnDamageDeal;
        }

        private void OnDamageDeal(object sender, S_DEAL_DAMAGE info)
        {
            if (Player.ZoneID != 504 && !IsMonster((long)info.target))
                return;

            Member local = Player.PlayerParty.Player;
            if (Player.PlayerParty.IsExpedition)
            {
                int newDamage = local.Damage + info.damage;
                Player.PlayerParty.TotalDamage = newDamage;
                local.SetPlayerInfo(new MemberInfo
                {
                    Name = Player.Name,
                    HR = (short)Player.Level,
                    MR = (short)Player.MasterRank,
                    WeaponId = Player.WeaponID,
                    IsLocalPlayer = true,
                    IsLeader = true,
                    Damage = newDamage,
                    DamagePercentage = 1
                }, true);
                
            }
        }

        private bool IsMonster(long address)
        {
            return Monsters.Count(m => m.MonsterAddress == address) > 0;
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
            while (true)
            {
                while (Kernel.GameIsRunning)
                {
                    if ((DateTime.UtcNow - Clock).TotalSeconds >= 10) Clock = DateTime.UtcNow;

                    SyncMonsterAndPartyState();
                    SyncMonstersStates();
                    GetWorldCurrentTime();

                    Thread.Sleep(ConfigManager.Settings.Overlay.GameScanDelay);
                }

                Thread.Sleep(1000);
            }
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
        #endregion
    }
}
