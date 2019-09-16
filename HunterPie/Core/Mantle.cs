namespace HunterPie.Core {
    class Mantle {
        public string Name { get; private set; }
        public int ID { get; private set; }
        public float Cooldown { get; private set; }
        public float Timer { get; private set; }
        public float staticCooldown { get; private set; }
        public float staticTimer { get; private set; }

        public void SetCooldown(float cd, float staticCd) {
            this.Cooldown = cd;
            this.staticCooldown = staticCd;
        }

        public void SetTimer(float Timer, float staticTimer) {
            this.Timer = Timer;
            this.staticTimer = staticTimer;
        }

        public void SetID(int newID) {
            this.ID = newID;
        }

        public void SetName(string newName) {
            this.Name = newName;
        }

    }
}
