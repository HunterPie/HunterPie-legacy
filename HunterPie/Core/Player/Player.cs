using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using HunterPie.Memory;
using HunterPie.Logger;

namespace HunterPie.Core {
    public class Player {
        // Consts (TODO: Move this somewhere else)
        private readonly int MaxHHBuffs = 56;
        private readonly int MaxPalicoBuffs = 15;
        private readonly int MaxMiscBuffs = 28;

        // Private variables
        private Int64 _playerAddress = 0x0;
        private int _level;
        private int _zoneId = -1;
        private byte _weaponId;
        private string _sessionId;

        // Game info
        private readonly int[] PeaceZones = new int[10] { 0, 301, 302, 303, 305, 306, 501, 502, 503, 506 };
        private readonly int[] _HBZones = new int[9] { 301, 302, 303, 305, 306, 501, 502, 503, 506 };
        
        // Player info
        private Int64 LEVEL_ADDRESS;
        private Int64 EQUIPMENT_ADDRESS;
        private Int64 PlayerStructAddress;
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
        public int MasterRank { get; private set; }
        public string Name { get; private set; }
        public int ZoneID {
            get { return _zoneId; }
            set {
                if (_zoneId != value) {
                    if ((_zoneId == -1 || PeaceZones.Contains(_zoneId)) && !PeaceZones.Contains(value)) _onPeaceZoneLeave();
                    if (_HBZones.Contains(_zoneId) && !_HBZones.Contains(value)) _onVillageLeave();
                    _zoneId = value;
                    _onZoneChange();
                    if (PeaceZones.Contains(value)) _onPeaceZoneEnter();
                    if (_HBZones.Contains(value)) _onVillageEnter();
                }
            }
        }
        public string ZoneName {
            get { return GStrings.GetStageNameByID(ZoneID); }
        }
        public int LastZoneID { get; private set; }
        public byte WeaponID {
            get { return _weaponId; }
            set {
                if (_weaponId != value) {
                    _weaponId = value;
                    _onWeaponChange();
                }
            }
        }
        public string WeaponName {
            get { return GStrings.GetWeaponNameByID(WeaponID); }
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
        public bool inPeaceZone {
            get { return PeaceZones.Contains(this.ZoneID); }
        }
        public bool inHarvestZone {
            get { return _HBZones.Contains(ZoneID); }
        }
        
        // Party
        public Party PlayerParty = new Party();

        // Harvesting & Activities
        public HarvestBox Harvest = new HarvestBox();
        public Activities Activity = new Activities();

        // Mantles
        public Mantle PrimaryMantle = new Mantle();
        public Mantle SecondaryMantle = new Mantle();

        // Abnormalities
        public Abnormalities Abnormalities = new Abnormalities();

        // Threading
        private ThreadStart ScanPlayerInfoRef;
        private Thread ScanPlayerInfo;

        ~Player() {
            PlayerParty = null;
            Harvest = null;
            PrimaryMantle = null;
            SecondaryMantle = null;
        }

        // Event handlers
        // Level event handler
        public delegate void PlayerEvents(object source, EventArgs args);
        public event PlayerEvents OnLevelChange;
        public event PlayerEvents OnZoneChange;
        public event PlayerEvents OnWeaponChange;
        public event PlayerEvents OnSessionChange;
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

        protected virtual void _onZoneChange() {
            OnZoneChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onWeaponChange() {
            OnWeaponChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onSessionChange() {
            OnSessionChange?.Invoke(this, EventArgs.Empty);
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
            ScanPlayerInfo = new Thread(ScanPlayerInfoRef) {
                Name = "Scanner_Player"
            };
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
                    GetFertilizers();
                    GetArgosyData();
                    GetTailraidersData();
                    GetSteamFuel();
                    GetPrimaryMantle();
                    GetSecondaryMantle();
                    GetPrimaryMantleTimers();
                    GetSecondaryMantleTimers();
                    GetParty();
                    GetPlayerAbnormalities();
                }
                GetZoneId();
                GetSessionId();
                GetEquipmentAddress();
                Thread.Sleep(150);
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private bool GetPlayerAddress() {
            Int64 AddressValue = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.WEAPON_OFFSET, Address.Offsets.WeaponOffsets);
            Int64 pAddress = 0x0;
            Int64 nextPlayer = 0x27E9F0;
            if (AddressValue > 0x0) {
                PlayerSelectedPointer = AddressValue;
                string pName = Scanner.READ_STRING(AddressValue - 0x270, 32);
                int pLevel = Scanner.READ_INT(AddressValue - 0x230);
                // If char name starts with a null char then the game haven't launched yet
                if (pName == "") return false;
                for (int playerSlot = 0; playerSlot < 3; playerSlot++) {
                    pAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.LEVEL_OFFSET, Address.Offsets.LevelOffsets) + (nextPlayer * playerSlot);
                    if (Scanner.READ_INT(pAddress) == pLevel && Scanner.READ_STRING(pAddress - 0x40, 32)?.Trim('\x00') == pName && PlayerAddress != pAddress) {
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
            Name = Scanner.READ_STRING(Address, 32);
        }

        private void GetZoneId() {
            Int64 ZoneAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ZONE_OFFSET, Address.Offsets.ZoneOffsets);
            int zoneId = Scanner.READ_INT(ZoneAddress);
            if (zoneId != ZoneID) {
                this.LastZoneID = ZoneID;
                this.ZoneID = zoneId;
            }
        }

        public void ChangeLastZone() {
            this.LastZoneID = ZoneID;
        }

        private void GetWeaponId() {
            Int64 Address = Memory.Address.BASE + Memory.Address.WEAPON_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.WeaponOffsets);
            PlayerStructAddress = Address;
            WeaponID = Scanner.READ_BYTE(Address);
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
        }

        private void GetSecondaryMantle() {
            Int64 Address = LEVEL_ADDRESS + 0x34 + 0x4;
            int mantleId = Scanner.READ_INT(Address);
            SecondaryMantle.SetID(mantleId);
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
            Int64 PartyContainer = Scanner.READ_MULTILEVEL_PTR(address, Address.Offsets.PartyOffsets) - 0x22B7;
            int totalDamage = 0;
            for (int i = 0; i < PlayerParty.MaxSize; i++) totalDamage += GetPartyMemberDamage(i);
            PlayerParty.TotalDamage = totalDamage;
            GetQuestElapsedTime();
            for (int i = 0; i < PlayerParty.MaxSize; i++) {
                string playerName = GetPartyMemberName(PartyContainer + (i * 0x1C0));
                byte playerWeapon = Scanner.READ_BYTE(PartyContainer + (i * 0x1C0 + 0x33));
                int playerDamage = GetPartyMemberDamage(i);
                float playerDamagePercentage = 0;
                if (totalDamage != 0) {
                    playerDamagePercentage = playerDamage / (float)totalDamage;
                }
                PlayerParty[i].SetPlayerInfo(playerName, playerWeapon, playerDamage, playerDamagePercentage);
            }

        }

        private void GetQuestElapsedTime() {
            Int64 EpochAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.DAMAGE_OFFSET, Address.Offsets.DamageOffsets);
            Int64 Epoch = Scanner.READ_LONGLONG(EpochAddress + 0x1028);
            PlayerParty.ShowDPS = true;
            PlayerParty.Epoch = DateTime.UtcNow - DateTimeOffset.FromUnixTimeSeconds(Epoch);
        }

        private int GetPartyMemberDamage(int playerIndex) {
            Int64 DPSAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.DAMAGE_OFFSET, Address.Offsets.DamageOffsets);
            return Scanner.READ_INT(DPSAddress + (0x2A0 * playerIndex));
        }

        private string GetPartyMemberName(Int64 NameAddress) {
            string PartyMemberName = Scanner.READ_STRING(NameAddress, 32);
            return PartyMemberName ?? PartyMemberName.Trim('\x00');
        }

        private void GetFertilizers() {
            Int64 Address = this.LEVEL_ADDRESS;
            
            for (int fertCount = 0; fertCount < 4; fertCount++) {
                // Calculates memory address
                Int64 FertilizerAddress = Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * fertCount);
                // Read memory
                int FertilizerId = Scanner.READ_INT(FertilizerAddress - 0x4);
                int FertilizerCount = Scanner.READ_INT(FertilizerAddress);
                // update fertilizer data
                Harvest.Box[fertCount].ID = FertilizerId;
                Harvest.Box[fertCount].Amount = FertilizerCount;
            }
            UpdateHarvestBoxCounter(Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * 3));
        }

        private void UpdateHarvestBoxCounter(Int64 LastFertAddress) {
            Int64 Address = LastFertAddress + Memory.Address.Offsets.HarvestBoxOffset;
            int counter = 0;
            for (long iAddress = Address; iAddress < Address + 0x330; iAddress += 0x10) {
                int memValue = Scanner.READ_INT(iAddress);
                if (memValue > 0) {
                    counter++;
                }
            }
            Harvest.Counter = counter;
        }

        private void GetSteamFuel() {
            Int64 NaturalFuelAddress = this.LEVEL_ADDRESS + Address.Offsets.SteamFuelOffset;
            Activity.NaturalFuel = Scanner.READ_INT(NaturalFuelAddress);
            Activity.StoredFuel = Scanner.READ_INT(NaturalFuelAddress + 0x4);
        }

        private void GetArgosyData() {
            Int64 ArgosyDaysAddress = this.LEVEL_ADDRESS + Address.Offsets.ArgosyOffset;
            byte ArgosyDays = Scanner.READ_BYTE(ArgosyDaysAddress);
            bool ArgosyInTown = ArgosyDays < 250;
            if (ArgosyDays >= 250) { ArgosyDays = (byte)(byte.MaxValue - ArgosyDays + 1); }
            Activity.SetArgosyInfo(ArgosyDays, ArgosyInTown);
        }

        private void GetTailraidersData() {
            Int64 TailraidersDaysAddress = this.LEVEL_ADDRESS + Address.Offsets.TailRaidersOffset;
            byte TailraidersQuestsDone = Scanner.READ_BYTE(TailraidersDaysAddress);
            bool isDeployed = TailraidersQuestsDone != 255;
            byte QuestsLeft = !isDeployed ? (byte)0 : (byte)(Activity.TailraidersMaxQuest - TailraidersQuestsDone);
            Activity.SetTailraidersInfo(QuestsLeft, isDeployed);
        }

        private void GetPlayerAbnormalities() {
            GetPlayerHuntingHornAbnormalities();
            GetPlayerPalicoAbnormalities();
            GetPlayerMiscAbnormalities();
        }

        private void GetPlayerHuntingHornAbnormalities() {
            // Gets the player abnormalities caused by HH
            Int64 HHAbnormalityAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets) - 0x4;
            for (int AbnormalityNumber = 0; AbnormalityNumber <= MaxHHBuffs; AbnormalityNumber++) {
                HHAbnormalityAddress += 0x4;
                float Duration = Scanner.READ_FLOAT(HHAbnormalityAddress);
                byte Stack = Scanner.READ_BYTE(HHAbnormalityAddress + (0x12C - (0x3 * AbnormalityNumber)));

                string AbnormalityID = $"HH_{AbnormalityNumber}";
                //Debugger.Log($"{HHAbnormalityAddress:X} = {AbnormalityNumber}");
                if (Duration <= 0) {
                    // Check if there's an abnormality with that ID
                    if (Abnormalities[AbnormalityID] != null) { Abnormalities.Remove(AbnormalityID); }
                    else { continue; }
                } else {
                    // Check for existing abnormalities before making a new one
                    if (Abnormalities[AbnormalityID] != null) { Abnormalities[AbnormalityID].UpdateAbnormalityInfo("HUNTINGHORN", Duration, Stack, AbnormalityNumber, true); }
                    else {
                        Abnormality NewAbnorm = new Abnormality();
                        NewAbnorm.UpdateAbnormalityInfo("HUNTINGHORN", Duration, Stack, AbnormalityNumber, true);
                        Abnormalities.Add(AbnormalityID, NewAbnorm);
                    }
                }
            }
        }

        private void GetPlayerPalicoAbnormalities() {
            // Gets the player abnormalities caused by palico's skills
            Int64 FirstHHBuffAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets) + 0xE4;

        }

        private void GetPlayerMiscAbnormalities() {
            // Gets the player abnormalities caused by consumables and blights
            Int64 FirstHHBuffAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets) + 0x5B4;

        }
    }
}
