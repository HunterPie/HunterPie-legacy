using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HunterPie.Core.Definitions;
using HunterPie.Core.LPlayer;
using HunterPie.Core.LPlayer.Jobs;
using HunterPie.Logger;
using HunterPie.Memory;
using Classes = HunterPie.Core.Enums.Classes;

namespace HunterPie.Core
{
    public class Player
    {

        private long playerAddress = 0x0;
        private int level;
        private int zoneId = -1;
        private byte weaponId;
        private string sessionId;
        private long classAddress;

        private readonly int[] HarvestBoxZones =
        {
            301,
            302,
            303,
            305,
            306,
            501,
            502,
            503,
            506
        };
        private readonly int[] PeaceZones =
        {
            0,
            301,
            302,
            303,
            305,
            306,
            501,
            502,
            503,
            506
        };

        private long SESSION_ADDRESS { get; set; }
        private long LEVEL_ADDRESS { get; set; }
        private long EQUIPMENT_ADDRESS { get; set; }
        private long PlayerStructAddress { get; set; }

        public long PlayerAddress
        {
            get => playerAddress;
            set
            {
                if (playerAddress != value)
                {
                    playerAddress = value;
                    if (value != 0x0)
                    {
                        Debugger.Debug($"Found player address -> {value:X}");
                        Dispatch(OnCharacterLogin);
                    }
                }
            }
        }
        public string Name { get; private set; }
        public int Level
        {
            get => level;
            set
            {
                if (level != value)
                {
                    level = value;
                    Dispatch(OnLevelChange);
                }
            }
        }
        public int MasterRank { get; private set; }
        public int PlayTime { get; private set; }
        public bool IsLoggedOn => playerAddress != 0;

        public byte WeaponID
        {
            get => weaponId;
            set
            {
                if (weaponId != value)
                {
                    weaponId = value;
                    Dispatch(OnWeaponChange);
                }
            }
        }
        public string WeaponName => GStrings.GetWeaponNameByID(WeaponID);
        public long ClassAddress
        {
            get => classAddress;
            set
            {
                if (value != classAddress)
                {
                    classAddress = value;
                    Dispatch(OnClassChange);
                }
            }
        }

        public int ZoneID
        {
            get => zoneId;
            set
            {
                if (zoneId != value)
                {
                    if ((zoneId == -1 || PeaceZones.Contains(zoneId)) && !PeaceZones.Contains(value)) Dispatch(OnPeaceZoneLeave);
                    if (HarvestBoxZones.Contains(zoneId) && !HarvestBoxZones.Contains(value)) Dispatch(OnVillageLeave);
                    zoneId = value;
                    Dispatch(OnZoneChange);
                    if (PeaceZones.Contains(value)) Dispatch(OnPeaceZoneEnter);
                    if (HarvestBoxZones.Contains(value)) Dispatch(OnVillageEnter);
                    if (value == 0 && LEVEL_ADDRESS != 0x0)
                    {
                        LEVEL_ADDRESS = 0x0;
                        PlayerAddress = 0x0;
                        Dispatch(OnCharacterLogout);
                    }
                }
            }
        }
        public string ZoneName => GStrings.GetStageNameByID(ZoneID);
        public int LastZoneID { get; private set; }
        public bool InPeaceZone => PeaceZones.Contains(ZoneID);
        public bool InHarvestZone => HarvestBoxZones.Contains(ZoneID);

        public string SessionID
        {
            get => sessionId;
            set
            {
                if (sessionId != value)
                {
                    sessionId = value;
                    GetSteamSession();
                    Dispatch(OnSessionChange);
                }
            }
        }
        public long SteamSession { get; private set; }
        public long SteamID { get; private set; }

        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Stamina { get; set; }
        public float MaxStamina { get; set; }

        readonly Vector3 Position = new Vector3();

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

        #region Jobs
        public Greatsword Greatsword = new Greatsword();
        public DualBlades DualBlades = new DualBlades();
        public Longsword Longsword = new Longsword();
        public Hammer Hammer = new Hammer();
        public Lance Lance = new Lance();
        public GunLance GunLance = new GunLance();
        public SwitchAxe SwitchAxe = new SwitchAxe();
        public ChargeBlade ChargeBlade = new ChargeBlade();
        public InsectGlaive InsectGlaive = new InsectGlaive();
        public Bow Bow = new Bow();
        public LightBowgun LightBowgun = new LightBowgun();
        public HeavyBowgun HeavyBowgun = new HeavyBowgun();
        #endregion

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
        public event PlayerEvents OnClassChange;

        private void Dispatch(PlayerEvents e) => e?.Invoke(this, EventArgs.Empty);
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
            long PlayerGearBase = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerGearOffsets);

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

        private GameStructs.BowgunMod[] GetBowgunMods(long BaseAddress)
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

        private GameStructs.NewAugment[] GetWeaponNewAugments(long BaseAddress)
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

        private GameStructs.AwakenedSkill[] GetWeaponAwakenedSkills(long BaseAddress)
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

        private GameStructs.CustomAugment[] GetCustomAugments(long BaseAddress)
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

        private GameStructs.Decoration[] GetDecorationsFromGear(long BaseAddress, int GearIndex)
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

        private GameStructs.Decoration[] GetWeaponDecorations(long BaseAddress)
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

        private GameStructs.Augment[] GetWeaponAugments(long BaseAddress)
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

        private GameStructs.Decoration[] GetMantleDecorations(long BaseAddress)
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

        public sItem[] GetDecorationsFromStorage()
        {
            // We have up to 500 different slots in our decoration storage box
            sItem[] decorations = new sItem[500];

            for (long sStart = 0; sStart < 0x10 * 500; sStart += 0x10)
            {
                decorations[sStart / 0x10] = Scanner.Win32.Read<sItem>(PlayerAddress + 0x3F098 + sStart);
            }

            return decorations;
        }


        public sGear[] GetGearFromStorage()
        {
            // We have up to 2509 different slots in our storage box
            // And 127 in the mantle box?
            List<sGear> gear = new List<sGear>();

            for (long sStart = 0; sStart < 0x98 * 2509; sStart += 0x98)
            {
                gear.Add(Scanner.Win32.Read<sGear>(LEVEL_ADDRESS + 0x40FD8 + sStart));
            }

            for (long sStart = 0; sStart < 0x98 * 127; sStart += 0x98)
            {
                gear.Add(Scanner.Win32.Read<sGear>(LEVEL_ADDRESS + 0xE9258 + sStart));
            }

            return gear.ToArray();
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
                    GetPlayerPlaytime();
                    GetWeaponId();
                    GetPlayerBasicInfo();
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
                    GetPlayerPosition();
                }
                GetSessionId();
                GetEquipmentAddress();
                Thread.Sleep(UserSettings.PlayerConfig.Overlay.GameScanDelay);
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private bool GetPlayerAddress()
        {
            if (ZoneID == 0)
            {
                PlayerAddress = 0;
                LEVEL_ADDRESS = 0;
                return false;
            }
            long FirstSaveAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.LEVEL_OFFSET, Address.Offsets.LevelOffsets);
            uint CurrentSaveSlot = Scanner.Read<uint>(FirstSaveAddress + 0x44);
            long NextPlayerSave = 0x27E9F0;
            long CurrentPlayerSaveHeader = Scanner.Read<long>(FirstSaveAddress) + NextPlayerSave * CurrentSaveSlot;

            if (CurrentPlayerSaveHeader != PlayerAddress)
            {
                LEVEL_ADDRESS = CurrentPlayerSaveHeader + 0x90;
                GetPlayerLevel();
                GetPlayerMasterRank();
                GetPlayerName();
                PlayerAddress = CurrentPlayerSaveHeader;
            }
            return true;
        }

        private void GetPlayerLevel() => Level = Scanner.Read<int>(LEVEL_ADDRESS);

        private void GetPlayerMasterRank() => MasterRank = Scanner.Read<int>(LEVEL_ADDRESS + 0x44);

        private void GetPlayerName()
        {
            long Address = LEVEL_ADDRESS - 0x40;
            Name = Scanner.READ_STRING(Address, 32);
        }

        private void GetPlayerPlaytime() => PlayTime = Scanner.Read<int>(LEVEL_ADDRESS + 0x10);

        private void GetPlayerPosition()
        {
            long address = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerPositionOffsets);
            sVector3 vector3 = Scanner.Win32.Read<sVector3>(address);
            Position.Update(vector3);
        }

        private void GetZoneId()
        {
            long ZoneAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ZONE_OFFSET, Address.Offsets.ZoneOffsets);
            int zoneId = Scanner.Read<int>(ZoneAddress);
            if (zoneId != ZoneID)
            {
                LastZoneID = ZoneID;
                ZoneID = zoneId;
            }
        }

        public void ChangeLastZone() => LastZoneID = ZoneID;

        private void GetPlayerBasicInfo()
        {
            long address = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerBasicInformationOffsets);

            MaxHealth = Scanner.Read<float>(address + 0x60);
            Health = Scanner.Read<float>(address + 0x64);

            MaxStamina = Scanner.Read<float>(address + 0x144);
            Stamina = Scanner.Read<float>(address + 0x13C);

            // Hacky way to update the eat timer
            if (!InHarvestZone)
            {
                long EatTimerAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.CANTEEN_OFFSET, Address.Offsets.PlayerCanteenTimer);
                UpdateAbnormality(AbnormalityData.MiscAbnormalities.Where(a => a.Id == 999).FirstOrDefault(), EatTimerAddress);
            }
                
        }

        private void GetWeaponId()
        {
            long Address = Memory.Address.BASE + Memory.Address.WEAPON_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.WeaponOffsets);
            PlayerStructAddress = Address;
            WeaponID = Scanner.Read<byte>(Address);
        }

        private void GetSessionId()
        {
            long Address = Memory.Address.BASE + Memory.Address.SESSION_OFFSET;
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
            long Address = Memory.Address.BASE + Memory.Address.EQUIPMENT_OFFSET;
            Address = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.EquipmentOffsets);
            if (EQUIPMENT_ADDRESS != Address) Debugger.Debug($"New equipment address found -> 0x{Address:X}");
            EQUIPMENT_ADDRESS = Address;
        }

        private void GetPrimaryMantle()
        {
            long Address = PlayerStructAddress + 0x34;
            int mantleId = Scanner.Read<int>(Address);
            PrimaryMantle.SetID(mantleId);
        }

        private void GetSecondaryMantle()
        {
            long Address = PlayerStructAddress + 0x34 + 0x4;
            int mantleId = Scanner.Read<int>(Address);
            SecondaryMantle.SetID(mantleId);
        }

        private void GetPrimaryMantleTimers()
        {
            long PrimaryMantleTimerFixed = (PrimaryMantle.ID * 4) + Address.timerFixed;
            long PrimaryMantleTimer = (PrimaryMantle.ID * 4) + Address.timerDynamic;
            long PrimaryMantleCdFixed = (PrimaryMantle.ID * 4) + Address.cooldownFixed;
            long PrimaryMantleCdDynamic = (PrimaryMantle.ID * 4) + Address.cooldownDynamic;
            PrimaryMantle.SetCooldown(Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleCdDynamic), Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleCdFixed));
            PrimaryMantle.SetTimer(Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleTimer), Scanner.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleTimerFixed));
        }

        private void GetSecondaryMantleTimers()
        {
            long SecondaryMantleTimerFixed = (SecondaryMantle.ID * 4) + Address.timerFixed;
            long SecondaryMantleTimer = (SecondaryMantle.ID * 4) + Address.timerDynamic;
            long SecondaryMantleCdFixed = (SecondaryMantle.ID * 4) + Address.cooldownFixed;
            long SecondaryMantleCdDynamic = (SecondaryMantle.ID * 4) + Address.cooldownDynamic;
            SecondaryMantle.SetCooldown(Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleCdDynamic), Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleCdFixed));
            SecondaryMantle.SetTimer(Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleTimer), Scanner.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleTimerFixed));
        }

        private void GetParty()
        {

            long address = Address.BASE + Address.PARTY_OFFSET;
            long PartyContainer = Scanner.READ_MULTILEVEL_PTR(address, Address.Offsets.PartyOffsets) - 0x22B7;
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
            long TimerAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            float Timer = Scanner.Read<float>(TimerAddress + 0xBC8);
            PlayerParty.ShowDPS = true;
            if (Timer > 0)
            {
                PlayerParty.Epoch = TimeSpan.FromSeconds(Timer);
            }
            else { PlayerParty.Epoch = TimeSpan.Zero; }
        }

        private int GetPartyMemberDamage(int playerIndex)
        {
            long DPSAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.DAMAGE_OFFSET, Address.Offsets.DamageOffsets);
            return Scanner.Read<int>(DPSAddress + (0x2A0 * playerIndex));
        }

        private string GetPartyMemberName(long NameAddress)
        {
            string PartyMemberName = Scanner.READ_STRING(NameAddress, 32);
            return PartyMemberName ?? PartyMemberName.Trim('\x00');
        }

        private void GetFertilizers()
        {
            long Address = LEVEL_ADDRESS;
            for (int fertCount = 0; fertCount < 4; fertCount++)
            {
                // Calculates memory address
                long FertilizerAddress = Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * fertCount) - 0xC;
                sItem element = Scanner.Win32.Read<sItem>(FertilizerAddress);
                // Read memory
                int FertilizerId = element.ItemId;
                int FertilizerCount = element.Amount;
                // update fertilizer data
                Harvest.Box[fertCount].ID = FertilizerId;
                Harvest.Box[fertCount].Amount = FertilizerCount;
            }
            UpdateHarvestBoxCounter(Address + Memory.Address.Offsets.FertilizersOffset + (0x10 * 3) - 0xC);
        }

        private void UpdateHarvestBoxCounter(long LastFertAddress)
        {
            long Address = LastFertAddress + Memory.Address.Offsets.HarvestBoxOffset;
            int counter = 0;
            for (long iAddress = Address; iAddress < Address + 0x320; iAddress += 0x10)
            {
                sItem element = Scanner.Win32.Read<sItem>(iAddress);
                if (element.Amount > 0)
                {
                    counter++;
                }
            }
            Harvest.Counter = counter;
        }

        private void GetSteamFuel()
        {
            long NaturalFuelAddress = LEVEL_ADDRESS + Address.Offsets.SteamFuelOffset;
            Activity.NaturalFuel = Scanner.Read<int>(NaturalFuelAddress);
            Activity.StoredFuel = Scanner.Read<int>(NaturalFuelAddress + 0x4);
        }

        private void GetArgosyData()
        {
            long ArgosyDaysAddress = LEVEL_ADDRESS + Address.Offsets.ArgosyOffset;
            byte ArgosyDays = Scanner.Read<byte>(ArgosyDaysAddress);
            bool ArgosyInTown = ArgosyDays < 250;
            if (ArgosyDays >= 250) { ArgosyDays = (byte)(byte.MaxValue - ArgosyDays + 1); }
            Activity.SetArgosyInfo(ArgosyDays, ArgosyInTown);
        }

        private void GetTailraidersData()
        {
            long TailraidersDaysAddress = LEVEL_ADDRESS + Address.Offsets.TailRaidersOffset;
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

            foreach (AbnormalityInfo abnormality in AbnormalityData.MiscAbnormalities.Where(ab => ab.Id != 999))
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
            long AbnormAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            bool HasSafiBuff = Scanner.Read<int>(AbnormAddress + 0x9A8) >= 1;
            int SafiCounter = HasSafiBuff ? Scanner.Read<int>(AbnormAddress + 0x7A8) : -1;
            long weaponAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.WEAPON_MECHANICS_OFFSET, Address.Offsets.WeaponMechanicsOffsets);
            ClassAddress = weaponAddress;
            switch ((Classes)WeaponID)
            {
                case Classes.Greatsword:
                    GetGreatswordInformation(weaponAddress);
                    Greatsword.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.SwordAndShield:
                    break;
                case Classes.DualBlades:
                    GetDualBladesInformation(weaponAddress);
                    DualBlades.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.LongSword:
                    GetLongswordInformation(weaponAddress);
                    Longsword.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.Hammer:
                    GetHammerInformation(weaponAddress);
                    Hammer.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.Lance:
                    Lance.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.GunLance:
                    GetGunLanceInformation(weaponAddress, AbnormAddress);
                    GunLance.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.SwitchAxe:
                    GetSwitchAxeInformation(weaponAddress, AbnormAddress);
                    SwitchAxe.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.ChargeBlade:
                    GetChargeBladeInformation(weaponAddress);
                    ChargeBlade.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.InsectGlaive:
                    GetInsectGlaiveInformation(weaponAddress);
                    InsectGlaive.SafijiivaRegenCounter = SafiCounter;
                    break;
                case Classes.Bow:
                    GetBowInformation(weaponAddress);
                    Bow.SafijiivaRegenCounter = SafiCounter;
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
            bool inDemonMode = Scanner.Read<byte>(weaponAddress - 0x4) == 1;
            bool isReducing = Scanner.Read<byte>(weaponAddress - 0x3) == 1;
            float demonGauge = Scanner.Read<float>(weaponAddress);
            DualBlades.InDemonMode = inDemonMode;
            DualBlades.DemonGauge = demonGauge;
            DualBlades.IsReducing = isReducing;
        }

        private void GetLongswordInformation(long weaponAddress)
        {
            float gauge = Scanner.Read<float>(weaponAddress - 0x4);
            int chargeLevel = Scanner.Read<int>(weaponAddress + 0x4);
            float chargeGauge = Scanner.Read<float>(weaponAddress + 0x8);
            float spiritGaugeBlink = Math.Max(Scanner.Read<float>(weaponAddress + 0xC), Scanner.Read<float>(weaponAddress + 0x1C));
            Longsword.InnerGauge = gauge;
            Longsword.ChargeLevel = chargeLevel;
            Longsword.OuterGauge = chargeGauge;
            Longsword.SpiritGaugeBlinkDuration = spiritGaugeBlink;
        }

        private void GetHammerInformation(long weaponAddress)
        {
            bool isPowerCharged = Scanner.Read<byte>(weaponAddress - 0x18) == 1;
            int chargeLevel = Scanner.Read<int>(weaponAddress - 0x10);
            float chargeProgress = Scanner.Read<float>(weaponAddress - 0x14);
            bool isSheathed = Scanner.Read<byte>(weaponAddress - 0x18CB) == 0;
            Hammer.IsPowerCharged = isPowerCharged;
            Hammer.ChargeLevel = chargeLevel;
            Hammer.ChargeProgress = chargeProgress;
            Hammer.IsWeaponSheated = isSheathed;
        }

        private void GetGunLanceInformation(long weaponAddress, long AbnormAddress)
        {
            long AbnormalitiesAddress = AbnormAddress;
            int totalAmmo = Scanner.Read<int>(weaponAddress - 0x4);
            int currentAmmo = Scanner.Read<int>(weaponAddress);
            int totalBigAmmo = Scanner.Read<int>(weaponAddress + 0x10);
            int currentBigAmmo = Scanner.Read<int>(weaponAddress + 0xC);
            float wyvernsfire = Scanner.Read<float>(AbnormalitiesAddress + 0xBC0);
            bool hasFirestakeLoaded = Scanner.Read<float>(weaponAddress + 0xBC) != 0f;
            float wyvernstakeMax = Scanner.Read<float>(weaponAddress + 0xC0);
            // Check if the Firestake timer ptr is 0
            long wyvernstakeTimerPtr = Scanner.Read<long>(weaponAddress + 0x204);
            float wyvernstakeTimer = 0;
            if (wyvernstakeTimerPtr != 0x00000000)
            {
                wyvernstakeTimer = Scanner.Read<float>(wyvernstakeTimerPtr + 0xE20);
            }
            GunLance.TotalAmmo = totalAmmo;
            GunLance.Ammo = currentAmmo;
            GunLance.TotalBigAmmo = totalBigAmmo;
            GunLance.BigAmmo = currentBigAmmo;
            GunLance.WyvernstakeNextMax = hasFirestakeLoaded ? Scanner.Read<float>(weaponAddress + 0xBC) : wyvernstakeMax;
            GunLance.WyvernstakeMax = wyvernstakeMax;
            GunLance.WyvernsFireTimer = wyvernsfire;
            GunLance.HasWyvernstakeLoaded = hasFirestakeLoaded;
            GunLance.WyvernstakeBlastTimer = wyvernstakeTimer;
        }

        private void GetSwitchAxeInformation(long weaponAddress, long buffAddress)
        {
            float outerGauge = Scanner.Read<float>(weaponAddress - 0xC);
            float swordChargeTimer = Scanner.Read<float>(weaponAddress - 0x8);
            float innerGauge = Scanner.Read<float>(weaponAddress - 0x1C);
            float switchAxeBuff = 0;
            bool isAxeBuffActive = Scanner.Read<byte>(buffAddress + 0x6E5) == 1;
            if (isAxeBuffActive)
            {
                switchAxeBuff = Scanner.Read<float>(buffAddress + 0x6E8);
            }
            SwitchAxe.OuterGauge = outerGauge;
            SwitchAxe.SwordChargeMaxTimer = swordChargeTimer > SwitchAxe.SwordChargeMaxTimer || swordChargeTimer <= 0 ? swordChargeTimer : SwitchAxe.SwordChargeMaxTimer;
            SwitchAxe.SwordChargeTimer = swordChargeTimer;
            SwitchAxe.InnerGauge = innerGauge;
            SwitchAxe.IsBuffActive = isAxeBuffActive;
            SwitchAxe.SwitchAxeBuffTimer = switchAxeBuff;
        }

        private void GetChargeBladeInformation(long weaponAddress)
        {
            float hiddenGauge = Scanner.Read<float>(weaponAddress + 0x4);
            int vialsAmount = Scanner.Read<int>(weaponAddress + 0x8);
            float swordBuff = Scanner.Read<float>(weaponAddress + 0x10);
            float shieldBuff = Scanner.Read<float>(weaponAddress + 0xC);
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

            // Insect Glaive has some dumb bugs sometimes where the buffs duration are either negative
            // or NaN
            redBuff = float.IsNaN(redBuff) ? 0 : redBuff;
            whiteBuff = float.IsNaN(whiteBuff) ? 0 : whiteBuff;
            orangeBuff = float.IsNaN(orangeBuff) ? 0 : orangeBuff;

            // For whatever reason, some IGs split their data between two IG structures I suppose?
            // So we can use this pointer that will always point to the right data
            long dataPtr = Scanner.Read<long>(weaponAddress - 0x236C - 0x28);
            float redChargeTimer = Scanner.Read<float>(dataPtr + 0x1BE8);
            float yellowChargeTimer = Scanner.Read<float>(dataPtr + 0x1BEC);
            float kinsectStamina = Scanner.Read<float>(dataPtr + 0xACC);
            KinsectChargeBuff chargeFlag = redChargeTimer > 0 && yellowChargeTimer > 0 ? KinsectChargeBuff.Both :
                redChargeTimer > 0 ? KinsectChargeBuff.Red :
                yellowChargeTimer > 0 ? KinsectChargeBuff.Yellow : KinsectChargeBuff.None;
            int kinsectBuffQueueSize = Scanner.Read<int>(weaponAddress + 0x24);
            InsectGlaive.RedBuff = redBuff;
            InsectGlaive.WhiteBuff = whiteBuff;
            InsectGlaive.OrangeBuff = orangeBuff;
            InsectGlaive.KinsectChargeType = chargeFlag;
            InsectGlaive.RedKinsectTimer = redChargeTimer;
            InsectGlaive.YellowKinsectTimer = yellowChargeTimer;
            InsectGlaive.BuffQueueSize = kinsectBuffQueueSize;
            InsectGlaive.KinsectStamina = kinsectStamina;
            if (kinsectBuffQueueSize > 0)
            {
                List<int> BuffQueue = new List<int>();
                for (int i = 0; i < 3; i++)
                {
                    BuffQueue.Add(Scanner.Read<int>(weaponAddress + 0xC + (0x4 * i)));
                }
                int BuffQueuePtr = Scanner.Read<int>(weaponAddress + 0x1C);
                InsectGlaive.FirstBuffQueued = BuffQueue.ElementAtOrDefault(BuffQueuePtr);
                InsectGlaive.SecondBuffQueued = BuffQueue.ElementAtOrDefault(BuffQueuePtr + 1 > 2 ? 0 : BuffQueuePtr + 1);
            }
        }

        private void GetBowInformation(long weaponAddress)
        {
            float chargeProgress = Scanner.Read<float>(weaponAddress + 0x44);
            int chargeLevel = Scanner.Read<int>(weaponAddress + 0x48);
            int mChargeLevel = Scanner.Read<int>(weaponAddress + 0x4C);
            bool isSheathed = Scanner.Read<byte>(weaponAddress - 0x18CB) == 0;
            Bow.MaxChargeLevel = mChargeLevel;
            Bow.ChargeLevel = chargeLevel;
            Bow.ChargeProgress = chargeProgress;
            Bow.IsWeaponSheated = isSheathed;
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
