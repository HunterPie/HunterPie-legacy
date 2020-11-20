using HunterPie.Core.Events;

namespace HunterPie.Core.Local
{
    /// <summary>
    /// Player stamina logic
    /// </summary>
    public class StaminaComponent
    {

        /// <summary>
        /// Player stamina
        /// </summary>
        public float Stamina
        {
            get => stamina;
            private set
            {
                if (value != stamina)
                {
                    stamina = value;
                    Dispatch(OnStaminaUpdate);
                }
            }
        }

        /// <summary>
        /// Player maximum stamina
        /// </summary>
        public float MaxStamina
        {
            get => maxStamina;
            private set
            {
                if (value != maxStamina)
                {
                    maxStamina = value;
                    Dispatch(OnMaxStaminaUpdate);
                }
            }
        }

        public delegate void PlayerStaminaEvents(object source, PlayerStaminaEventArgs args);

        public event PlayerStaminaEvents OnStaminaUpdate;
        public event PlayerStaminaEvents OnMaxStaminaUpdate;

        #region Private
        private float stamina;
        private float maxStamina;

        private void Dispatch(PlayerStaminaEvents e) => e?.Invoke(this, new PlayerStaminaEventArgs(this));

        /// <summary>
        /// Updates the stamina values
        /// </summary>
        /// <param name="maxStamina">current maximum stamina</param>
        /// <param name="stamina">current stamina</param>
        internal void Update(float maxStamina, float stamina)
        {
            MaxStamina = maxStamina;
            Stamina = stamina;
        }
        #endregion
    }
}
