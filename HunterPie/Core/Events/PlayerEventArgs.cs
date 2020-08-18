using System;
using HunterPie.Core.LPlayer.Jobs;
using Classes = HunterPie.Core.Enums.Classes;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for player events
    /// </summary>
    public class PlayerEventArgs : EventArgs
    {
        /// <summary>
        /// Player name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Player high rank
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Player master rank
        /// </summary>
        public int MasterRank { get; }

        /// <summary>
        /// Player current equipped weapon
        /// </summary>
        public Classes Weapon { get; }

        /// <summary>
        /// Player weapon name
        /// </summary>
        public string WeaponName { get; }

        /// <summary>
        /// Player class information
        /// </summary>
        public Job Class { get; }

        /// <summary>
        /// The current Session Id.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// The current Session Steam Id, this is the Id you use to use Steam to join a session
        /// </summary>
        public long SteamSessionId { get; }

        /// <summary>
        /// Local player Steam Id
        /// </summary>
        public long SteamId { get; }

        public PlayerEventArgs(Player player)
        {
            Name = player.Name;
            Level = player.Level;
            MasterRank = player.MasterRank;
            Weapon = (Classes)player.WeaponID;
            WeaponName = player.WeaponName;
            Class = GetJobBasedOnClass(player);
            SessionId = player.SessionID;
            SteamSessionId = player.SteamSession;
            SteamId = player.SteamID;
        }

        private Job GetJobBasedOnClass(Player player)
        {
            switch ((Classes)player.WeaponID)
            {
                case Classes.DualBlades:
                    return player.DualBlades;
                case Classes.LongSword:
                    return player.Longsword;
                case Classes.Hammer:
                    return player.Hammer;
                case Classes.Lance:
                    return player.Lance;
                case Classes.GunLance:
                    return player.GunLance;
                case Classes.SwitchAxe:
                    return player.SwitchAxe;
                case Classes.ChargeBlade:
                    return player.ChargeBlade;
                case Classes.InsectGlaive:
                    return player.InsectGlaive;
                case Classes.Bow:
                    return player.Bow;
                default:
                    return null;
            }
        }
    }
}
