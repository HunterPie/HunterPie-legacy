using System;
using System.Linq;
using System.Threading;
using HunterPie.Core.LPlayer;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Core.LPlayer.Jobs;

namespace HunterPie.Core
{
    public class Player
    {

        // Private variables
        private Int64 _playerAddress = 0x0;
        private int _level;
        private int _zoneId = -1;
        private byte _weaponId;
        private string _sessionId;

        // Game info
        private readonly int[] PeaceZones = new int[11] { 0, 301, 302, 303, 305, 306, 501, 502, 503, 504, 506 };
        private readonly int[] _HBZones = new int[9] { 301, 302, 303, 305, 306, 501, 502, 503, 506 };

        // Player info
        private Int64 SESSION_ADDRESS;
        private Int64 LEVEL_ADDRESS;
        private Int64 EQUIPMENT_ADDRESS;
        private Int64 PlayerStructAddress;
        public Int64 PlayerAddress
        {
            get => _playerAddress;
            set
            {
                if (_playerAddress != value)
                {
                    _playerAddress = value;
                    if (value != 0x0)
                    {
                        Debugger.Debug($"Found player address -> {value:X}");
                        _onLogin();
                    }
                }
            }
        }
        public int Level
        { // Hunter Rank
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;
                    _onLevelUp();
                }
            }
        }
        public int MasterRank { get; private set; }
        public string Name { get; private set; }
        public int ZoneID
        {
            get => _zoneId;
            set
            {
                if (_zoneId != value)
                {
                    if ((_zoneId == -1 || PeaceZones.Contains(_zoneId)) && !PeaceZones.Contains(value)) _onPeaceZoneLeave();
                    if (_HBZones.Contains(_zoneId) && !_HBZones.Contains(value)) _onVillageLeave();
                    _zoneId = value;
                    _onZoneChange();
                    if (PeaceZones.Contains(value)) _onPeaceZoneEnter();
                    if (_HBZones.Contains(value)) _onVillageEnter();
                    if (value == 0 && LEVEL_ADDRESS != 0x0)
                    {
                        LEVEL_ADDRESS = 0x0;
                        PlayerAddress = 0x0;
                        _onLogout();
                    }
                }
            }
        }
        public string ZoneName => GStrings.GetStageNameByID(ZoneID);
        public int LastZoneID { get; private set; }
        public byte WeaponID
        {
            get => _weaponId;
            set
            {
                if (_weaponId != value)
                {
                    _weaponId = value;
                    _onWeaponChange();
                }
            }
        }
        public string WeaponName => GStrings.GetWeaponNameByID(WeaponID);
        public string SessionID
        {
            get => _sessionId;
            set
            {
                if (_sessionId != value)
                {
                    _sessionId = value;
                    GetSteamSession();
                    _onSessionChange();
                }
            }
        }
        public bool InPeaceZone => PeaceZones.Contains(ZoneID);
        public bool InHarvestZone => _HBZones.Contains(ZoneID);
        public Int64 SteamSession { get; private set; }
        public Int64 SteamID { get; private set; }

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

        // Job data
        public Greatsword Greatsword = new Greatsword();
        public DualBlades DualBlades = new DualBlades();
        public Longsword Longsword = new Longsword();
        public Hammer Hammer = new Hammer();
        public GunLance GunLance = new GunLance();
        public SwitchAxe SwitchAxe = new SwitchAxe();
        public ChargeBlade ChargeBlade = new ChargeBlade();
        public InsectGlaive InsectGlaive = new InsectGlaive();
        public Bow Bow = new Bow();
        public LightBowgun LightBowgun = new LightBowgun();
        public HeavyBowgun HeavyBowgun = new HeavyBowgun();

        // Threading
        private ThreadStart ScanPlayerInfoRef;
        private Thread ScanPlayerInfo;

        ~Player()
        {
            PlayerParty = null;
            Harvest = null;
            PrimaryMantle = null;
            SecondaryMantle = null;
        }

        #region Events
        // Event handlers

        public delegate void PlayerEvents(object source, EventArgs args);
        public event PlayerEvents OnLevelChange;
        public event PlayerEvents OnZoneChange;
        public event PlayerEvents OnWeaponChange;
        public event PlayerEvents OnSessionChange;
        public event PlayerEvents OnCharacterLogin;
        public event PlayerEvents OnCharacterLogout;
        public event PlayerEvents OnPeaceZoneEnter;
        public event PlayerEvents OnVillageEnter;
        public event PlayerEvents OnPeaceZoneLeave;
        public event PlayerEvents OnVillageLeave;

        // Dispatchers
        protected virtual void _onLogin() => OnCharacterLogin?.Invoke(this, EventArgs.Empty);

        protected virtual void _onLogout() => OnCharacterLogout?.Invoke(this, EventArgs.Empty);

        protected virtual void _onLevelUp() => OnLevelChange?.Invoke(this, EventArgs.Empty);

        protected virtual void _onZoneChange() => OnZoneChange?.Invoke(this, EventArgs.Empty);

        protected virtual void _onWeaponChange() => OnWeaponChange?.Invoke(this, EventArgs.Empty);

        protected virtual void _onSessionChange() => OnSessionChange?.Invoke(this, EventArgs.Empty);

        protected virtual void _onPeaceZoneEnter() => OnPeaceZoneEnter?.Invoke(this, EventArgs.Empty);

        protected virtual void _onVillageEnter() => OnVillageEnter?.Invoke(this, EventArgs.Empty);

        protected virtual void _onPeaceZoneLeave() => OnPeaceZoneLeave?.Invoke(this, EventArgs.Empty);

        protected virtual void _onVillageLeave() => OnVillageLeave?.Invoke(this, EventArgs.Empty);
        #endregion


        #region Scanner
        public void StartScanning()
        {
            ScanPlayerInfoRef = new ThreadStart(GetPlayerInfo);
            ScanPlayerInfo = new Thread(ScanPlayerInfoRef)
            {
                Name = "Scanner_Player"
            };
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLAYER_SCANNER_INITIALIZED']"));
            ScanPlayerInfo.Start();
        }

        public void StopScanning() => ScanPlayerInfo.Abort();
        #endregion

        #region Manual Player Data
        /*
            Player data that needs to be called by an external function and doesn't need to keep track of this data
            every second.
        */
        public GameStructs.Gear GetPlayerGear()
        {
            Int64 PlayerGearBase = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerGearOffsets);

            // Helm
            GameStructs.Armor Helm = new GameStructs.Armor()
            {
                ID = Scanner.Read<int>(PlayerGearBase),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 0)
            };

            // Chest
            GameStructs.Armor Chest = new GameStructs.Armor()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0x4),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 1)
            };

            // Arms
            GameStructs.Armor Arms = new GameStructs.Armor()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0x8),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 2)
            };

            // Waist
            GameStructs.Armor Waist = new GameStructs.Armor()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0xC),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 3)
            };

            // Waist
            GameStructs.Armor Legs = new GameStructs.Armor()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0x10),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 4)
            };

            // Charm
            GameStructs.Charm Charm = new GameStructs.Charm()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0x14)
            };

            // Weapon
            GameStructs.Weapon Weapon = new GameStructs.Weapon()
            {
                Type = Scanner.Read<int>(PlayerGearBase + 0x124),
                ID = Scanner.Read<int>(PlayerGearBase + 0x128),
                Decorations = GetWeaponDecorations(PlayerGearBase + 0x128),
                NewAugments = GetWeaponNewAugments(PlayerGearBase + 0x128),
                Awakenings = GetWeaponAwakenedSkills(PlayerGearBase + 0x128),
                CustomAugments = GetCustomAugments(PlayerGearBase + 0x128),
                BowgunMods = GetBowgunMods(PlayerGearBase + 0x128)
            };

            // Primary Tool
            GameStructs.SpecializedTool PrimaryTool = new GameStructs.SpecializedTool()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0x158),
                Decorations = GetMantleDecorations(PlayerGearBase + 0x164)
            };

            // Secondary Tool
            GameStructs.SpecializedTool SecondaryTool = new GameStructs.SpecializedTool()
            {
                ID = Scanner.Read<int>(PlayerGearBase + 0x15C),
                Decorations = GetMantleDecorations(PlayerGearBase + 0x170)
            };
            // Now we put all the data in the player gear struct
            GameStructs.Gear PlayerGear = new GameStructs.Gear()
            {
                Helmet = Helm,
                Chest = Chest,
                Hands = Arms,
                Waist = Waist,
                Legs = Legs,
                Charm = Charm,
                Weapon = Weapon,
                SpecializedTools = new GameStructs.SpecializedTool[2] {
                    PrimaryTool, SecondaryTool
                }
            };
            return PlayerGear;
        }

        private GameStructs.BowgunMod[] GetBowgunMods(Int64 BaseAddress)
        {
            GameStructs.BowgunMod[] bowgunMods = new GameStructs.BowgunMod[5];
            for (int i = 0; i < 5; i++)
            {
                GameStructs.BowgunMod dummy = new GameStructs.BowgunMod()
                {
                    ID = GameStructs.ConvertToMax(Scanner.Read<uint>(BaseAddress + 0x10 + (i * 4)))
                };
                bowgunMods[i] = dummy;
            }
            return bowgunMods;
        }

        private GameStructs.NewAugment[] GetWeaponNewAugments(Int64 BaseAddress)
        {
            GameStructs.NewAugment[] NewAugments = new GameStructs.NewAugment[7];
            // New augments can be determined by their index, so we use their index as 
            // an ID. Their value is a byte that holds the augment level.
            for (int AugmentIndex = 0; AugmentIndex < 7; AugmentIndex++)
            {
                GameStructs.NewAugment dummy = new GameStructs.NewAugment()
                {
                    ID = (byte)AugmentIndex,
                    Level = Scanner.Read<byte>(BaseAddress + 0x84 + AugmentIndex)
                };
                NewAugments[AugmentIndex] = dummy;
            }
            return NewAugments;
        }

        private GameStructs.AwakenedSkill[] GetWeaponAwakenedSkills(Int64 BaseAddress)
        {
            GameStructs.AwakenedSkill[] AwakenedSkills = new GameStructs.AwakenedSkill[5];
            // Awakened skills slots are determined by their index, their value is a short that
            // holds their awakened skill ID
            for (int AwakIndex = 0; AwakIndex < 5; AwakIndex++)
            {
                GameStructs.AwakenedSkill dummy = new GameStructs.AwakenedSkill()
                {
                    ID = Scanner.Read<short>(BaseAddress + 0x8C + (AwakIndex * sizeof(short)))
                };
                AwakenedSkills[AwakIndex] = dummy;
            }
            return AwakenedSkills;
        }

        private GameStructs.CustomAugment[] GetCustomAugments(Int64 BaseAddress)
        {
            GameStructs.CustomAugment[] CustomAugments = new GameStructs.CustomAugment[7];
            for (int AugIndex = 0; AugIndex < 7; AugIndex++)
            {
                GameStructs.CustomAugment dummy = new GameStructs.CustomAugment()
                {
                    ID = Scanner.Read<byte>(BaseAddress + 0x78 + AugIndex),
                    Level = (byte)AugIndex
                };
                CustomAugments[AugIndex] = dummy;
            }
            return CustomAugments;
        }

        private GameStructs.Decoration[] GetDecorationsFromGear(Int64 BaseAddress, int GearIndex)
        {
            GameStructs.Decoration[] Decorations = new GameStructs.Decoration[3];
            for (int DecorationIndex = 0; DecorationIndex < 3; DecorationIndex++)
            {
                GameStructs.Decoration dummy = new GameStructs.Decoration()
                {
                    ID = GameStructs.ConvertToMax(Scanner.Read<uint>(BaseAddress + 0x30 + (0x3 * GearIndex * 0x4) + (0x4 * DecorationIndex)))
                };
                Decorations[DecorationIndex] = dummy;
            }
            return Decorations;
        }

        private GameStructs.Decoration[] GetWeaponDecorations(Int64 BaseAddress)
        {
            GameStructs.Decoration[] Decorations = new GameStructs.Decoration[3];
            for (int DecorationIndex = 0; DecorationIndex < 3; DecorationIndex++)
            {
                GameStructs.Decoration dummy = new GameStructs.Decoration()
                {
                    ID = GameStructs.ConvertToMax(Scanner.Read<uint>(BaseAddress + ((DecorationIndex + 1) * 0x4)))
                };
                Decorations[DecorationIndex] = dummy;
            }
            return Decorations;
        }

        private GameStructs.Augment[] GetWeaponAugments(Int64 BaseAddress)
        {
            GameStructs.Augment[] Augments = new GameStructs.Augment[3];
            for (int AugmentIndex = 0; AugmentIndex < 3; AugmentIndex++)
            {
                GameStructs.Augment dummy = new GameStructs.Augment()
                {
                    ID = Scanner.Read<int>(BaseAddress + 0x24 + (AugmentIndex * 0x4))
                };
                Augments[AugmentIndex] = dummy;
            }
            return Augments;
        }

        private GameStructs.Decoration[] GetMantleDecorations(Int64 BaseAddress)
        {
            GameStructs.Decoration[] Decorations = new GameStructs.Decoration[2];
            for (int DecorationIndex = 0; DecorationIndex < 2; DecorationIndex++)
            {
                GameStructs.Decoration dummy = new GameStructs.Decoration()
                {
                    ID = GameStructs.ConvertToMax(Scanner.Read<uint>(BaseAddress + (DecorationIndex * 0x4)))
                };
                Decorations[DecorationIndex] = dummy;
            }
            return Decorations;
        }

        #endregion

        #region Automatic Player Data
        /*
            Player data that is tracked by the Player class, cannot be called by an external function.
        */

        private void GetPlayerInfo()
        {
            while (Scanner.GameIsRunning)
            {
                GetZoneId();
                if (GetPlayerAddress())
                {
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
                    GetJobInformation();
                }
                GetSessionId();
                GetEquipmentAddress();
                Thread.Sleep(Math.Max(50, UserSettings.PlayerConfig.Overlay.GameScanDelay));
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private bool GetPlayerAddress()
        {
            Int64 AddressValue = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.WEAPON_OFFSET, Address.Offsets.WeaponOffsets);
            Int64 nextPlayer = 0x27E9F0;
            if (AddressValue > 0x0)
            {

                string pName = Scanner.READ_STRING(AddressValue - 0x270, 32);
                int pLevel = Scanner.Read<int>(AddressValue - 0x230);
                // If char name starts with a null char then the game haven't launched yet
                if (pName == "") return false;
                for (int playerSlot = 0; playerSlot < 3; playerSlot++)
                {
                    long pAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.LEVEL_OFFSET, Address.Offsets.LevelOffsets) + (nextPlayer * playerSlot);
                    if (Scanner.Read<int>(pAddress) == pLevel && Scanner.READ_STRING(pAddress - 0x40, 32)?.Trim('\x00') == pName && PlayerAddress != pAddress)
                    {
                        LEVEL_ADDRESS = pAddress;
                        GetPlayerLevel();
                        GetPlayerName();
                        PlayerAddress = pAddress;
                        return true;
                    }
                }
            }
            else
            {
                PlayerAddress = 0x0;
                LEVEL_ADDRESS = 0x0;
                return false;
            }
            return true;
        }

        private void GetPlayerLevel() => Level = Scanner.Read<int>(LEVEL_ADDRESS);

        private void GetPlayerMasterRank() => MasterRank = Scanner.Read<int>(LEVEL_ADDRESS + 0x44);

        private void GetPlayerName()
        {
            Int64 Address = LEVEL_ADDRESS - 0x40;
            Name = Scanner.READ_STRING(Address, 32);
        }

        private void GetZoneId()
        {
            Int64 ZoneAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ZONE_OFFSET, Address.Offsets.ZoneOffsets);
            int zoneId = Scanner.Read<int>(ZoneAddress);
            if (zoneId != ZoneID)
            {
                LastZoneID = ZoneID;
                ZoneID = zoneId;
            }
        }

        public void ChangeLastZone() => LastZoneID = ZoneID;

        private void GetWeaponId()
        {
            Int64 Address = Memory.Address.BASE + Memory.Address.WEAPON_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.WeaponOffsets);
            PlayerStructAddress = Address;
            WeaponID = Scanner.Read<byte>(Address);
        }

        private void GetSessionId()
        {
            Int64 Address = Memory.Address.BASE + Memory.Address.SESSION_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.SessionOffsets);
            SESSION_ADDRESS = Address;
            SessionID = Scanner.READ_STRING(SESSION_ADDRESS, 12);
        }

        private void GetSteamSession()
        {
            SteamSession = Scanner.Read<long>(SESSION_ADDRESS + 0x10);
            SteamID = Scanner.Read<long>(SESSION_ADDRESS + 0x1184);
            Debugger.Debug($"Steam Session: {SteamSession}/{SteamID}");
        }

        private void GetEquipmentAddress()
        {
            Int64 Address = Memory.Address.BASE + Memory.Address.EQUIPMENT_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.EquipmentOffsets);
            if (EQUIPMENT_ADDRESS != Address) Debugger.Debug($"New equipment address found -> 0x{Address:X}");
            EQUIPMENT_ADDRESS = Address;
        }

        private void GetPrimaryMantle()
        {
            Int64 Address = PlayerStructAddress + 0x34;
            int mantleId = Scanner.Read<int>(Address);
            PrimaryMantle.SetID(mantleId);
        }

        private void GetSecondaryMantle()
        {
            Int64 Address = PlayerStructAddress + 0x34 + 0x4;
            int mantleId = Scanner.Read<int>(Address);
            SecondaryMantle.SetID(mantleId);
        }

        private void GetPrimaryMantleTimers()
        {
            Int64 PrimaryMantleTimerFixed = (PrimaryMantle.ID * 4) + Address.timerFixed;
            Int64 PrimaryMantleTimer = (PrimaryMantle.ID * 4) + Address.timerDynamic;
            Int64 PrimaryMantleCdFixed = (PrimaryMantle.ID * 4) + Address.cooldownFixed;
            Int64 PrimaryMantleCdDynamic = (PrimaryMantle.ID * 4) + Address.cooldownDynamic;
            PrimaryMantle.SetCooldown(Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleCdDynamic), Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleCdFixed));
            PrimaryMantle.SetTimer(Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleTimer), Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleTimerFixed));
        }

        private void GetSecondaryMantleTimers()
        {
            Int64 SecondaryMantleTimerFixed = (SecondaryMantle.ID * 4) + Address.timerFixed;
            Int64 SecondaryMantleTimer = (SecondaryMantle.ID * 4) + Address.timerDynamic;
            Int64 SecondaryMantleCdFixed = (SecondaryMantle.ID * 4) + Address.cooldownFixed;
            Int64 SecondaryMantleCdDynamic = (SecondaryMantle.ID * 4) + Address.cooldownDynamic;
            SecondaryMantle.SetCooldown(Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleCdDynamic), Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleCdFixed));
            SecondaryMantle.SetTimer(Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleTimer), Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleTimerFixed));
        }

        private void GetParty()
        {
            
            Int64 address = Address.BASE + Address.PARTY_OFFSET;
            Int64 PartyContainer = Scanner.READ_MULTILEVEL_PTR(address, Address.Offsets.PartyOffsets) - 0x22B7;
            if (InPeaceZone)
            {
                PlayerParty.LobbySize = Scanner.Read<int>(PartyContainer - 0xA961);
            }
            else
            {
                int totalDamage = 0;
                int[] playerDamages = new int[PlayerParty.MaxSize];
                for (int i = 0; i < PlayerParty.MaxSize; i++)
                {
                    int playerDamage = GetPartyMemberDamage(i);
                    totalDamage += playerDamage;
                    playerDamages[i] = playerDamage;
                }

                PlayerParty.TotalDamage = totalDamage;
                GetQuestElapsedTime();
                for (int i = 0; i < PlayerParty.MaxSize; i++)
                {
                    string playerName = GetPartyMemberName(PartyContainer + (i * 0x1C0));
                    short HR = Scanner.Read<short>(PartyContainer + (i * 0x1C0 + 0x27));
                    short MR = Scanner.Read<short>(PartyContainer + (i * 0x1C0 + 0x29));
                    byte playerWeapon = playerName == Name && HR == Level ? WeaponID : Scanner.Read<byte>(PartyContainer + (i * 0x1C0 + 0x33));
                    int playerDamage = playerDamages[i];
                    float playerDamagePercentage = 0;
                    if (totalDamage != 0)
                    {
                        playerDamagePercentage = playerDamage / (float)totalDamage;
                    }

                    if (i == 0) PlayerParty[i].IsPartyLeader = true;
                    
                    PlayerParty[i].HR = HR;
                    PlayerParty[i].MR = MR;
                    PlayerParty[i].IsMe = playerName == Name && HR == Level;
                    PlayerParty[i].SetPlayerInfo(playerName, playerWeapon, playerDamage, playerDamagePercentage);
                }
            }

        }

        private void GetQuestElapsedTime()
        {
            Int64 TimerAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            float Timer = Scanner.Read<float>(TimerAddress + 0xB74);
            PlayerParty.ShowDPS = true;
            if (Timer > 0)
            {
                PlayerParty.Epoch = TimeSpan.FromSeconds(Timer);
            }
            else { PlayerParty.Epoch = TimeSpan.Zero; }
        }

        private int GetPartyMemberDamage(int playerIndex)
        {
            Int64 DPSAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.DAMAGE_OFFSET, Address.Offsets.DamageOffsets);
            return Scanner.Read<int>(DPSAddress + (0x2A0 * playerIndex));
        }

        private string GetPartyMemberName(Int64 NameAddress)
        {
            string PartyMemberName = Scanner.READ_STRING(NameAddress, 32);
            return PartyMemberName ?? PartyMemberName.Trim('\x00');
        }

        private void GetFertilizers()
        {
            Int64 Address = LEVEL_ADDRESS;

            for (int fertCount = 0; fertCount < 4; fertCount++)
            {
                // Calculates memory address
                Int64 FertilizerAddress = Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * fertCount);
                // Read memory
                int FertilizerId = Scanner.Read<int>(FertilizerAddress - 0x4);
                int FertilizerCount = Scanner.Read<int>(FertilizerAddress);
                // update fertilizer data
                Harvest.Box[fertCount].ID = FertilizerId;
                Harvest.Box[fertCount].Amount = FertilizerCount;
            }
            UpdateHarvestBoxCounter(Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * 3));
        }

        private void UpdateHarvestBoxCounter(Int64 LastFertAddress)
        {
            Int64 Address = LastFertAddress + Memory.Address.Offsets.HarvestBoxOffset;
            int counter = 0;
            for (long iAddress = Address; iAddress < Address + 0x330; iAddress += 0x10)
            {
                int memValue = Scanner.Read<int>(iAddress);
                if (memValue > 0)
                {
                    counter++;
                }
            }
            Harvest.Counter = counter;
        }

        private void GetSteamFuel()
        {
            Int64 NaturalFuelAddress = LEVEL_ADDRESS + Address.Offsets.SteamFuelOffset;
            Activity.NaturalFuel = Scanner.Read<int>(NaturalFuelAddress);
            Activity.StoredFuel = Scanner.Read<int>(NaturalFuelAddress + 0x4);
        }

        private void GetArgosyData()
        {
            Int64 ArgosyDaysAddress = LEVEL_ADDRESS + Address.Offsets.ArgosyOffset;
            byte ArgosyDays = Scanner.Read<byte>(ArgosyDaysAddress);
            bool ArgosyInTown = ArgosyDays < 250;
            if (ArgosyDays >= 250) { ArgosyDays = (byte)(byte.MaxValue - ArgosyDays + 1); }
            Activity.SetArgosyInfo(ArgosyDays, ArgosyInTown);
        }

        private void GetTailraidersData()
        {
            Int64 TailraidersDaysAddress = LEVEL_ADDRESS + Address.Offsets.TailRaidersOffset;
            byte TailraidersQuestsDone = Scanner.Read<byte>(TailraidersDaysAddress);
            bool isDeployed = TailraidersQuestsDone != 255;
            byte QuestsLeft = !isDeployed ? (byte)0 : (byte)(Activity.TailraidersMaxQuest - TailraidersQuestsDone);
            Activity.SetTailraidersInfo(QuestsLeft, isDeployed);
        }

        private void GetPlayerAbnormalities()
        {
            if (InHarvestZone)
            {
                Abnormalities.ClearAbnormalities();
                return;
            }
            long abnormalityBaseAddress = Scanner.READ_MULTILEVEL_PTR(
                Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            GetPlayerHuntingHornAbnormalities(abnormalityBaseAddress);
            GetPlayerPalicoAbnormalities(abnormalityBaseAddress);
            GetPlayerMiscAbnormalities(abnormalityBaseAddress);
            GetPlayerGearAbnormalities(abnormalityBaseAddress);
        }

        private void GetPlayerHuntingHornAbnormalities(long abnormalityBaseAddress)
        {
            // Gets the player abnormalities caused by HH
            foreach (AbnormalityInfo abnormality in AbnormalityData.HuntingHornAbnormalities)
            {
                UpdateAbnormality(abnormality, abnormalityBaseAddress);
            }
        }

        private void GetPlayerPalicoAbnormalities(long abnormalityBaseAddress)
        {
            // Gets the player abnormalities caused by palico's skills
            foreach (AbnormalityInfo abnormality in AbnormalityData.PalicoAbnormalities)
            {
                UpdateAbnormality(abnormality, abnormalityBaseAddress);
            }
        }

        private void GetPlayerMiscAbnormalities(long abnormalityBaseAddress)
        {
            // Gets the player abnormalities caused by consumables and blights
            // Blights
            foreach (AbnormalityInfo abnormality in AbnormalityData.BlightAbnormalities)
            {
                UpdateAbnormality(abnormality, abnormalityBaseAddress);
            }

            foreach (AbnormalityInfo abnormality in AbnormalityData.MiscAbnormalities)
            {
                UpdateAbnormality(abnormality, abnormalityBaseAddress);
            }
        }

        private void GetPlayerGearAbnormalities(long abnormalityBaseAddress)
        {
            long abnormalityGearBase = Scanner.READ_MULTILEVEL_PTR(
                Address.BASE + Address.ABNORMALITY_OFFSET,
                Address.Offsets.AbnormalityGearOffsets);

            foreach (AbnormalityInfo abnormality in AbnormalityData.GearAbnormalities)
            {
                UpdateAbnormality(abnormality, (abnormality.IsGearBuff ? abnormalityGearBase : abnormalityBaseAddress));
            }
        }

        private void UpdateAbnormality(AbnormalityInfo info, long baseAddress)
        {
            const int firstHornBuffOffset = 0x38;
            long abnormalityAddress = baseAddress + info.Offset;
            float duration = Scanner.Read<float>(abnormalityAddress);

            bool hasConditions = info.HasConditions;
            bool DebuffCondition = false;
            byte stack = 0;
            // Palico and misc buffs don't stack
            switch (info.Type)
            {
                case "HUNTINGHORN":
                    stack = Scanner.Read<byte>(baseAddress + 0x164 + (info.Offset - firstHornBuffOffset) / 4);
                    break;
                case "DEBUFF":
                    if (info.HasConditions)
                    {
                        stack = Scanner.Read<byte>(baseAddress + info.Offset + info.ConditionOffset);
                        DebuffCondition = stack == 0;
                    }
                    break;
                case "MISC":
                    if (info.HasConditions)
                    {
                        stack = Scanner.Read<byte>(baseAddress + info.Offset + info.ConditionOffset);
                        hasConditions = stack > 0;
                    }
                    break;
            }

            if ((int)duration <= 0 && !(hasConditions && info.IsInfinite))
            {
                if (Abnormalities[info.InternalId] != null)
                {
                    Abnormalities.Remove(info.InternalId);
                }

                return;
            }

            if (stack < info.Stack)
                return;

            if (DebuffCondition) return;

            if (Abnormalities[info.InternalId] != null)
            {
                Abnormalities[info.InternalId].UpdateAbnormalityInfo(duration, stack);
            }
            else
            {
                var a = new Abnormality(info);
                a.UpdateAbnormalityInfo(duration, stack);
                Abnormalities.Add(info.InternalId, a);
            }
        }

        private void GetJobInformation()
        {
            long weaponAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.WEAPON_MECHANICS_OFFSET, Address.Offsets.WeaponMechanicsOffsets);
            switch((Classes)WeaponID)
            {
                case Classes.Greatsword:
                    GetGreatswordInformation(weaponAddress);
                    break;
                case Classes.DualBlades:
                    GetDualBladesInformation(weaponAddress);
                    break;
                case Classes.LongSword:
                    GetLongswordInformation(weaponAddress);
                    break;
                case Classes.Hammer:
                    GetHammerInformation(weaponAddress);
                    break;
                case Classes.GunLance:
                    GetGunLanceInformation(weaponAddress);
                    break;
                case Classes.SwitchAxe:
                    GetSwitchAxeInformation(weaponAddress);
                    break;
                case Classes.ChargeBlade:
                    GetChargeBladeInformation(weaponAddress);
                    break;
                case Classes.InsectGlaive:
                    GetInsectGlaiveInformation(weaponAddress);
                    break;
                case Classes.Bow:
                    GetBowInformation(weaponAddress);
                    break;
                case Classes.HeavyBowgun:
                    GetHeavyBowgunInformation(weaponAddress);
                    break;
                case Classes.LightBowgun:
                    GetLightBowgunInformation(weaponAddress);
                    break;
            }
        }

        private void GetGreatswordInformation(long weaponAddress)
        {
            uint chargeLevel = Scanner.Read<uint>(weaponAddress - 0x14);
            Greatsword.ChargeLevel = chargeLevel;
        }

        private void GetDualBladesInformation(long weaponAddress)
        {
            bool inDemonMode = Scanner.Read<bool>(weaponAddress - 0x4);
            float demonGauge = Scanner.Read<float>(weaponAddress);
            DualBlades.InDemonMode = inDemonMode;
            DualBlades.DemonGauge = demonGauge;
        }

        private void GetLongswordInformation(long weaponAddress)
        {
            float gauge = Scanner.Read<float>(weaponAddress - 0x4);
            int chargeLevel = Scanner.Read<int>(weaponAddress + 0x4);
            float chargeGauge = Scanner.Read<float>(weaponAddress + 0x8);
            Longsword.InnerGauge = gauge;
            Longsword.ChargeLevel = chargeLevel;
            Longsword.OuterGauge = chargeGauge;
        }

        private void GetHammerInformation(long weaponAddress)
        {
            bool isPowerCharged = Scanner.Read<bool>(weaponAddress - 0x18);
            int chargeLevel = Scanner.Read<int>(weaponAddress - 0x10);
            Hammer.IsPowerCharged = isPowerCharged;
            Hammer.ChargeLevel = chargeLevel;
        }

        private void GetGunLanceInformation(long weaponAddress)
        {
            long AbnormalitiesAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            int totalAmmo = Scanner.Read<int>(weaponAddress - 0x4);
            int currentAmmo = Scanner.Read<int>(weaponAddress);
            int totalBigAmmo = Scanner.Read<int>(weaponAddress + 0xC);
            int currentBigAmmo = Scanner.Read<int>(weaponAddress + 0x10);
            float wyvernblast = Scanner.Read<float>(AbnormalitiesAddress + 0xB70);
            GunLance.TotalAmmo = totalAmmo;
            GunLance.Ammo = currentAmmo;
            GunLance.TotalBigAmmo = totalBigAmmo;
            GunLance.BigAmmo = currentBigAmmo;
            GunLance.WyvernblastTimer = wyvernblast;
        }

        private void GetSwitchAxeInformation(long weaponAddress)
        {
            float outerGauge = Scanner.Read<float>(weaponAddress - 0xC);
            float innerGauge = Scanner.Read<float>(weaponAddress - 0x1C);
            SwitchAxe.OuterGauge = outerGauge;
            SwitchAxe.InnerGauge = innerGauge;
        }

        private void GetChargeBladeInformation(long weaponAddress)
        {
            float hiddenGauge = Scanner.Read<float>(weaponAddress + 0x4);
            float shieldBuff = Scanner.Read<float>(weaponAddress + 0xC);
            float swordBuff = Scanner.Read<float>(weaponAddress + 0x10);
            int vialsAmount = Scanner.Read<int>(weaponAddress + 0x8);
            float poweraxeBuff = Scanner.Read<float>(weaponAddress + 0x104);
            ChargeBlade.VialChargeGauge = hiddenGauge;
            ChargeBlade.ShieldBuffTimer = shieldBuff;
            ChargeBlade.SwordBuffTimer = swordBuff;
            ChargeBlade.Vials = vialsAmount;
            ChargeBlade.PoweraxeTimer = poweraxeBuff;
        }

        private void GetInsectGlaiveInformation(long weaponAddress)
        {
            float redBuff = Scanner.Read<float>(weaponAddress - 0x4);
            float whiteBuff = Scanner.Read<float>(weaponAddress);
            float orangeBuff = Scanner.Read<float>(weaponAddress + 0x4);
            InsectGlaive.RedBuff = redBuff;
            InsectGlaive.WhiteBuff = whiteBuff;
            InsectGlaive.OrangeBuff = orangeBuff;
        }

        private void GetBowInformation(long weaponAddress)
        {
            int chargeLevel = Scanner.Read<int>(weaponAddress + 0x68);
            Bow.ChargeLevel = chargeLevel;
        }

        private void GetLightBowgunInformation(long weaponAddress)
        {
            float specialAmmoTimer = Scanner.Read<float>(weaponAddress + 0x4E0);
            LightBowgun.SpecialAmmoRegen = specialAmmoTimer;
        }

        private void GetHeavyBowgunInformation(long weaponAddress)
        {
            float wyvernsnipe = Scanner.Read<float>(weaponAddress - 0xC);
            float wyvernheart = Scanner.Read<float>(weaponAddress - 0x14);
            HeavyBowgun.WyvernsnipeTimer = wyvernsnipe;
            HeavyBowgun.WyvernheartTimer = wyvernheart;
        }

        #endregion
    }
}
