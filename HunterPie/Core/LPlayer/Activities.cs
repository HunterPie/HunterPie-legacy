namespace HunterPie.Core
{
    public class Activities
    {
        // Argosy, Steamworks and Tailraiders
        int _NaturalFuel { get; set; }
        int _StoredFuel { get; set; }
        byte _ArgosyDaysLeft { get; set; }
        bool ArgosyInTown { get; set; }
        byte _TailraidersDaysLeft { get; set; } = 255;
        bool _TailraidersDeployed { get; set; } = true;
        private readonly int NaturalFuelMax = 700;
        public readonly byte TailraidersMaxQuest = 5;

        public int NaturalFuel
        {
            get => _NaturalFuel;
            set
            {
                if (value != _NaturalFuel)
                {
                    _NaturalFuel = value;
                    _OnNaturalSteamChange();
                }
            }
        }
        public int StoredFuel
        {
            get => _StoredFuel;
            set
            {
                if (value != _StoredFuel)
                {
                    _StoredFuel = value;
                    _OnStoredSteamChange();
                }
            }
        }
        public byte ArgosyDaysLeft
        {
            get => _ArgosyDaysLeft;
            set
            {
                if (value != _ArgosyDaysLeft)
                {
                    _ArgosyDaysLeft = value;
                    _OnArgosyDaysChange();
                }
            }
        }
        public byte TailraidersDaysLeft
        {
            get => _TailraidersDaysLeft;
            set
            {
                if (value != _TailraidersDaysLeft)
                {
                    _TailraidersDaysLeft = value;
                    _OnTailraidersDaysChange();
                }
            }
        }
        bool TailraidersDeployed
        {
            get => _TailraidersDeployed;
            set
            {
                if (value != _TailraidersDeployed)
                {
                    _TailraidersDeployed = value;
                    _OnTailraidersDaysChange();

                }
            }
        }
        #region ACTIVITY EVENTS
        // Tail raiders, steam fuel and argosy events;
        public delegate void SteamFuelEvents(object source, SteamFuelEventArgs args);
        public delegate void DaysLeftEvents(object source, DaysLeftEventArgs args);
        public event SteamFuelEvents OnNaturalSteamChange;
        public event SteamFuelEvents OnStoredSteamChange;
        public event DaysLeftEvents OnArgosyDaysChange;
        public event DaysLeftEvents OnTailraidersDaysChange;

        protected virtual void _OnNaturalSteamChange() => OnNaturalSteamChange?.Invoke(this, new SteamFuelEventArgs(NaturalFuel, NaturalFuelMax));

        protected virtual void _OnStoredSteamChange() => OnStoredSteamChange?.Invoke(this, new SteamFuelEventArgs(StoredFuel, 0));

        protected virtual void _OnArgosyDaysChange() => OnArgosyDaysChange?.Invoke(this, new DaysLeftEventArgs(ArgosyDaysLeft, ArgosyInTown));

        protected virtual void _OnTailraidersDaysChange() => OnTailraidersDaysChange?.Invoke(this, new DaysLeftEventArgs(TailraidersDaysLeft, TailraidersDeployed));
        #endregion

        #region INFO SETTERS

        public void SetSteamFuelInfo(int NaturalFuel, int StoredFuel)
        {
            this.NaturalFuel = NaturalFuel;
            this.StoredFuel = StoredFuel;
        }

        public void SetArgosyInfo(byte Days, bool IsInTown)
        {
            ArgosyInTown = IsInTown;
            ArgosyDaysLeft = Days;
        }

        public void SetTailraidersInfo(byte Days, bool IsDeployed)
        {
            TailraidersDaysLeft = Days;
            TailraidersDeployed = IsDeployed;
        }
        #endregion

    }
}
