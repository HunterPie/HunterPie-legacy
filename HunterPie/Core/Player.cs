using System;
using System.Linq;
using System.Threading;
using HunterPie.Memory;

namespace HunterPie.Core {
    class Player {

        // Private variables
        private int[] _charPlaytimes = new int[3] { -1, -1, -1 };
        private int _slot = -1;
        private int _level;
        private string _name;
        private int _zoneId = -1;
        private string _zoneName;
        private int _weaponId;
        private string _weaponName;
        private string _sessionId;
        private bool _inPeaceZone;
        private bool _inHarvestZone;
        private int _partySize;

        // Game info
        private int[] PeaceZones = new int[11] { 0, 5, 7, 11, 15, 16, 21, 23, 24, 31, 33 };
        private int[] _HBZones = new int[4] { 31, 33, 11, 21 };

        // Player info
        private Int64 LEVEL_ADDRESS;
        private Int64 EQUIPMENT_ADDRESS;
        public int Slot {
            get {
                return _slot;
            } set {
                if (_slot != value) {
                    _slot = value;
                    DispatchLogin();
                }
            }
        }
        public int Level {
            get {
                return _level;
            } set {
                if (_level != value) {
                    _level = value;
                    DispatchLevelUp();
                }
            }
        }
        public string Name {
            get {
                return _name;
            } set {
                if (_name != value) {
                    _name = value;
                    DispatchNameChange();
                }
            }
        }
        public int ZoneID {
            get {
                return _zoneId;
            } set {
                if (_zoneId != value) {
                    _zoneId = value;
                    DispatchZoneChange();
                }
            }
        }
        public string ZoneName {
            get {
                return _zoneName;
            } set {
                if (_zoneName != value) {
                    _zoneName = value;
                }
            }
        }
        public int LastZoneID { get; private set; }
        public int WeaponID {
            get {
                return _weaponId;
            } set {
                if (_weaponId != value) {
                    _weaponId = value;
                    DispatchWeaponChange();
                }
            }
        }
        public string WeaponName {
            get {
                return _weaponName;
            } set {
                if (_weaponName != value) {
                    _weaponName = value;
                }
            }
        }
        public string SessionID {
            get {
                return _sessionId;
            } set {
                if (_sessionId != value) {
                    _sessionId = value;
                    DispatchSessionChange();
                }
            }
        }
        public bool inPeaceZone = true;
        public bool inHarvestZone {
            get {
                return _HBZones.Contains(ZoneID);
            }
        }
        // Party
        public string[] Party = new string[4];
        public int PartySize {
            get {
                return _partySize;
            } set {
                if (_partySize != value) {
                    _partySize = value;
                    DispatchPartyChange();
                }
            }
        }
        public int PartyMax = 4;

        // Harvesting
        public HarvestBox Harvest = new HarvestBox();

        // Mantles
        public Mantle PrimaryMantle = new Mantle();
        public Mantle SecondaryMantle = new Mantle();

        // Threading
        private ThreadStart ScanPlayerInfoRef;
        private Thread ScanPlayerInfo;

        // Event handlers
        // Level event handler
        public delegate void LevelEventHandler(object source, EventArgs args);
        public event LevelEventHandler OnLevelChange;

        // Name event handler
        public delegate void NameEventHandler(object source, EventArgs args);
        public event NameEventHandler OnNameChange;
        
        // Zone change event
        public delegate void ZoneEventHandler(object source, EventArgs args);
        public event ZoneEventHandler OnZoneChange;

        // Weapon change event
        public delegate void WeaponEventHandler(object source, EventArgs args);
        public event WeaponEventHandler OnWeaponChange;

        // Session change
        public delegate void SessionEventHandler(object source, EventArgs args);
        public event SessionEventHandler OnSessionChange;

        // Got in a party
        public delegate void PartyEventHandler(object source, EventArgs args);
        public event PartyEventHandler OnPartyChange;

        // On login
        public delegate void LoginEventHandler(object source, EventArgs args);
        public event LoginEventHandler OnCharacterLogin;

        // Dispatchers

        protected virtual void DispatchLogin() {
            OnCharacterLogin?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DispatchLevelUp() {
            OnLevelChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DispatchNameChange() {
            OnNameChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DispatchZoneChange() {
            OnZoneChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DispatchWeaponChange() {
            OnWeaponChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void DispatchSessionChange() {
            OnSessionChange?.Invoke(this, EventArgs.Empty);
        }
        
        protected virtual void DispatchPartyChange() {
            OnPartyChange?.Invoke(this, EventArgs.Empty);
        }

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
                GetPlayerSlot();
                GetPlayerLevel();
                GetPlayerName();
                GetZoneId();
                GetWeaponId();
                GetFertilizers();
                GetSessionId();
                GetEquipmentAddress();
                GetPrimaryMantle();
                GetSecondaryMantle();
                GetPrimaryMantleTimers();
                GetSecondaryMantleTimers();
                GetParty();
                Thread.Sleep(1200);
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private void GetPlayerSlot() {
            // This is a workaround until I find a better way to get which character is the user on.
            // This method is based on character playtime, checking which one is being updated
            Int64 Address = Memory.Address.BASE + Memory.Address.LEVEL_OFFSET;
            Int64[] Offset = new Int64[4] { 0x70, 0x68, 0x8, 0x20 };
            Int64 AddressValue = Scanner.READ_MULTILEVEL_PTR(Address, Offset);
            Int64 currentChar;
            Int64 nextChar = 0x139F20;
            int playtime;
            int charId = 999;
            // TODO: FIX THIS
            for (int charIndex = 2; charIndex >= 0; charIndex--) {
                currentChar = AddressValue + 0x118 + (nextChar * charIndex);
                playtime = Scanner.READ_INT(currentChar);
                if (_charPlaytimes[charIndex] != playtime) {
                    if (_charPlaytimes.Length == 3 && _charPlaytimes[0] != -1) charId = charIndex;
                    _charPlaytimes[charIndex] = playtime;
                }
            }
            Slot = charId;
        }

        private void GetPlayerLevel() {
            Int64 nextChar = 0x139F20; // Next char offset
            Int64 Address = Memory.Address.BASE + Memory.Address.LEVEL_OFFSET;
            Int64[] Offset = new Int64[4] { 0x70, 0x68, 0x8, 0x20 };
            Int64 AddressValue = Scanner.READ_MULTILEVEL_PTR(Address, Offset) + (nextChar * (Slot == 999 || Slot == -1 ? 0 : Slot));
            if (LEVEL_ADDRESS != AddressValue + 0x108 && AddressValue != 0x0) Debugger.Log($"Found player address at 0x{AddressValue+0x108:X}");
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
            int partySize = 0;
            for (int member = 0; member < PartyMax; member++) {
                string partyMemberName = GetPartyMemberName(PartyContainer + (member * 0x21));
                if (partyMemberName == null) {
                    this.Party[member] = null;
                    continue;
                } else {
                    this.Party[member] = partyMemberName;
                    partySize++;
                }
            }
            this.PartySize = partySize;
        }

        private string GetPartyMemberName(Int64 NameAddress) {
            try {
                string PartyMemberName = Scanner.READ_STRING(NameAddress, 32);
                return PartyMemberName[0] == '\x00' ? null : PartyMemberName;
            } catch {
                return null;
            }
        }

        private void GetFertilizers() {
            Int64 Address = this.LEVEL_ADDRESS;
            for (int fertCount = 0; fertCount < 4; fertCount++) {
                // Calculates memory address
                Int64 FertilizerAddress = Address + 0x6740C + (0x10 * fertCount);
                // Read memory
                int FertilizerId = Scanner.READ_INT(FertilizerAddress - 0x4);
                string FertilizerName = GStrings.FertilizerName(FertilizerId);
                int FertilizerCount = Scanner.READ_INT(FertilizerAddress);
                // update fertilizer data
                Harvest.Box[fertCount].Name = FertilizerName;
                Harvest.Box[fertCount].ID = FertilizerId;
                Harvest.Box[fertCount].Amount = FertilizerCount;
            }
            UpdateHarvestBoxCounter(Address + 0x6740C + (0x10 * 3));
        }

        private void UpdateHarvestBoxCounter(Int64 LastFertAddress) {
            Int64 Address = LastFertAddress + 0x10;
            Harvest.Counter = 0;
            for (long iAddress = Address; iAddress < Address + 0x1F0; iAddress += 0x10) {
                int memValue = Scanner.READ_INT(iAddress);
                if (memValue > 0) {
                    Harvest.Counter++;
                }
            }
        }
    }
}
