namespace HunterPie.Core {
    class Fertilizer {
        public string Name;
        public int ID;
        public int Amount;
    }
    class HarvestBox {

        public Fertilizer[] Box = new Fertilizer[4];
        public int Counter = 0;
        public int Max = 30;

        public HarvestBox() {
            PopulateBox();
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
