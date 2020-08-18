using System;

namespace HunterPie.Core.Events
{
    /// <summary>
    /// Event args for the party events
    /// </summary>
    public class PartyMemberEventArgs : EventArgs
    {
        /// <summary>
        /// Party member name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Party member damage.
        /// </summary>
        public int Damage { get; }
        /// <summary>
        /// Party member weapon name.
        /// </summary>
        public string Weapon { get; }
        /// <summary>
        /// Whether this member is in party or not.
        /// </summary>
        public bool IsInParty { get; }

        public PartyMemberEventArgs(Member member)
        {
            Name = member.Name;
            Damage = member.Damage;
            Weapon = member.WeaponIconName;
            IsInParty = member.IsInParty;
        }
    }
}
