namespace HunterPie.Core {

    public class FertilizerEventArgs {
        public int ID;
        public string Name;
        public int Amount;

        public FertilizerEventArgs(Fertilizer m) {
            this.ID = m.ID;
            this.Name = m.Name;
            this.Amount = m.Amount;
        }
    }

    public class HarvestBoxEventArgs {
        public int Counter;
        public int Max;

        public HarvestBoxEventArgs(HarvestBox m) {
            this.Counter = m.Counter;
            this.Max = m.Max;
        }

    }

    public class Fertilizer {
        private int _id;
        private int _amount;

        public string Name;
        public int ID {
            get { return _id; }
            set {
                if (value != _id) {
                    _id = value;
                }
            }
        }
        public int Amount;

        public delegate void FertilizerEvents(object source, FertilizerEventArgs args);
        public event FertilizerEvents OnFertilizerChange;
        public event FertilizerEvents OnAmountUpdate;
        
        protected virtual void _onFertilizerChange() {
            FertilizerEventArgs args = new FertilizerEventArgs(this);
            OnFertilizerChange?.Invoke(this, args);
        }

        protected virtual void _onAmountUpdate() {
            FertilizerEventArgs args = new FertilizerEventArgs(this);
            OnAmountUpdate?.Invoke(this, args);
        }

    }
    public class HarvestBox {

        private int _counter;

        public Fertilizer[] Box = new Fertilizer[4];
        public int Counter {
            get { return _counter; }
            set {
                if (_counter != value) {
                    _counter = value;
                }
            }
        }
        public int Max = 30;

        public HarvestBox() {
            PopulateBox();
        }

        public delegate void HarvestBoxEvents(object source, HarvestBoxEventArgs args);
        public event HarvestBoxEvents OnCounterChange;
        
        protected virtual void _onCounterChange() {
            HarvestBoxEventArgs args = new HarvestBoxEventArgs(this);
            OnCounterChange?.Invoke(this, args);
        }

        private void PopulateBox() {
            for (int i = 0; i < 4; i++) {
                Fertilizer fert = new Fertilizer();
                fert.Name = "Unknown";
                fert.ID = 0;
                fert.Amount = 0;
                Box[i] = fert;
            }
        }

    }
}
