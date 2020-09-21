using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HunterPie.Core.Definitions;
using HunterPie.Core.Jobs;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Core.Local;
using HunterPie.Core.Events;
using Classes = HunterPie.Core.Enums.Classes;
using AbnormalityType = HunterPie.Core.Enums.AbnormalityType;
using HunterPie.Core.Enums;

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
        private int actionId;

        private readonly int[] harvestBoxZones =
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
        private readonly int[] peaceZones =
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
            private set
            {
                if (playerAddress != value)
                {
                    playerAddress = value;
                    if (value != 0x0)
                    {
                        Debugger.Debug($"Found player address -> {value:X}");
                        Dispatch(OnCharacterLogin, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Player Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Player High rank
        /// </summary>
        public int Level
        {
            get => level;
            private set
            {
                if (level != value)
                {
                    level = value;
                    Dispatch(OnLevelChange, new PlayerEventArgs(this));
                }
            }
        }

        /// <summary>
        /// Player master rank
        /// </summary>
        public int MasterRank { get; private set; }

        /// <summary>
        /// Player playtime in seconds
        /// </summary>
        public int PlayTime { get; private set; }

        /// <summary>
        /// Whether this player is logged on or not
        /// </summary>
        public bool IsLoggedOn => playerAddress != 0;

        /// <summary>
        /// Player Weapon Id
        /// </summary>
        public byte WeaponID
        {
            get => weaponId;
            private set
            {
                if (weaponId != value)
                {
                    weaponId = value;
                    Dispatch(OnWeaponChange, new PlayerEventArgs(this));
                }
            }
        }

        /// <summary>
        /// Player weapon name, in HunterPie's current localization
        /// </summary>
        public string WeaponName => GStrings.GetWeaponNameByID(WeaponID);
        public long ClassAddress
        {
            get => classAddress;
            private set
            {
                if (value != classAddress)
                {
                    Debugger.Debug($"{value:X}");
                    classAddress = value;
                    Dispatch(OnClassChange, new PlayerEventArgs(this));
                }
            }
        }

        /// <summary>
        /// Player gear skill list
        /// </summary>
        public sPlayerSkill[] Skills;

        /// <summary>
        /// Current Zone Id
        /// </summary>
        public int ZoneID
        {
            get => zoneId;
            private set
            {
                if (zoneId != value)
                {
                    if ((zoneId == -1 || peaceZones.Contains(zoneId)) && !peaceZones.Contains(value)) Dispatch(OnPeaceZoneLeave, new PlayerLocationEventArgs(this));
                    if (harvestBoxZones.Contains(zoneId) && !harvestBoxZones.Contains(value)) Dispatch(OnVillageLeave, new PlayerLocationEventArgs(this));
                    zoneId = value;
                    Dispatch(OnZoneChange, new PlayerLocationEventArgs(this));
                    if (peaceZones.Contains(value)) Dispatch(OnPeaceZoneEnter, new PlayerLocationEventArgs(this));
                    if (harvestBoxZones.Contains(value)) Dispatch(OnVillageEnter, new PlayerLocationEventArgs(this));
                    if (value == 0 && LEVEL_ADDRESS != 0x0)
                    {
                        LEVEL_ADDRESS = 0x0;
                        PlayerAddress = 0x0;
                        Dispatch(OnCharacterLogout, EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Current zone name, it uses HunterPie's localization
        /// </summary>
        public string ZoneName => GStrings.GetStageNameByID(ZoneID);

        /// <summary>
        /// Last Zone Id
        /// </summary>
        public int LastZoneID { get; private set; }

        /// <summary>
        /// Whether the player is in a peace zone or not. A zone is considered a peace zone when the player cannot use weapons in there.
        /// </summary>
        public bool InPeaceZone => peaceZones.Contains(ZoneID);

        /// <summary>
        /// Whether the player is in a zone where the Harvest Box can be accessed
        /// </summary>
        public bool InHarvestZone => harvestBoxZones.Contains(ZoneID);

        /// <summary>
        /// Current Session Id
        /// </summary>
        public string SessionID
        {
            get => sessionId;
            private set
            {
                if (sessionId != value)
                {
                    sessionId = value;
                    GetSteamSession();
                    Dispatch(OnSessionChange, new PlayerEventArgs(this));
                }
            }
        }

        /// <summary>
        /// Current Steam Session Id
        /// </summary>
        public long SteamSession { get; private set; }

        /// <summary>
        /// Client's account steam Id
        /// </summary>
        public long SteamID { get; private set; }

        /// <summary>
        /// Player health
        /// </summary>
        public float Health { get; private set; }

        /// <summary>
        /// Player maximum health
        /// </summary>
        public float MaxHealth { get; private set; }

        /// <summary>
        /// Player stamina
        /// </summary>
        public float Stamina { get; private set; }

        /// <summary>
        /// Player maximum stamina
        /// </summary>
        public float MaxStamina { get; private set; }

        /// <summary>
        /// Player action id
        /// </summary>
        public int ActionId
        {
            get => actionId;
            set
            {
                if (value != actionId)
                {
                    actionId = value;
                    Debugger.Debug($"Current Action ID: {value}");
                }
            }
        }

        /// <summary>
        /// Player position
        /// </summary>
        public readonly Vector3 Position = new Vector3();

        /// <summary>
        /// Player item pouch
        /// </summary>
        public readonly Inventory Inventory = new Inventory();

        /// <summary>
        /// Player current party
        /// </summary>
        public readonly Party PlayerParty = new Party();

        /// <summary>
        /// Player harvest box
        /// </summary>
        public readonly HarvestBox Harvest = new HarvestBox();

        /// <summary>
        /// Argosy, Tailraiders and Steam Fuel
        /// </summary>
        public readonly Activities Activity = new Activities();

        /// <summary>
        /// Current primary mantle
        /// </summary>
        public readonly Mantle PrimaryMantle = new Mantle();

        /// <summary>
        /// Current secondary mantle
        /// </summary>
        public readonly Mantle SecondaryMantle = new Mantle();

        /// <summary>
        /// Player abnormalities
        /// </summary>
        public readonly Abnormalities Abnormalities = new Abnormalities();

        #region Jobs
        public readonly Greatsword Greatsword = new Greatsword();
        public readonly DualBlades DualBlades = new DualBlades();
        public readonly Longsword Longsword = new Longsword();
        public readonly Hammer Hammer = new Hammer();
        public readonly HuntingHorn HuntingHorn = new HuntingHorn();
        public readonly Lance Lance = new Lance();
        public readonly GunLance GunLance = new GunLance();
        public readonly SwitchAxe SwitchAxe = new SwitchAxe();
        public readonly ChargeBlade ChargeBlade = new ChargeBlade();
        public readonly InsectGlaive InsectGlaive = new InsectGlaive();
        public readonly Bow Bow = new Bow();
        public readonly LightBowgun LightBowgun = new LightBowgun();
        public readonly HeavyBowgun HeavyBowgun = new HeavyBowgun();
        #endregion

        // Threading
        private ThreadStart scanPlayerInfoRef;
        private Thread scanPlayerInfo;

        #region Events
        // Event handlers

        public delegate void PlayerEvents(object source, EventArgs args);

        public event PlayerEvents OnLevelChange;
        public event PlayerEvents OnWeaponChange;
        public event PlayerEvents OnSessionChange;
        public event PlayerEvents OnClassChange;

        public event PlayerEvents OnCharacterLogin;
        public event PlayerEvents OnCharacterLogout;

        public event PlayerEvents OnZoneChange;
        public event PlayerEvents OnPeaceZoneEnter;
        public event PlayerEvents OnVillageEnter;
        public event PlayerEvents OnPeaceZoneLeave;
        public event PlayerEvents OnVillageLeave;

        private void Dispatch(PlayerEvents e, EventArgs args) => e?.Invoke(this, args);
        #endregion

        #region Scanner
        public void StartScanning()
        {
            scanPlayerInfoRef = new ThreadStart(GetPlayerInfo);
            scanPlayerInfo = new Thread(scanPlayerInfoRef)
            {
                Name = "Scanner_Player"
            };
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLAYER_SCANNER_INITIALIZED']"));
            scanPlayerInfo.Start();
        }

        public void StopScanning() => scanPlayerInfo.Abort();
        #endregion

        #region Manual Player Data
        /*
            Player data that needs to be called by an external function and doesn't need to keep track of this data
            every second.
        */
        public GameStructs.Gear GetPlayerGear()
        {
            long PlayerGearBase = Kernel.ReadMultilevelPtr(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerGearOffsets);

            // Helm
            GameStructs.Armor Helm = new GameStructs.Armor()
            {
                ID = Kernel.Read<int>(PlayerGearBase),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 0)
            };

            // Chest
            GameStructs.Armor Chest = new GameStructs.Armor()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0x4),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 1)
            };

            // Arms
            GameStructs.Armor Arms = new GameStructs.Armor()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0x8),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 2)
            };

            // Waist
            GameStructs.Armor Waist = new GameStructs.Armor()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0xC),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 3)
            };

            // Waist
            GameStructs.Armor Legs = new GameStructs.Armor()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0x10),
                Decorations = GetDecorationsFromGear(PlayerGearBase, 4)
            };

            // Charm
            GameStructs.Charm Charm = new GameStructs.Charm()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0x14)
            };

            // Weapon
            GameStructs.Weapon Weapon = new GameStructs.Weapon()
            {
                Type = Kernel.Read<int>(PlayerGearBase + 0x124),
                ID = Kernel.Read<int>(PlayerGearBase + 0x128),
                Decorations = GetWeaponDecorations(PlayerGearBase + 0x128),
                NewAugments = GetWeaponNewAugments(PlayerGearBase + 0x128),
                Awakenings = GetWeaponAwakenedSkills(PlayerGearBase + 0x128),
                CustomAugments = GetCustomAugments(PlayerGearBase + 0x128),
                BowgunMods = GetBowgunMods(PlayerGearBase + 0x128)
            };

            // Primary Tool
            GameStructs.SpecializedTool PrimaryTool = new GameStructs.SpecializedTool()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0x158),
                Decorations = GetMantleDecorations(PlayerGearBase + 0x164)
            };

            // Secondary Tool
            GameStructs.SpecializedTool SecondaryTool = new GameStructs.SpecializedTool()
            {
                ID = Kernel.Read<int>(PlayerGearBase + 0x15C),
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
                    ID = GameStructs.ConvertToMax(Kernel.Read<uint>(BaseAddress + 0x10 + (i * 4)))
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
                    Level = Kernel.Read<byte>(BaseAddress + 0x84 + AugmentIndex)
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
                    ID = Kernel.Read<short>(BaseAddress + 0x8C + (AwakIndex * sizeof(short)))
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
                    ID = Kernel.Read<byte>(BaseAddress + 0x78 + AugIndex),
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
                    ID = GameStructs.ConvertToMax(Kernel.Read<uint>(BaseAddress + 0x30 + (0x3 * GearIndex * 0x4) + (0x4 * DecorationIndex)))
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
                    ID = GameStructs.ConvertToMax(Kernel.Read<uint>(BaseAddress + ((DecorationIndex + 1) * 0x4)))
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
                    ID = Kernel.Read<int>(BaseAddress + 0x24 + (AugmentIndex * 0x4))
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
                    ID = GameStructs.ConvertToMax(Kernel.Read<uint>(BaseAddress + (DecorationIndex * 0x4)))
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
                decorations[sStart / 0x10] = Kernel.ReadStructure<sItem>(PlayerAddress + 0x3F098 + sStart);
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
                gear.Add(Kernel.ReadStructure<sGear>(LEVEL_ADDRESS + 0x40FD8 + sStart));
            }

            for (long sStart = 0; sStart < 0x98 * 127; sStart += 0x98)
            {
                gear.Add(Kernel.ReadStructure<sGear>(LEVEL_ADDRESS + 0xE9258 + sStart));
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
            while (Kernel.GameIsRunning)
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
                    GetPlayerInventory();
                    GetFertilizers();
                    GetArgosyData();
                    GetTailraidersData();
                    GetSteamFuel();
                    GetPrimaryMantle();
                    GetSecondaryMantle();
                    GetPrimaryMantleTimers();
                    GetSecondaryMantleTimers();
                    GetPlayerSkills();
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
            long FirstSaveAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.LEVEL_OFFSET, Address.Offsets.LevelOffsets);
            uint CurrentSaveSlot = Kernel.Read<uint>(FirstSaveAddress + 0x44);
            long NextPlayerSave = 0x27E9F0;
            long CurrentPlayerSaveHeader = Kernel.Read<long>(FirstSaveAddress) + NextPlayerSave * CurrentSaveSlot;

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

        private void GetPlayerLevel() => Level = Kernel.Read<int>(LEVEL_ADDRESS);

        private void GetPlayerMasterRank() => MasterRank = Kernel.Read<int>(LEVEL_ADDRESS + 0x44);

        private void GetPlayerName()
        {
            long Address = LEVEL_ADDRESS - 0x40;
            Name = Kernel.ReadString(Address, 32);
        }

        private void GetPlayerPlaytime() => PlayTime = Kernel.Read<int>(LEVEL_ADDRESS + 0x10);

        private void GetPlayerInventory()
        {            
            sItem[] inventoryItems = Kernel.ReadStructure<sItem>(PlayerAddress + 0x38080, 40);
            Inventory.RefreshPouch(inventoryItems);
        }

        private void GetPlayerPosition()
        {
            long address = Kernel.ReadMultilevelPtr(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerPositionOffsets);
            sVector3 vector3 = Kernel.ReadStructure<sVector3>(address);
            Position.Update(vector3);
        }

        private void GetZoneId()
        {
            long ZoneAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.ZONE_OFFSET, Address.Offsets.ZoneOffsets);
            int zoneId = Kernel.Read<int>(ZoneAddress);
            if (zoneId != ZoneID)
            {
                LastZoneID = ZoneID;
                ZoneID = zoneId;
            }
        }

        public void ChangeLastZone() => LastZoneID = ZoneID;

        private void GetPlayerBasicInfo()
        {
            long address = Kernel.ReadMultilevelPtr(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerBasicInformationOffsets);

            MaxHealth = Kernel.Read<float>(address + 0x60);
            Health = Kernel.Read<float>(address + 0x64);

            MaxStamina = Kernel.Read<float>(address + 0x144);
            Stamina = Kernel.Read<float>(address + 0x13C);

            ActionId = Kernel.Read<int>(address - 0x4B28);

            // Hacky way to update the eat timer
            if (!InHarvestZone)
            {
                long EatTimerAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.CANTEEN_OFFSET, Address.Offsets.PlayerCanteenTimer);
                UpdateAbnormality(AbnormalityData.MiscAbnormalities.Where(a => a.Id == 999).FirstOrDefault(), EatTimerAddress);
            }
                
        }

        private void GetWeaponId()
        {
            long Address = Memory.Address.BASE + Memory.Address.WEAPON_OFFSET;
            Address = Kernel.ReadMultilevelPtr(Address, Memory.Address.Offsets.WeaponOffsets);
            PlayerStructAddress = Address;
            WeaponID = Kernel.Read<byte>(Address);
        }

        private void GetSessionId()
        {
            long Address = Memory.Address.BASE + Memory.Address.SESSION_OFFSET;
            Address = Kernel.ReadMultilevelPtr(Address, Memory.Address.Offsets.SessionOffsets);
            SESSION_ADDRESS = Address;
            SessionID = Kernel.ReadString(SESSION_ADDRESS, 12);
        }

        private void GetSteamSession()
        {
            SteamSession = Kernel.Read<long>(SESSION_ADDRESS + 0x10);
            SteamID = Kernel.Read<long>(SESSION_ADDRESS + 0x1184);
            Debugger.Debug($"Steam Session: {SteamSession}/{SteamID}");
        }

        private void GetEquipmentAddress()
        {
            long Address = Memory.Address.BASE + Memory.Address.EQUIPMENT_OFFSET;
            Address = Kernel.ReadMultilevelPtr(Address, Memory.Address.Offsets.EquipmentOffsets);
            if (EQUIPMENT_ADDRESS != Address) Debugger.Debug($"New equipment address found -> 0x{Address:X}");
            EQUIPMENT_ADDRESS = Address;
        }

        private void GetPrimaryMantle()
        {
            long Address = PlayerStructAddress + 0x34;
            int mantleId = Kernel.Read<int>(Address);
            PrimaryMantle.SetID(mantleId);
        }

        private void GetSecondaryMantle()
        {
            long Address = PlayerStructAddress + 0x34 + 0x4;
            int mantleId = Kernel.Read<int>(Address);
            SecondaryMantle.SetID(mantleId);
        }

        private void GetPrimaryMantleTimers()
        {
            long PrimaryMantleTimerFixed = (PrimaryMantle.ID * 4) + Address.TimerFixed;
            long PrimaryMantleTimer = (PrimaryMantle.ID * 4) + Address.TimerDynamic;
            long PrimaryMantleCdFixed = (PrimaryMantle.ID * 4) + Address.CooldownFixed;
            long PrimaryMantleCdDynamic = (PrimaryMantle.ID * 4) + Address.CooldownDynamic;
            PrimaryMantle.SetCooldown(Kernel.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleCdDynamic), Kernel.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleCdFixed));
            PrimaryMantle.SetTimer(Kernel.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleTimer), Kernel.Read<float>(EQUIPMENT_ADDRESS + PrimaryMantleTimerFixed));
        }

        private void GetSecondaryMantleTimers()
        {
            long SecondaryMantleTimerFixed = (SecondaryMantle.ID * 4) + Address.TimerFixed;
            long SecondaryMantleTimer = (SecondaryMantle.ID * 4) + Address.TimerDynamic;
            long SecondaryMantleCdFixed = (SecondaryMantle.ID * 4) + Address.CooldownFixed;
            long SecondaryMantleCdDynamic = (SecondaryMantle.ID * 4) + Address.CooldownDynamic;
            SecondaryMantle.SetCooldown(Kernel.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleCdDynamic), Kernel.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleCdFixed));
            SecondaryMantle.SetTimer(Kernel.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleTimer), Kernel.Read<float>(EQUIPMENT_ADDRESS + SecondaryMantleTimerFixed));
        }

        private void GetPlayerSkills()
        {
            long address = Kernel.ReadMultilevelPtr(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.SkillOffsets);
            Skills = Kernel.ReadStructure<sPlayerSkill>(address, 226);
        }

        private void GetParty()
        {

            long address = Address.BASE + Address.PARTY_OFFSET;
            long PartyContainer = Kernel.ReadMultilevelPtr(address, Address.Offsets.PartyOffsets) - 0x22B7;
            if (InPeaceZone)
            {
                PlayerParty.LobbySize = Kernel.Read<int>(PartyContainer - 0xA961);
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
                    short HR = Kernel.Read<short>(PartyContainer + (i * 0x1C0 + 0x27));
                    short MR = Kernel.Read<short>(PartyContainer + (i * 0x1C0 + 0x29));
                    byte playerWeapon = playerName == Name && HR == Level ? WeaponID : Kernel.Read<byte>(PartyContainer + (i * 0x1C0 + 0x33));
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
            long TimerAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            float Timer = Kernel.Read<float>(TimerAddress + 0xBC8);
            PlayerParty.ShowDPS = true;
            if (Timer > 0)
            {
                float multiplier = 1.0f;

                // Adjust timer based on Focus level, because fOR SOME REASON FOCUS AFFECTS IT?????
                switch (Math.Min(3, Skills[52].Level % 256))
                {
                    case 1:
                        multiplier = 0.95f;
                        break;
                    case 2:
                        multiplier = 0.9f;
                        break;
                    case 3:
                        multiplier = 0.8333f;
                        break;
                }
                PlayerParty.Epoch = TimeSpan.FromSeconds(Timer * multiplier);
            }
            else { PlayerParty.Epoch = TimeSpan.Zero; }
        }

        private int GetPartyMemberDamage(int playerIndex)
        {
            long DPSAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.DAMAGE_OFFSET, Address.Offsets.DamageOffsets);
            return Kernel.Read<int>(DPSAddress + (0x2A0 * playerIndex));
        }

        private string GetPartyMemberName(long NameAddress)
        {
            string PartyMemberName = Kernel.ReadString(NameAddress, 32);
            return PartyMemberName ?? PartyMemberName.Trim('\x00');
        }

        private void GetFertilizers()
        {
            long Address = LEVEL_ADDRESS + Memory.Address.Offsets.FertilizersOffset - 0xC;
                       
            sItem[] fertilizers = Kernel.ReadStructure<sItem>(Address, 4);

            for (int i = 0; i < fertilizers.Length; i++)
            {
                Harvest.Box[i].ID = fertilizers[i].ItemId;
                Harvest.Box[i].Amount = fertilizers[i].Amount;
            }
            UpdateHarvestBoxCounter(LEVEL_ADDRESS + Memory.Address.Offsets.FertilizersOffset + (0x10 * 3) - 0xC);
        }

        private void UpdateHarvestBoxCounter(long LastFertAddress)
        {
            long Address = LastFertAddress + Memory.Address.Offsets.HarvestBoxOffset;
            int counter = 0;
            sItem[] elements = Kernel.ReadStructure<sItem>(Address, 50);
            foreach (sItem element in elements)
            {
                if (element.Amount > 0) counter++;
            }
            
            Harvest.Counter = counter;
            
        }

        private void GetSteamFuel()
        {
            long NaturalFuelAddress = LEVEL_ADDRESS + Address.Offsets.SteamFuelOffset;
            Activity.NaturalFuel = Kernel.Read<int>(NaturalFuelAddress);
            Activity.StoredFuel = Kernel.Read<int>(NaturalFuelAddress + 0x4);
        }

        private void GetArgosyData()
        {
            long ArgosyDaysAddress = LEVEL_ADDRESS + Address.Offsets.ArgosyOffset;
            byte ArgosyDays = Kernel.Read<byte>(ArgosyDaysAddress);
            bool ArgosyInTown = ArgosyDays < 250;
            if (ArgosyDays >= 250) { ArgosyDays = (byte)(byte.MaxValue - ArgosyDays + 1); }
            Activity.SetArgosyInfo(ArgosyDays, ArgosyInTown);
        }

        private void GetTailraidersData()
        {
            long TailraidersDaysAddress = LEVEL_ADDRESS + Address.Offsets.TailRaidersOffset;
            byte TailraidersQuestsDone = Kernel.Read<byte>(TailraidersDaysAddress);
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
            long abnormalityBaseAddress = Kernel.ReadMultilevelPtr(
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
            long abnormalityGearBase = Kernel.ReadMultilevelPtr(
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
            float duration = Kernel.Read<float>(abnormalityAddress);

            bool hasConditions = info.HasConditions;
            bool DebuffCondition = false;
            byte stack = 0;
            // Palico and misc buffs don't stack
            switch (info.Type)
            {
                case AbnormalityType.HuntingHorn:
                    stack = Kernel.Read<byte>(baseAddress + 0x164 + (info.Offset - firstHornBuffOffset) / 4);
                    break;
                case AbnormalityType.Debuff:
                    if (info.HasConditions)
                    {
                        stack = Kernel.Read<byte>(baseAddress + info.Offset + info.ConditionOffset);
                        DebuffCondition = stack == 0;
                    }
                    break;
                case AbnormalityType.Misc:
                    if (info.HasConditions)
                    {
                        stack = Kernel.Read<byte>(baseAddress + info.Offset + info.ConditionOffset);
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
            long AbnormAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.ABNORMALITY_OFFSET, Address.Offsets.AbnormalityOffsets);
            bool HasSafiBuff = Kernel.Read<int>(AbnormAddress + 0x9A8) >= 1;
            int SafiCounter = HasSafiBuff ? Kernel.Read<int>(AbnormAddress + 0x7A8) : -1;
            long weaponAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.WEAPON_MECHANICS_OFFSET, Address.Offsets.WeaponMechanicsOffsets);
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
                case Classes.HuntingHorn:
                    GetHuntingHornInformation(weaponAddress);
                    HuntingHorn.SafijiivaRegenCounter = SafiCounter;
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

        private void GetHuntingHornInformation(long weaponAddress)
        {
            sHuntingHornMechanics hhCore = Kernel.ReadStructure<sHuntingHornMechanics>(weaponAddress + 0xD4);
            sHuntingHornSong[] availableSongs = Kernel.ReadStructure<sHuntingHornSong>(weaponAddress - 0x1C, 10);

            HuntingHorn.UpdateInformation(hhCore, availableSongs, ActionId);
        }

        private void GetGreatswordInformation(long weaponAddress)
        {
            uint chargeLevel = Kernel.Read<uint>(weaponAddress - 0x14);
            Greatsword.ChargeLevel = chargeLevel;
        }

        private void GetDualBladesInformation(long weaponAddress)
        {
            bool inDemonMode = Kernel.Read<byte>(weaponAddress - 0x4) == 1;
            bool isReducing = Kernel.Read<byte>(weaponAddress - 0x3) == 1;
            float demonGauge = Kernel.Read<float>(weaponAddress);
            DualBlades.InDemonMode = inDemonMode;
            DualBlades.DemonGauge = demonGauge;
            DualBlades.IsReducing = isReducing;
        }

        private void GetLongswordInformation(long weaponAddress)
        {
            float gauge = Kernel.Read<float>(weaponAddress - 0x4);
            int chargeLevel = Kernel.Read<int>(weaponAddress + 0x4);
            float chargeGauge = Kernel.Read<float>(weaponAddress + 0x8);
            float spiritGaugeBlink = Math.Max(Kernel.Read<float>(weaponAddress + 0xC), Kernel.Read<float>(weaponAddress + 0x1C));
            Longsword.InnerGauge = gauge;
            Longsword.ChargeLevel = chargeLevel;
            Longsword.OuterGauge = chargeGauge;
            Longsword.SpiritGaugeBlinkDuration = spiritGaugeBlink;
        }

        private void GetHammerInformation(long weaponAddress)
        {
            bool isPowerCharged = Kernel.Read<byte>(weaponAddress - 0x18) == 1;
            int chargeLevel = Kernel.Read<int>(weaponAddress - 0x10);
            float chargeProgress = Kernel.Read<float>(weaponAddress - 0x14);
            bool isSheathed = Kernel.Read<byte>(weaponAddress - 0x18CB) == 0;
            Hammer.IsPowerCharged = isPowerCharged;
            Hammer.ChargeLevel = chargeLevel;
            Hammer.ChargeProgress = chargeProgress;
            Hammer.IsWeaponSheated = isSheathed;
        }

        private void GetGunLanceInformation(long weaponAddress, long AbnormAddress)
        {
            long AbnormalitiesAddress = AbnormAddress;
            int totalAmmo = Kernel.Read<int>(weaponAddress - 0x4);
            int currentAmmo = Kernel.Read<int>(weaponAddress);
            int totalBigAmmo = Kernel.Read<int>(weaponAddress + 0x10);
            int currentBigAmmo = Kernel.Read<int>(weaponAddress + 0xC);
            float wyvernsfire = Kernel.Read<float>(AbnormalitiesAddress + 0xBC0);
            bool hasFirestakeLoaded = Kernel.Read<float>(weaponAddress + 0xBC) != 0f;
            float wyvernstakeMax = Kernel.Read<float>(weaponAddress + 0xC0);
            // Check if the Firestake timer ptr is 0
            long wyvernstakeTimerPtr = Kernel.Read<long>(weaponAddress + 0x204);
            float wyvernstakeTimer = 0;
            if (wyvernstakeTimerPtr != 0x00000000)
            {
                wyvernstakeTimer = Kernel.Read<float>(wyvernstakeTimerPtr + 0xE20);
            }
            GunLance.TotalAmmo = totalAmmo;
            GunLance.Ammo = currentAmmo;
            GunLance.TotalBigAmmo = totalBigAmmo;
            GunLance.BigAmmo = currentBigAmmo;
            GunLance.WyvernstakeNextMax = hasFirestakeLoaded ? Kernel.Read<float>(weaponAddress + 0xBC) : wyvernstakeMax;
            GunLance.WyvernstakeMax = wyvernstakeMax;
            GunLance.WyvernsFireTimer = wyvernsfire;
            GunLance.HasWyvernstakeLoaded = hasFirestakeLoaded;
            GunLance.WyvernstakeBlastTimer = wyvernstakeTimer;
        }

        private void GetSwitchAxeInformation(long weaponAddress, long buffAddress)
        {
            float powerProlongerMultiplier = CalculatePowerProlongerMultiplier();

            float outerGauge = Kernel.Read<float>(weaponAddress - 0xC);
            float swordChargeTimer = Kernel.Read<float>(weaponAddress - 0x8) * powerProlongerMultiplier;
            float innerGauge = Kernel.Read<float>(weaponAddress - 0x1C);
            float switchAxeBuff = 0;
            bool isAxeBuffActive = Kernel.Read<byte>(buffAddress + 0x6E5) == 1;
            if (isAxeBuffActive)
            {
                switchAxeBuff = Kernel.Read<float>(buffAddress + 0x6E8) * powerProlongerMultiplier;
            }
            SwitchAxe.OuterGauge = outerGauge;
            SwitchAxe.SwordChargeMaxTimer = swordChargeTimer > SwitchAxe.SwordChargeMaxTimer || swordChargeTimer <= 0 ? swordChargeTimer : SwitchAxe.SwordChargeMaxTimer;
            SwitchAxe.SwordChargeTimer = swordChargeTimer;
            SwitchAxe.InnerGauge = innerGauge;
            SwitchAxe.IsBuffActive = isAxeBuffActive;
            SwitchAxe.SwitchAxeBuffMaxTimer = 45 * powerProlongerMultiplier;
            SwitchAxe.SwitchAxeBuffTimer = switchAxeBuff;
        }

        private void GetChargeBladeInformation(long weaponAddress)
        {
            float powerProlongerMultiplier = CalculatePowerProlongerMultiplier();
            float hiddenGauge = Kernel.Read<float>(weaponAddress + 0x4);
            int vialsAmount = Kernel.Read<int>(weaponAddress + 0x8);
            float swordBuff = Kernel.Read<float>(weaponAddress + 0x10) * powerProlongerMultiplier;
            float shieldBuff = Kernel.Read<float>(weaponAddress + 0xC) * powerProlongerMultiplier;
            float poweraxeBuff = Kernel.Read<float>(weaponAddress + 0x104) * powerProlongerMultiplier;
            ChargeBlade.VialChargeGauge = hiddenGauge;
            ChargeBlade.ShieldBuffTimer = shieldBuff;
            ChargeBlade.SwordBuffTimer = swordBuff;
            ChargeBlade.Vials = vialsAmount;
            ChargeBlade.PoweraxeTimer = poweraxeBuff;
        }

        private void GetInsectGlaiveInformation(long weaponAddress)
        {
            float powerProlongerMultiplier = CalculatePowerProlongerMultiplier();

            float redBuff = Kernel.Read<float>(weaponAddress - 0x4);
            float whiteBuff = Kernel.Read<float>(weaponAddress);
            float orangeBuff = Kernel.Read<float>(weaponAddress + 0x4);

            // Insect Glaive has some dumb bugs sometimes where the buffs duration are either negative
            // or NaN
            redBuff = float.IsNaN(redBuff * powerProlongerMultiplier) ? 0 : redBuff * powerProlongerMultiplier;
            whiteBuff = float.IsNaN(whiteBuff * powerProlongerMultiplier) ? 0 : whiteBuff * powerProlongerMultiplier;
            orangeBuff = float.IsNaN(orangeBuff * powerProlongerMultiplier) ? 0 : orangeBuff * powerProlongerMultiplier;

            // For whatever reason, some IGs split their data between two IG structures I suppose?
            // So we can use this pointer that will always point to the right data
            long dataPtr = Kernel.Read<long>(weaponAddress - 0x236C - 0x28);
            float redChargeTimer = Kernel.Read<float>(dataPtr + 0x1BE8);
            float yellowChargeTimer = Kernel.Read<float>(dataPtr + 0x1BEC);
            float kinsectStamina = Kernel.Read<float>(dataPtr + 0xACC);
            KinsectChargeBuff chargeFlag = redChargeTimer > 0 && yellowChargeTimer > 0 ? KinsectChargeBuff.Both :
                redChargeTimer > 0 ? KinsectChargeBuff.Red :
                yellowChargeTimer > 0 ? KinsectChargeBuff.Yellow : KinsectChargeBuff.None;
            int kinsectBuffQueueSize = Kernel.Read<int>(weaponAddress + 0x24);
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
                    BuffQueue.Add(Kernel.Read<int>(weaponAddress + 0xC + (0x4 * i)));
                }
                int BuffQueuePtr = Kernel.Read<int>(weaponAddress + 0x1C);
                InsectGlaive.FirstBuffQueued = BuffQueue.ElementAtOrDefault(BuffQueuePtr);
                InsectGlaive.SecondBuffQueued = BuffQueue.ElementAtOrDefault(BuffQueuePtr + 1 > 2 ? 0 : BuffQueuePtr + 1);
            }
        }

        private void GetBowInformation(long weaponAddress)
        {
            float chargeProgress = Kernel.Read<float>(weaponAddress + 0x44);
            int chargeLevel = Kernel.Read<int>(weaponAddress + 0x48);
            int mChargeLevel = Kernel.Read<int>(weaponAddress + 0x4C);
            bool isSheathed = Kernel.Read<byte>(weaponAddress - 0x18CB) == 0;
            Bow.MaxChargeLevel = mChargeLevel;
            Bow.ChargeLevel = chargeLevel;
            Bow.ChargeProgress = chargeProgress;
            Bow.IsWeaponSheated = isSheathed;
        }

        private void GetLightBowgunInformation(long weaponAddress)
        {
            float specialAmmoTimer = Kernel.Read<float>(weaponAddress + 0x4E0);
            LightBowgun.SpecialAmmoRegen = specialAmmoTimer;
        }

        private void GetHeavyBowgunInformation(long weaponAddress)
        {
            HeavyBowgunInformation data = new HeavyBowgunInformation
            {
                WyvernsnipeTimer = Kernel.Read<float>(weaponAddress - 0xC),
                WyvernheartTimer = Kernel.Read<float>(weaponAddress - 0x14),
                HasScopeEquipped = Kernel.Read<byte>(weaponAddress + 0x4B8) == 1,
                ScopeZoomMultiplier = Kernel.Read<float>(weaponAddress + 0x4D0),
                EquippedAmmo = Kernel.ReadStructure<sEquippedAmmo>(weaponAddress + 0x454),
                SpecialAmmoType = (HBGSpecialType)Kernel.Read<int>(weaponAddress - 0x18),
                FocusLevel = Math.Min(3, Skills[52].Level % 256)
            };
            sAmmo[] ammos = Kernel.ReadStructure<sAmmo>(weaponAddress + 0x34, 40);
            HeavyBowgun.UpdateInformation(data, ammos);
        }
#endregion

        private float CalculatePowerProlongerMultiplier()
        {
            short level = (short)Math.Min(3, (Skills[53].Level % 512));
            //Debugger.Log(level);
            if (level == 0) return 1.0f;

            if ((Classes)WeaponID == Classes.SwitchAxe || (Classes)WeaponID == Classes.DualBlades)
            {
                return 1.0f + ((float)Math.Pow(2, level - 1) / 10.0f) + (2 * (float)level / 10);
            } else
            {
                return 1.0f + (float)Math.Pow(2, level - 1) / 10.0f;
            }
        }
    }
}
