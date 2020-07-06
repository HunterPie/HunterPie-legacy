namespace HunterPie.Core
{
    public class Fertilizer
    {
        private int _id = -1;
        private int _amount = -1;

        public string Name => GStrings.GetFertilizerNameByID(ID);
        public int ID
        {
            get => _id;
            set
            {
                if (value != _id)
                {
                    _id = value;
                    _onFertilizerChange();
                }
            }
        }
        public int Amount
        {
            get => _amount;
            set
            {
                if (value != _amount)
                {
                    _amount = value;
                    _onAmountUpdate();
                }
            }
        }

        // Fertilizer Events
        public delegate void FertilizerEvents(object source, FertilizerEventArgs args);
        public event FertilizerEvents OnFertilizerChange;
        public event FertilizerEvents OnAmountUpdate;

        protected virtual void _onFertilizerChange()
        {
            FertilizerEventArgs args = new FertilizerEventArgs(this);
            OnFertilizerChange?.Invoke(this, args);
        }

        protected virtual void _onAmountUpdate()
        {
            FertilizerEventArgs args = new FertilizerEventArgs(this);
            OnAmountUpdate?.Invoke(this, args);
        }

    }
    public class HarvestBox
    {

        private int _counter = -1;

        public Fertilizer[] Box = new Fertilizer[4];
        public int Counter
        {
            get => _counter;
            set
            {
                if (_counter != value)
                {
                    _counter = value;
                    _onCounterChange();
                }
            }
        }
        public int Max = 50;

        public HarvestBox() => PopulateBox();

        // Harvest Box Events
        public delegate void HarvestBoxEvents(object source, HarvestBoxEventArgs args);
        public event HarvestBoxEvents OnCounterChange;

        protected virtual void _onCounterChange()
        {
            HarvestBoxEventArgs args = new HarvestBoxEventArgs(this);
            OnCounterChange?.Invoke(this, args);
        }

        private void PopulateBox()
        {
            for (int i = 0; i < 4; i++)
            {
                Fertilizer fert = new Fertilizer();
                Box[i] = fert;
            }
        }

    }
}
