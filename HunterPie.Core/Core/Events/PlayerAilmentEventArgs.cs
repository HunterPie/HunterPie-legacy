using System;
using HunterPie.Core.Enums;

namespace HunterPie.Core.Events
{
    public class PlayerAilmentEventArgs : EventArgs
    {

        public float AilmentTimer { get; }
        public float AilmentMaxTimer { get; }
        public PlayerAilment AilmentType { get; }

        public PlayerAilmentEventArgs(Player p)
        {
            AilmentTimer = p.AilmentTimer;
            AilmentMaxTimer = p.MaxAilmentTimer;
            AilmentType = p.AilmentType;
        }
    }
}
