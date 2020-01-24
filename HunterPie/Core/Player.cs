using System;
using System.Linq;
using System.Threading;
using HunterPie.Memory;
using HunterPie.Logger;

namespace HunterPie.Core {
    public class Player {

        // Private variables
        private Int64 _playerAddress = 0x0;
        private int _level;
        private string _name;
        private int _zoneId = -1;
        private string _zoneName;
        private int _weaponId;
        private string _weaponName;
        private string _sessionId;
        private int _partySize;
        private int _masterRank; // TODO: Add this

        // Game info
        private int[] PeaceZones = new int[9] { 0, 301, 302, 303, 305, 306, 501, 502, 503 };
        private int[] _HBZones = new int[9] { 301, 302, 303, 305, 306, 501, 502, 503, 506 };

        // Player info
        private Int64 LEVEL_ADDRESS;
        private Int64 EQUIPMENT_ADDRESS;
        private Int64 PlayerSelectedPointer;
        private int PlayerSlot;
        public Int64 PlayerAddress {
            get { return _playerAddress; }
            set {
                if (_playerAddress != value) {
                    _playerAddress = value;
                    if (value != 0x0) _onLogin();
                }
            }
        }
        public int Level { // Hunter Rank
            get { return _level; }
            set {
                if (_level != value) {
                    _level = value;
                    _onLevelUp();
                }
            }
        }
        public int MasterRank {
            get { return _masterRank; }
            set {
                if (_masterRank != value) {
                    _masterRank = value;
                    _onMasterRankLevelUp();
                }
            }
        }
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    _onNameChange();
                }
            }
        }
        public int ZoneID {
            get { return _zoneId; }
            set {
                if (_zoneId != value) {
                    if (PeaceZones.Contains(_zoneId) && !PeaceZones.Contains(value)) _onPeaceZoneLeave();
                    if (_HBZones.Contains(_zoneId) && !_HBZones.Contains(value)) _onVillageLeave();
                    _zoneId = value;
                    _onZoneChange();
                    if (PeaceZones.Contains(value)) _onPeaceZoneEnter();
                    if (_HBZones.Contains(value)) _onVillageEnter();
                }
            }
        }
        public string ZoneName {
            get { return _zoneName; }
            set {
                if (_zoneName != value) {
                    _zoneName = value;
                }
            }
        }
        public int LastZoneID { get; private set; }
        public int WeaponID {
            get { return _weaponId; }
            set {
                if (_weaponId != value) {
                    _weaponId = value;
                    _onWeaponChange();
                }
            }
        }
        public string WeaponName {
            get { return _weaponName; }
            set {
                if (_weaponName != value) {
                    _weaponName = value;
                }
            }
        }
        public string SessionID {
            get { return _sessionId; }
            set {
                if (_sessionId != value) {
                    _sessionId = value;
                    _onSessionChange();
                }
            }
        }
        public bool inPeaceZone = true;
        public bool inHarvestZone {
            get { return _HBZones.Contains(ZoneID); }
        }
        // Party
        public string[] Party = new string[4];
        public int PartySize {
            get { return _partySize; }
            set {
                if (_partySize != value) {
                    _partySize = value;
                    _onPartyChange();
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
        public delegate void PlayerEvents(object source, EventArgs args);
        public event PlayerEvents OnLevelChange;
        public event PlayerEvents OnMasterRankChange;
        public event PlayerEvents OnNameChange;
        public event PlayerEvents OnZoneChange;
        public event PlayerEvents OnWeaponChange;
        public event PlayerEvents OnSessionChange;
        public event PlayerEvents OnPartyChange;
        public event PlayerEvents OnCharacterLogin;
        public event PlayerEvents OnPeaceZoneEnter;
        public event PlayerEvents OnVillageEnter;
        public event PlayerEvents OnPeaceZoneLeave;
        public event PlayerEvents OnVillageLeave;
        

        // Dispatchers

        protected virtual void _onLogin() {
            OnCharacterLogin?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onLevelUp() {
            OnLevelChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onMasterRankLevelUp() {
            OnMasterRankChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onNameChange() {
            OnNameChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onZoneChange() {
            OnZoneChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onWeaponChange() {
            OnWeaponChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onSessionChange() {
            OnSessionChange?.Invoke(this, EventArgs.Empty);
        }
        
        protected virtual void _onPartyChange() {
            OnPartyChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onPeaceZoneEnter() {
            OnPeaceZoneEnter?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onVillageEnter() {
            OnVillageEnter?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onPeaceZoneLeave() {
            OnPeaceZoneLeave?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onVillageLeave() {
            OnVillageLeave?.Invoke(this, EventArgs.Empty);
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
                if (GetPlayerAddress()) {
                    GetPlayerLevel();
                    GetPlayerMasterRank();
                    GetPlayerName();
                    GetWeaponId();
                }
                GetZoneId();
                GetFertilizers();
                GetSessionId();
                GetEquipmentAddress();
                GetPrimaryMantle();
                GetSecondaryMantle();
                GetPrimaryMantleTimers();
                GetSecondaryMantleTimers();
                GetParty();
                Thread.Sleep(200);
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private bool GetPlayerAddress() {
            // TODO: Find a better way to detect the player character
            Int64 Address = Memory.Address.BASE + Memory.Address.WEAPON_OFFSET;
            Int64 AddressValue = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.WeaponOffsets);
            Int64 pAddress = 0x0;
            Int64 nextPlayer = 0x27E9F0;
            if (AddressValue > 0x0) {
                PlayerSelectedPointer = AddressValue;
                string pName = Scanner.READ_STRING(AddressValue - 0x270, 32);
                int pLevel = Scanner.READ_INT(AddressValue - 0x230);
                // If char name starts with a null char then the game haven't launched yet
                if (pName[0] == '\x00') return false;
                for (int playerSlot = 0; playerSlot < 3; playerSlot++) {
                    Address = Memory.Address.BASE + Memory.Address.LEVEL_OFFSET;
                    pAddress = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.LevelOffsets) + (nextPlayer * playerSlot);
                    if (Scanner.READ_INT(pAddress) == pLevel && Scanner.READ_STRING(pAddress - 0x40, 32) == pName && PlayerAddress != pAddress) {
                        LEVEL_ADDRESS = pAddress;
                        GetPlayerLevel();
                        GetPlayerName();
                        PlayerAddress = pAddress;
                        this.PlayerSlot = playerSlot;
                        return true;
                    }
                }
            } else {
                PlayerAddress = 0x0;
                LEVEL_ADDRESS = 0x0;
                return false;
            }
            return true;
        }

        private void GetPlayerLevel() {
            Level = Scanner.READ_INT(LEVEL_ADDRESS);
        }

        private void GetPlayerMasterRank() {
            MasterRank = Scanner.READ_INT(LEVEL_ADDRESS + 0x44);
        }

        private void GetPlayerName() {
            Int64 Address = LEVEL_ADDRESS - 0x40;
            Name = Scanner.READ_STRING(Address, 32).Trim('\x00');
        }

        private void GetZoneId() {
            int ZoneOffset = PlayerSlot == 0 ? 0x95D0 : 0xAB90;
            int zoneId = Scanner.READ_INT(PlayerSelectedPointer + ZoneOffset);
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
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.WeaponOffsets);
            WeaponID = Scanner.READ_INT(Address);
            WeaponName = GStrings.WeaponName(WeaponID);
        }

        private void GetSessionId() {
            Int64 Address = Memory.Address.BASE + Memory.Address.SESSION_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.SessionOffsets);
            SessionID = Scanner.READ_STRING(Address, 12);
        }

        private void GetEquipmentAddress() {
            Int64 Address = Memory.Address.BASE + Memory.Address.EQUIPMENT_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.EquipmentOffsets);
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
                Int64 FertilizerAddress = Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * fertCount);
                // Read memory
                int FertilizerId = Scanner.READ_INT(FertilizerAddress - 0x4);
                string FertilizerName = GStrings.FertilizerName(FertilizerId);
                int FertilizerCount = Scanner.READ_INT(FertilizerAddress);
                // update fertilizer data
                Harvest.Box[fertCount].Name = FertilizerName;
                Harvest.Box[fertCount].ID = FertilizerId;
                Harvest.Box[fertCount].Amount = FertilizerCount;
            }
            UpdateHarvestBoxCounter(Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * 3));
        }

        private void UpdateHarvestBoxCounter(Int64 LastFertAddress) {
            Int64 Address = LastFertAddress + Memory.Address.Offsets.HarvestBoxOffset;
            int counter = 0;
            for (long iAddress = Address; iAddress < Address + 0x1F0; iAddress += 0x10) {
                int memValue = Scanner.READ_INT(iAddress);
                if (memValue > 0) {
                    counter++;
                }
            }
            Harvest.Counter = counter;
        }
    }
}
