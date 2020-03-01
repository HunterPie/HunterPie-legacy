using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HunterPie.Core {
    public class Activities {
        // Argosy, Steamworks and Tailraiders
        int _NaturalFuel { get; set; }
        int _StoredFuel { get; set; }
        int _ArgosyDaysLeft { get; set; }
        int _TailraidersDaysLeft { get; set; }
        private readonly int NaturalFuelMax = 700;

        public int NaturalFuel {
            get { return _NaturalFuel; }
            set {
                if (value != _NaturalFuel) {
                    _NaturalFuel = value;
                    _OnNaturalSteamChange();
                }
            }
        }
        public int StoredFuel {
            get { return _StoredFuel; }
            set {
                if (value != _StoredFuel) {
                    _StoredFuel = value;
                    _OnStoredSteamChange();
                }
            }
        }
        public int ArgosyDaysLeft {
            get { return _ArgosyDaysLeft; }
            set {
                if (value != _ArgosyDaysLeft) {
                    _ArgosyDaysLeft = value;
                    _OnArgosyDaysChange();
                }
            }
        }
        public int TailraidersDaysLeft {
            get { return _TailraidersDaysLeft; }
            set {
                if (value != _TailraidersDaysLeft) {
                    _TailraidersDaysLeft = value;
                    _OnTailraidersDaysChange();
                }
            }
        }

        #region EVENTS
        // Tail raiders, steam fuel and argosy events;
        public delegate void SteamFuelEvents(object source, SteamFuelEventArgs args);
        public delegate void DaysLeftEvents(object source, DaysLeftEventArgs args);
        public event SteamFuelEvents OnNaturalSteamChange;
        public event SteamFuelEvents OnStoredSteamChange;
        public event DaysLeftEvents OnArgosyDaysChange;
        public event DaysLeftEvents OnTailraidersDaysChange;

        protected virtual void _OnNaturalSteamChange() {
            OnNaturalSteamChange?.Invoke(this, new SteamFuelEventArgs(NaturalFuel, NaturalFuelMax));
        }
        
        protected virtual void _OnStoredSteamChange() {
            OnStoredSteamChange?.Invoke(this, new SteamFuelEventArgs(StoredFuel, 0));
        }

        protected virtual void _OnArgosyDaysChange() {
            OnArgosyDaysChange?.Invoke(this, new DaysLeftEventArgs(ArgosyDaysLeft));
        }
        
        protected virtual void _OnTailraidersDaysChange() {
            OnTailraidersDaysChange?.Invoke(this, new DaysLeftEventArgs(TailraidersDaysLeft));
        }
        #endregion


    }
}
