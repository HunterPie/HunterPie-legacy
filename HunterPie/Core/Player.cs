using System;
using System.Linq;
using System.Threading;
using HunterPie.Memory;

namespace HunterPie.Core {
    class Player {
        // Game info
        private int[] PeaceZones = new int[11] { 0, 5, 7, 11, 15, 16, 21, 23, 24, 31, 33 };

        // Player info
        private Int64 LEVEL_ADDRESS;
        private Int64 EQUIPMENT_ADDRESS;
        public int Slot = 0;
        public int Level { get; private set; }
        public string Name { get; private set; }
        public int ZoneID { get; private set; }
        public string ZoneName { get; private set; }
        public int LastZoneID { get; private set; }
        public int WeaponID { get; private set; }
        public string WeaponName { get; private set; }
        public string SessionID { get; private set; }
        public bool inPeaceZone = true;

        // Party
        public string[] Party = new string[4];
        public int PartySize = 0;
        public int PartyMax = 4;

        // Harvesting
        public int HarvestedItemsCounter { get; private set; }
        public object[] HarvestBoxFertilizers = new object[4];

        // Mantles
        public Mantle PrimaryMantle = new Mantle();
        public Mantle SecondaryMantle = new Mantle();

        // Threading
        private ThreadStart ScanPlayerInfoRef;
        private Thread ScanPlayerInfo;

        public void StartScanning() {
            ScanPlayerInfoRef = new ThreadStart(GetPlayerInfo);
            ScanPlayerInfo = new Thread(ScanPlayerInfoRef);
            ScanPlayerInfo.Name = "Scanner_Player";
            Debugger.Warn("Initializing Player memory scanner...");
            ScanPlayerInfo.Start();
        }

        public void StopScanning() {
            ScanPlayerInfo.Abort();
        }

        private void GetPlayerInfo() {
            while (Scanner.GameIsRunning) {
                GetPlayerLevel();
                GetPlayerName();
                GetZoneId();
                GetWeaponId();
                GetSessionId();
                GetEquipmentAddress();
                GetPrimaryMantle();
                GetSecondaryMantle();
                GetPrimaryMantleTimers();
                GetSecondaryMantleTimers();
                GetParty();
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private void GetPlayerLevel() {
            Int64 Address = Memory.Address.BASE + Memory.Address.LEVEL_OFFSET;
            Int64[] Offset = new Int64[4] { 0x70, 0x68, 0x8, 0x20 };
            Int64 AddressValue = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            if (LEVEL_ADDRESS != AddressValue + 0x108 && AddressValue != 0x0) Debugger.Log($"Found player address at 0x{AddressValue:X}");
            LEVEL_ADDRESS = AddressValue + 0x108;
            Level = Scanner.READ_INT(LEVEL_ADDRESS);
        }

        private void GetPlayerName() {
            Int64 Address = LEVEL_ADDRESS - 64;
            Name = Scanner.READ_STRING(Address, 32).Trim('\x00');
        }

        private void GetZoneId() {
            Int64 Address = Memory.Address.BASE + Memory.Address.ZONE_OFFSET;
            Int64[] Offset = new Int64[4] { 0x660, 0x28, 0x18, 0x440 };
            Int64 ZoneAddress = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            int zoneId = Scanner.READ_INT(ZoneAddress + 0x2B0);
            if (zoneId != ZoneID) {
                this.LastZoneID = ZoneID;
                this.ZoneID = zoneId;
                this.inPeaceZone = PeaceZones.Contains(this.ZoneID);
            }
            ZoneName = GStrings.ZoneName(ZoneID);
        }

        public void ChangeLastZone() {
            this.LastZoneID = ZoneID;
        }

        private void GetWeaponId() {
            Int64 Address = Memory.Address.BASE + Memory.Address.WEAPON_OFFSET;
            Int64[] Offset = new Int64[4] { 0x70, 0x5A8, 0x310, 0x148 };
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            WeaponID = Scanner.READ_INT(Address+0x2B8);
            WeaponName = GStrings.WeaponName(WeaponID);
        }

        private void GetSessionId() {
            Int64 Address = Memory.Address.BASE + Memory.Address.SESSION_OFFSET;
            Int64[] Offset = new Int64[4] { 0xA0, 0x20, 0x80, 0x9C };
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            SessionID = Scanner.READ_STRING(Address+0x3C8, 12);
        }

        private void GetEquipmentAddress() {
            Int64 Address = Memory.Address.BASE + Memory.Address.EQUIPMENT_OFFSET;
            Int64[] Offset = new Int64[4] { 0x78, 0x50, 0x40, 0x450 };
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            if (EQUIPMENT_ADDRESS != Address) Debugger.Log($"New equipment address found -> 0x{Address:X}");
            EQUIPMENT_ADDRESS = Address;
        }

        private void GetPrimaryMantle() {
            Int64 Address = LEVEL_ADDRESS + 0x34;
            int mantleId = Scanner.READ_INT(Address);
            PrimaryMantle.SetID(mantleId);
            PrimaryMantle.SetName(GStrings.MantleName(mantleId));
        }

        private void GetSecondaryMantle() {
            Int64 Address = LEVEL_ADDRESS + 0x34 + 0x4;
            int mantleId = Scanner.READ_INT(Address);
            SecondaryMantle.SetID(mantleId);
            SecondaryMantle.SetName(GStrings.MantleName(mantleId));
        }

        private void GetPrimaryMantleTimers() {
            Int64 PrimaryMantleTimerFixed = (PrimaryMantle.ID * 4) + Address.timerFixed;
            Int64 PrimaryMantleTimer = (PrimaryMantle.ID * 4) + Address.timerDynamic;
            Int64 PrimaryMantleCdFixed = (PrimaryMantle.ID * 4) + Address.cooldownFixed;
            Int64 PrimaryMantleCdDynamic = (PrimaryMantle.ID * 4) + Address.cooldownDynamic;
            PrimaryMantle.SetCooldown(Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + PrimaryMantleCdDynamic), Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + PrimaryMantleCdFixed));
            PrimaryMantle.SetTimer(Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + PrimaryMantleTimer), Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + PrimaryMantleTimerFixed));
        }

        private void GetSecondaryMantleTimers() {
            Int64 SecondaryMantleTimerFixed = (SecondaryMantle.ID * 4) + Address.timerFixed;
            Int64 SecondaryMantleTimer = (SecondaryMantle.ID * 4) + Address.timerDynamic;
            Int64 SecondaryMantleCdFixed = (SecondaryMantle.ID * 4) + Address.cooldownFixed;
            Int64 SecondaryMantleCdDynamic = (SecondaryMantle.ID * 4) + Address.cooldownDynamic;
            SecondaryMantle.SetCooldown(Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + SecondaryMantleCdDynamic), Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + SecondaryMantleCdFixed));
            SecondaryMantle.SetTimer(Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + SecondaryMantleTimer), Scanner.READ_FLOAT(EQUIPMENT_ADDRESS + SecondaryMantleTimerFixed));
        }

        private void GetParty() {
            Int64 address = Address.BASE + Address.PARTY_OFFSET;
            Int64[] offsets = new Int64[1] { 0x0 };
            Int64 PartyContainer = Scanner.READ_LONGLONG(address) + 0x54A45;
            this.PartySize = 0;
            for (int member = 0; member < PartyMax; member++) {
                string partyMemberName = GetPartyMemberName(PartyContainer + (member * 0x21));
                if (partyMemberName == null) {
                    this.Party[member] = null;
                    continue;
                } else {
                    this.Party[member] = partyMemberName;
                    this.PartySize++;
                }
            }
        }

        private string GetPartyMemberName(Int64 NameAddress) {
            try {
                string PartyMemberName = Scanner.READ_STRING(NameAddress, 32);
                return PartyMemberName[0] == '\x00' ? null : PartyMemberName;
            } catch {
                return null;
            }
        }

    }
}
