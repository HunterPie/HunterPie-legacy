using System;
using System.Threading;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Utils;

namespace HunterPie.Core
{

    public abstract class Game
    {
        // Game classes
        public Player Player { get; protected set; }

        public Monster FirstMonster { get; protected set; }
        public Monster SecondMonster { get; protected set; }
        public Monster ThirdMonster { get; protected set; }

        public abstract Monster HuntedMonster { get; }

        public readonly Monster[] Monsters = new Monster[3];

        public DateTime? Time { get; protected set; }
        public bool IsActive { get; protected set; }

        
        /// <summary>
        /// Whether the game window is focused or not
        /// </summary>
        public static bool IsWindowFocused => Kernel.IsForegroundWindow;

        /// <summary>
        /// The current game build version
        /// </summary>
        public static int Version => Kernel.GameVersion;

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

        public abstract void CreateInstances();
        public abstract void DestroyInstances();

        public abstract void UnhookEvents();

        public abstract void OnZoneChange(object source, EventArgs e);
    }
}
