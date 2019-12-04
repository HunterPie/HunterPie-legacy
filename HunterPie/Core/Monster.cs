using System;
using System.Threading;
using HunterPie.Memory;

namespace HunterPie.Core {
    class Monster {
        // Private vars
        private string _name;
        private float _currentHP;
        private bool _isTarget;

        // Monster basic info
        private int MonsterNumber;
        public string Name {
            get { return _name; }
            set {
                if (value != null && _name != value) {
                    _name = value;
                    _onMonsterSpawn();
                }
            }
        }
        public string ID { get; private set; }
        public float TotalHP { get; private set; }
        public float CurrentHP {
            get { return _currentHP; }
            set {
                if (value != _currentHP) {
                    _currentHP = value;
                    _onHPUpdate();
                }
            }
        }
        public float HPPercentage { get; private set; } = 1;
        public bool isTarget {
            get { return _isTarget; }
            set {
                if (value != _isTarget) {
                    _isTarget = value;
                    _onTargetted();
                }
            }
        }
        private Int64 MonsterAddress;

        // Threading
        ThreadStart MonsterInfoScanRef;
        Thread MonsterInfoScan;

        // Game events
        public delegate void MonsterEvents(object source, EventArgs args);
        public event MonsterEvents OnMonsterSpawn;
        public event MonsterEvents OnHPUpdate;
        public event MonsterEvents OnTargetted;

        protected virtual void _onMonsterSpawn() {
            OnMonsterSpawn?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onHPUpdate() {
            OnHPUpdate?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onTargetted() {
            OnTargetted?.Invoke(this, EventArgs.Empty);
        }

        public Monster(int initMonsterNumber) {
            MonsterNumber = initMonsterNumber;
        }

        public void StartThreadingScan() {
            MonsterInfoScanRef = new ThreadStart(ScanMonsterInfo);
            MonsterInfoScan = new Thread(MonsterInfoScanRef);
            MonsterInfoScan.Name = $"Scanner_Monster.{MonsterNumber}";
            Debugger.Warn($"Initializing monster({MonsterNumber}) scanner...");
            MonsterInfoScan.Start();
        }

        public void StopThread() {
            MonsterInfoScan.Abort();
        }

        private void ScanMonsterInfo() {
            while (Scanner.GameIsRunning) {
                GetMonsterAddress();
                GetMonsterIDAndName();
                GetMonsterHp();
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            ScanMonsterInfo();
        }

        private void GetMonsterAddress() {
            Int64 Address = Memory.Address.BASE + Memory.Address.MONSTER_OFFSET;
            Int64[] Offset = { 0xAF738, 0x47CDE0 };
            // This will give us the third monster's address, so we can find the second and first monster with it
            Int64 ThirdMonsterAddress = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            ThirdMonsterAddress = Scanner.READ_LONGLONG(ThirdMonsterAddress + 0x0);
            if (MonsterNumber == 3) {
                //if (ThirdMonsterAddress != MonsterAddress) Debugger.Log($"Found 3rd Monster address -> 0x{ThirdMonsterAddress:X}");
                MonsterAddress = ThirdMonsterAddress;
            } else if (MonsterNumber == 2) {
                Int64 SecondMonsterAddress = Scanner.READ_LONGLONG(ThirdMonsterAddress + 0x28);
                //if (SecondMonsterAddress != MonsterAddress) Debugger.Log($"Found 2nd Monster address -> 0x{SecondMonsterAddress:X}");
                MonsterAddress = SecondMonsterAddress;
            } else {
                Int64 FirstMonsterAddress = Scanner.READ_LONGLONG(Scanner.READ_LONGLONG(ThirdMonsterAddress + 0x28) + 0x28);
                //if (FirstMonsterAddress != MonsterAddress) Debugger.Log($"Found 1st Monster address -> 0x{FirstMonsterAddress:X}");
                MonsterAddress = FirstMonsterAddress;
            }
        }

        private void GetMonsterHp() {
            Int64 MonsterHPComponent = Scanner.READ_LONGLONG(this.MonsterAddress + 0x129D8 + 0x48);
            Int64 MonsterTotalHPAddress = MonsterHPComponent + 0x60;
            Int64 MonsterCurrentHPAddress = MonsterTotalHPAddress + 0x4;
            float f_TotalHP = Scanner.READ_FLOAT(MonsterTotalHPAddress);
            float f_CurrentHP = Scanner.READ_FLOAT(MonsterCurrentHPAddress);

            if ((this.ID != null) && f_CurrentHP <= f_TotalHP && f_CurrentHP > 0 && !this.ID.StartsWith("ems")) {
                this.TotalHP = f_TotalHP;
                this.CurrentHP = f_CurrentHP;
                this.HPPercentage = f_CurrentHP / f_TotalHP == 0 ? 1 : f_CurrentHP / f_TotalHP;
            } else {
                this.TotalHP = 0.0f;
                this.CurrentHP = 0.0f;
                this.HPPercentage = 1f;
            }
        }

        private void GetMonsterIDAndName() {
            Int64 NamePtr = Scanner.READ_LONGLONG(this.MonsterAddress + 0x290);
            string MonsterId = Scanner.READ_STRING(NamePtr + 0x0c, 64).Replace("\x00", "");

            if (MonsterId != "") {
                try {
                    string ActualID = MonsterId.Split('\\')[4];
                    if (ActualID.StartsWith("em")) {
                        if (ActualID != this.ID && GStrings.MonsterName(this.ID) != null) Debugger.Log($"Found new monster #{MonsterNumber} address -> 0x{MonsterAddress:X}");
                        this.ID = ActualID;
                        this.Name = GStrings.MonsterName(this.ID);
                    } else {
                        this.ID = null;
                        this.Name = null;
                    }
                } catch {
                    this.ID = null;
                    this.Name = null;
                }
                
            }
        }

    }
}
