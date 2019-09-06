using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HunterPie.Memory;

namespace HunterPie.Core {
    class Monster {
        // Monster basic info
        private int MonsterNumber;
        public string Name { get; private set; }
        public string ID { get; private set; }
        public float TotalHP { get; private set; }
        public float CurrentHP { get; private set; }
        public bool isTarget { get; private set; }
        private Int64 MonsterAddress;

        // Threading
        ThreadStart MonsterInfoScanRef;
        Thread MonsterInfoScan;

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
                if (ThirdMonsterAddress != MonsterAddress) Debugger.Log($"Found 3rd monster address: 0x{ThirdMonsterAddress:X}");
                MonsterAddress = ThirdMonsterAddress;
            } else if (MonsterNumber == 2) {
                Int64 SecondMonsterAddress = Scanner.READ_LONGLONG(ThirdMonsterAddress + 0x28);
                if (SecondMonsterAddress != MonsterAddress) Debugger.Log($"Found 2nd monster address: 0x{SecondMonsterAddress:X}");
                MonsterAddress = SecondMonsterAddress;
            } else {
                Int64 FirstMonsterAddress = Scanner.READ_LONGLONG(Scanner.READ_LONGLONG(ThirdMonsterAddress + 0x28) + 0x28);
                if (FirstMonsterAddress != MonsterAddress) Debugger.Log($"Found 1st monster address: 0x{FirstMonsterAddress:X}");
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
            } else {
                this.TotalHP = 0.0f;
                this.CurrentHP = 0.0f;
            }
        }

        private void GetMonsterIDAndName() {
            Int64 NamePtr = Scanner.READ_LONGLONG(this.MonsterAddress + 0x290);
            string MonsterId = Scanner.READ_STRING(NamePtr + 0x0c, 64).Replace("\x00", "");
            
            if (MonsterId != "") {
                this.ID = MonsterId.Split('\\')[4];
                this.Name = GStrings.MonsterName(this.ID);
            }
        }

    }
}
