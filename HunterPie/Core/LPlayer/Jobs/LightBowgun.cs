using System;

namespace HunterPie.Core.LPlayer.Jobs
{
    public class LightBowgunEventArgs : EventArgs
    {
        public float SpecialAmmoRegen;

        public LightBowgunEventArgs(LightBowgun weapon) => SpecialAmmoRegen = weapon.SpecialAmmoRegen;
    }
    public class LightBowgun : Job
    {
        private float specialAmmoRegen;

        public float SpecialAmmoRegen
        {
            get => specialAmmoRegen;
            set
            {
                if (value != specialAmmoRegen)
                {
                    specialAmmoRegen = value;
                    Dispatch(OnSpecialAmmoRegenUpdate);
                }
            }
        }
        public override int SafijiivaMaxHits => 10;

        public delegate void LightBowgunEvents(object source, LightBowgunEventArgs args);
        public event LightBowgunEvents OnSpecialAmmoRegenUpdate;

        private void Dispatch(LightBowgunEvents e) => e?.Invoke(this, new LightBowgunEventArgs(this));
    }
}
