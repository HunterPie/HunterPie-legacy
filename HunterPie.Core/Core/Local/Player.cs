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
using HunterPie.Utils;

namespace HunterPie.Core
{
    public class Player
    {
        #region Static stuff

        public static readonly float[] MaximumHealthPossible = {
            0.0f,
            15.0f,
            30.0f,
            50.0f
        };

        #endregion

        #region PRIVATE
        private long playerAddress = 0x0;
        private int level;
        private int zoneId = -1;
        private byte weaponId;
        private string sessionId;
        private long classAddress;
        private int actionId;
        private float ailmentTimer;
        private int masterRank;
        private Job currentWeapon;
        private bool hasHotDrink;
        private bool hasHUDActive = true;
        private PlayerAilment ailmentType;

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
        #endregion

        /// <summary>
        /// Current player save address
        /// </summary>
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
        public int MasterRank
        {
            get => masterRank;
            private set
            {
                if (masterRank != value)
                {
                    masterRank = value;
                    Dispatch(OnLevelChange, new PlayerEventArgs(this));
                }
            }
        }

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

        /// <summary>
        /// Player weapon data memory address
        /// </summary>
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
        public readonly sPlayerSkill[] Skills = new sPlayerSkill[226];

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
                    zoneId = value;

                    Dispatch(OnZoneChange, new PlayerLocationEventArgs(this));

                    if ((LastZoneID == -1 || peaceZones.Contains(LastZoneID)) && !peaceZones.Contains(zoneId))
                        Dispatch(OnPeaceZoneLeave, new PlayerLocationEventArgs(this));

                    if ((LastZoneID == -1 || harvestBoxZones.Contains(LastZoneID)) && !harvestBoxZones.Contains(zoneId))
                        Dispatch(OnVillageLeave, new PlayerLocationEventArgs(this));

                    if (!peaceZones.Contains(LastZoneID) && peaceZones.Contains(zoneId))
                        Dispatch(OnPeaceZoneEnter, new PlayerLocationEventArgs(this));

                    if (!harvestBoxZones.Contains(LastZoneID) && harvestBoxZones.Contains(zoneId))
                        Dispatch(OnVillageEnter, new PlayerLocationEventArgs(this));

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
        /// Player health component
        /// </summary>
        public readonly HealthComponent Health = new HealthComponent();

        /// <summary>
        /// Player stamina component
        /// </summary>
        public readonly StaminaComponent Stamina = new StaminaComponent();

        /// <summary>
        /// Food data
        /// </summary>
        public sFoodData FoodData = new sFoodData();

        /// <summary>
        /// Player action id
        /// </summary>
        public int ActionId
        {
            get => actionId;
            private set
            {
                if (value != actionId)
                {
                    actionId = value;
                    Dispatch(OnActionChange, new PlayerEventArgs(this));
                    Debugger.Debug($"Player -> {PlayerActionRef} (ID: {value})");
                }
            }
        }

        /// <summary>
        /// Player current ailment duration timer
        /// </summary>
        public float AilmentTimer
        {
            get => ailmentTimer;
            private set
            {
                if (value != ailmentTimer)
                {
                    ailmentTimer = value;
                    Dispatch(OnAilmentUpdate);
                }
            }
        }

        /// <summary>
        /// Player current ailment max duration
        /// </summary>
        public float MaxAilmentTimer { get; private set; }

        /// <summary>
        /// Player current active ailment, if there's any
        /// </summary>
        public PlayerAilment AilmentType
        {
            get => ailmentType;
            private set
            {
                if (value != ailmentType)
                {
                    ailmentType = value;
                    Dispatch(OnAilmentUpdate);
                }
            }
        }

        /// <summary>
        /// Whether the player has drank hot drink or not while in snow stages
        /// </summary>
        public bool HasHotDrink
        {
            get => hasHotDrink;
            private set
            {
                if (value != hasHotDrink)
                {
                    hasHotDrink = value;
                    Dispatch(OnHotDrinkStateChange, new PlayerEventArgs(this));
                }
            }
        }

        /// <summary>
        /// Whether the player has drank hot drink or not while in snow stages
        /// </summary>
        public bool HasHUDActive
        {
            get => hasHUDActive;
            private set
            {
                if (value != hasHUDActive)
                {
                    hasHUDActive = value;
                    Dispatch(OnHUDActiveChange, new PlayerEventArgs(this));
                }
            }
        }

        /// <summary>
        /// Gets the raw name for the player current action reference name
        /// </summary>
        public string PlayerActionRef { get; private set; }

        /// <summary>
        /// Player position
        /// </summary>
        public readonly Vector3 Position = new Vector3(0, 0, 0);

        /// <summary>
        /// Player item pouch
        /// </summary>
        public readonly Inventory Inventory = new Inventory();

        /// <summary>
        /// Player item box
        /// </summary>
        public readonly ItemBox ItemBox = new ItemBox();

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
        public readonly SwordAndShield SwordAndShield = new SwordAndShield();
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

        /// <summary>
        /// Pointer to the current weapon
        /// </summary>
        public Job CurrentWeapon
        {
            get => currentWeapon;
            set
            {
                if (currentWeapon is null || value.Type != currentWeapon.Type)
                {
                    LastWeapon = currentWeapon;
                    currentWeapon = value;
                }
            }
        }

        public Job LastWeapon { get; private set; }
        #endregion

        // Threading
        private ThreadStart scanPlayerInfoRef;
        private Thread scanPlayerInfo;

        #region Events
        // Event handlers

        public delegate void PlayerEvents(object source, EventArgs args);
        public delegate void PlayerAilmentEvents(object source, PlayerAilmentEventArgs args);

        public event PlayerEvents OnLevelChange;
        public event PlayerEvents OnWeaponChange;
        public event PlayerEvents OnSessionChange;
        public event PlayerEvents OnClassChange;
        public event PlayerEvents OnActionChange;

        public event PlayerEvents OnCharacterLogin;
        public event PlayerEvents OnCharacterLogout;

        public event PlayerEvents OnZoneChange;
        public event PlayerEvents OnPeaceZoneEnter;
        public event PlayerEvents OnVillageEnter;
        public event PlayerEvents OnPeaceZoneLeave;
        public event PlayerEvents OnVillageLeave;
        public event PlayerEvents OnHotDrinkStateChange;
        public event PlayerEvents OnHUDActiveChange;

        public event PlayerAilmentEvents OnAilmentUpdate;

        public event PlayerEvents OnPlayerScanFinished;

        private void Dispatch(PlayerEvents e, EventArgs args)
        {
            if (e is null)
                return;

            foreach (PlayerEvents del in e.GetInvocationList())
            {
                try
                {
                    del(this, args);
                } catch (Exception err)
                {
                    Debugger.Error($"Error on callback \"{del.Method.Name}\": {err.Message}");
                }
            }
        }
        private void Dispatch(PlayerAilmentEvents e)
        {
            if (e is null)
                return;

            PlayerAilmentEventArgs args = new PlayerAilmentEventArgs(this);
            foreach (PlayerAilmentEvents del in e.GetInvocationList())
            {
                try
                {
                    del(this, args);
                }
                catch (Exception err)
                {
                    Debugger.Error($"Error on callback \"{del.Method.Name}\": {err.Message}");
                }
            }
        }

        private void DispatchScanFinished()
        {
            if (OnPlayerScanFinished == null)
            {
                return;
            }

            foreach (PlayerEvents sub in OnPlayerScanFinished.GetInvocationList())
            {
                try
                {
                    sub(this, EventArgs.Empty);
                } catch (Exception err)
                {
                    Debugger.Error($"Exception in {sub.Method.Name}: {err.Message}");
                    OnPlayerScanFinished -= sub;
                }
            }

        }
        #endregion

        #region Scanner
        internal void StartScanning()
        {
            scanPlayerInfoRef = new ThreadStart(GetPlayerInfo);
            scanPlayerInfo = new Thread(scanPlayerInfoRef)
            {
                Name = "Scanner_Player"
            };
            scanPlayerInfo.SetApartmentState(ApartmentState.STA);
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_PLAYER_SCANNER_INITIALIZED']"));
            scanPlayerInfo.Start();
        }

        internal void StopScanning() => scanPlayerInfo.Abort();
        #endregion

        #region Manual Player Data
        /*
            Player data that needs to be called by an external function and doesn't need to keep track of this data
            every second.
        */
        public GameStructs.Gear GetPlayerGear()
        {
            long PlayerGearBase = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("EQUIPMENT_OFFSET"), Address.GetOffsets("PlayerGearOffsets"));

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
            sItem[] decorations = Kernel.ReadStructure<sItem>(PlayerAddress + 0x3F098, 500);

            return decorations;
        }


        public sGear[] GetGearFromStorage()
        {
            // We have up to 2509 different slots in our storage box
            // And 127 in the mantle box?
            List<sGear> gear = new List<sGear>();

            sGear[] gearBox = Kernel.ReadStructure<sGear>(LEVEL_ADDRESS + 0x40FD8, 2509);
            sGear[] mantleBox = Kernel.ReadStructure<sGear>(LEVEL_ADDRESS + 0xE9258, 127);

            gear.AddRange(gearBox);
            gear.AddRange(mantleBox);

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
                    GetPlayerFoodSkills();
                    GetPlayerBasicInfo();
                    GetPlayerInventory();
                    GetFertilizers();
                    GetArgosyData();
                    GetTailraidersData();
                    GetSteamFuel();
                    GetPrimaryMantle();
                    GetSecondaryMantle();
                    GetMantleTimers();
                    GetPlayerSkills();
                    GetParty();
                    GetPlayerEffects();
                    GetPlayerAbnormalities();
                    GetJobInformation();
                    GetPlayerPosition();
                    GetItemBox();
                    GetHUDState();
                }
                GetSessionId();
                GetEquipmentAddress();

                DispatchScanFinished();
                Thread.Sleep(ConfigManager.Settings.Overlay.GameScanDelay);
            }
            Thread.Sleep(1000);
            GetPlayerInfo();
        }

        private void GetPlayerEffects()
        {
            long hotDrinkFlagAddress = Kernel.ReadMultilevelPtr(EQUIPMENT_ADDRESS + 0x920, new [] { 0x13D58, 0x8, 0xA24 });
            bool hotDrinkFlag = Kernel.Read<byte>(hotDrinkFlagAddress) == 0;

            HasHotDrink = hotDrinkFlag;
        }

        private void GetHUDState()
        {
            // Could read in other HUD details if that matters to anyone
            var hasHUDActiveAddress = Kernel.ReadMultilevelPtr(
                Address.GetAddress("BASE") + Address.GetAddress("GAME_HUD_INFO_OFFSET"),
                Address.GetOffsets("HudActiveOffsets"));

            HasHUDActive = (Kernel.Read<byte>(hasHUDActiveAddress) == 1);
        }

        private bool GetPlayerAddress()
        {
            if (ZoneID == 0)
            {
                PlayerAddress = 0;
                LEVEL_ADDRESS = 0;
                return false;
            }
            long FirstSaveAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("LEVEL_OFFSET"), Address.GetOffsets("LevelOffsets"));
            uint CurrentSaveSlot = Kernel.Read<uint>(FirstSaveAddress + 0x44);
            long NextPlayerSave = 0x27E9F0;
            long CurrentPlayerSaveHeader = Kernel.Read<long>(FirstSaveAddress) + NextPlayerSave * CurrentSaveSlot;

            if (CurrentPlayerSaveHeader != PlayerAddress)
            {
                LEVEL_ADDRESS = CurrentPlayerSaveHeader + 0x90;
                GetPlayerName();
                GetPlayerLevel();
                GetPlayerMasterRank();
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

        private void GetItemBox()
        {
            // I think this changes. I had a different value before the 12/2 update
            int offset = 0x38a08;
            sItem[] itemBox = Kernel.ReadStructure<sItem>(PlayerAddress + offset, 2150);

            sItem[] consumables = new sItem[200];
            sItem[] ammo = new sItem[200];
            sItem[] materials = new sItem[1250];
            sItem[] decorations = new sItem[500];

            Array.Copy(itemBox, 0, consumables, 0, consumables.Length);
            Array.Copy(itemBox, 200, ammo, 0, ammo.Length);
            Array.Copy(itemBox, 400, materials, 0, materials.Length);
            Array.Copy(itemBox, 1650, decorations, 0, decorations.Length);

            ItemBox.Refresh(consumables, ammo, materials, decorations);
        }

        private void GetPlayerPosition()
        {
            long address = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("EQUIPMENT_OFFSET"), Address.GetOffsets("PlayerPositionOffsets"));
            sVector3 vector3 = Kernel.ReadStructure<sVector3>(address);
            Position.Update(vector3);
        }

        private void GetZoneId()
        {
            long ZoneAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("ZONE_OFFSET"), Address.GetOffsets("ZoneOffsets"));
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
            long address = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("EQUIPMENT_OFFSET"), Address.GetOffsets("PlayerBasicInformationOffsets"));

            GetPlayerHealth(address);

            GetPlayerStamina(address);

            // ActionId is our rcx
            long characterPointer = Kernel.Read<long>(address + 0x30);
            int actionId = Kernel.Read<int>(characterPointer + 0x6278);
            // mov      rax,[r8+r9*8+68] ;Always VILLAGE::IDLE
            // mov      rbx,[rax+rcx*8] ; will give us our player action ref name pointer
            long playerActionRefPtr = Kernel.ReadMultilevelPtr(characterPointer + 0x6240, new int[] { actionId * 8, 0x20, 0x0 });

            string playerActionRef = Kernel.ReadString(playerActionRefPtr, 64);

            PlayerActionRef = playerActionRef;
            ActionId = actionId;
            // Hacky way to update the eat timer
            if (!InHarvestZone)
            {
                long EatTimerAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("CANTEEN_OFFSET"), Address.GetOffsets("PlayerCanteenTimer"));
                UpdateAbnormality(AbnormalityData.MiscAbnormalities.Where(a => a.Id == 999).FirstOrDefault(), EatTimerAddress);
            }
            GetPlayerAilment(address);
        }

        private void GetPlayerHealth(long address)
        {
            long cGuiHealthAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("HUD_DATA_OFFSET"), Address.GetOffsets("gHudHealthBarOffsets"));

            sHealingData[] healingArray = Kernel.ReadStructure<sHealingData>(Kernel.Read<long>(address + 0x30) + 0xEBB0, 4);

            float[] health = Kernel.ReadStructure<float>(address + 0x60, 2);

            // Ensure we update the food skills before updating the health component
            if (health[0] != Health.MaxHealth)
            {
                GetPlayerFoodSkills();
            }

            Health.Update(
                maxHealth: health[0],
                health: health[1],
                healData: CalculateHealingData(healingArray),
                redHealth: Kernel.Read<float>(address + 0x2DE4)
            );

            if (cGuiHealthAddress != Kernel.NULLPTR)
            {
                sGuiHealth guiData = Kernel.ReadStructure<sGuiHealth>(cGuiHealthAddress + 0x460);
                guiData.MaxPossibleHealth = CalculatePlayerMaximumPossibleHealth();
                Health.Update(guiData);
            }
        }

        private sHealingData CalculateHealingData(sHealingData[] data)
        {
            sHealingData totalHealingData = new sHealingData
            {
                Ref1 = data[0].Ref1,
                Ref2 = data[0].Ref2,
                CurrentHeal = 0,
                OldMaxHeal = 0,
                MaxHeal = 0,
                Stage = 2,
            };

            for (int i = 0; i < data.Length; i++)
            {
                sHealingData current = data[i];
                float max = current.Stage == 1 ? current.MaxHeal * 2.5f : current.MaxHeal;
                if (current.Stage != 0)
                {
                    totalHealingData.CurrentHeal += current.CurrentHeal;
                    totalHealingData.MaxHeal += max;
                }
            }

            return totalHealingData;
        }

        private void GetPlayerFoodSkills()
        {
            long address = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("PLAYER_DATA_OFFSET"),
                Address.GetOffsets("FoodDataOffsets"));

            FoodData = Kernel.ReadStructure<sFoodData>(address);

        }

        /// <summary>
        /// Calculates the maximum possible health based on set skill level
        /// </summary>
        /// <returns>The maximum health the player can have</returns>
        private float CalculatePlayerMaximumPossibleHealth()
        {
            if (Skills != null && Skills.Length > 0)
            {
                sPlayerSkill vitality = Skills[(int)SetSkills.Vitality];
                return 150.0f + MaximumHealthPossible[Math.Min(vitality.LevelGear, MaximumHealthPossible.Length - 1)];
            }
            return 150.0f;
        }

        private void GetPlayerStamina(long address)
        {
            long cGuiStaminaAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("HUD_DATA_OFFSET"), Address.GetOffsets("gHudStaminaBarOffsets"));

            Stamina.Update(
                maxStamina: Kernel.Read<float>(address + 0x144),
                stamina: Kernel.Read<float>(address + 0x13C)
            );

            if (cGuiStaminaAddress != Kernel.NULLPTR)
            {
                sGuiStamina guiData = Kernel.ReadStructure<sGuiStamina>(cGuiStaminaAddress + 0x1F0);
                guiData.maxPossibleStamina = CalculateMaxPossibleStamina();
                Stamina.Update(guiData);
            }

        }

        private float CalculateMaxPossibleStamina()
        {
            if (Skills != null && Skills.Length > 0)
            {
                bool lunaFavor = Skills[(int)SetSkills.LunastraStaminaCapUp].LevelGear >= 2;
                bool anjaDominance = Skills[(int)SetSkills.AnjanathDominance].LevelGear >= 2;
                bool anjaWill = Skills[(int)SetSkills.AnjanathWill].LevelGear >= 4;

                bool stamCapActive = lunaFavor || anjaDominance || anjaWill;
                return stamCapActive ? 200.0f : 150.0f;
            }
            return 150.0f;
        }

        /// <summary>
        /// Gets the current player ailment if there's any
        /// </summary>
        private void GetPlayerAilment(long address)
        {
            float timer = Kernel.Read<float>(address + 0x2DF4);
            MaxAilmentTimer = Kernel.Read<float>(address + 0x2DF8);
            if (PlayerActionRef.Contains("SLEEP"))
            {
                AilmentType = PlayerAilment.Sleep;
            } else if (PlayerActionRef.Contains("PARALYSE"))
            {
                AilmentType = PlayerAilment.Paralysis;
            } else if (PlayerActionRef.Contains("STUN"))
            {
                AilmentType = PlayerAilment.Stun;
            } else
            {
                AilmentType = PlayerAilment.None;
                AilmentTimer = 0;
                return;
            }
            AilmentTimer = timer;
        }

        private void GetWeaponId()
        {
            long address = Address.GetAddress("BASE") + Address.GetAddress("WEAPON_OFFSET");
            address = Kernel.ReadMultilevelPtr(address, Address.GetOffsets("WeaponOffsets"));
            PlayerStructAddress = address;
            WeaponID = Kernel.Read<byte>(address);
        }

        private void GetSessionId()
        {
            long address = Address.GetAddress("BASE") + Address.GetAddress("SESSION_OFFSET");
            address = Kernel.ReadMultilevelPtr(address, Address.GetOffsets("SessionOffsets"));
            SESSION_ADDRESS = address;
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
            long address = Address.GetAddress("BASE") + Address.GetAddress("EQUIPMENT_OFFSET");
            address = Kernel.ReadMultilevelPtr(address, Address.GetOffsets("EquipmentOffsets"));
            if (EQUIPMENT_ADDRESS != address) Debugger.Debug($"New equipment address found -> 0x{address:X}");
            EQUIPMENT_ADDRESS = address;
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

        private void GetMantleTimers()
        {
            Mantle[] mantles = new Mantle[] { PrimaryMantle, SecondaryMantle };
            foreach (Mantle mantle in mantles) {
                long mantleTimerFixed = (mantle.ID * 4) + Address.TimerFixed;
                long mantleTimer = (mantle.ID * 4) + Address.TimerDynamic;
                long mantleCdFixed = (mantle.ID * 4) + Address.CooldownFixed;
                long mantleCd = (mantle.ID * 4) + Address.CooldownDynamic;

                mantle.SetCooldown(
                    Kernel.Read<float>(EQUIPMENT_ADDRESS + mantleCd),
                    Kernel.Read<float>(EQUIPMENT_ADDRESS + mantleCdFixed)
                );

                mantle.SetTimer(
                    Kernel.Read<float>(EQUIPMENT_ADDRESS + mantleTimer),
                    Kernel.Read<float>(EQUIPMENT_ADDRESS + mantleTimerFixed)
                );
            }
        }

        private void GetPlayerSkills()
        {
            long address = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("ABNORMALITY_OFFSET"), Address.GetOffsets("SkillOffsets"));
            sPlayerSkill[] buffer = Kernel.ReadStructure<sPlayerSkill>(address, 226);
            Array.Copy(buffer, Skills, buffer.Length);
        }

        private void GetParty()
        {

            long address = Address.GetAddress("BASE") + Address.GetAddress("PARTY_OFFSET");
            long PartyContainer = Kernel.ReadMultilevelPtr(address, Address.GetOffsets("PartyOffsets")) - 0x22B7;
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

                for (int i = 0; i < PlayerParty.MaxSize; i++)
                {
                    short[] levels = Kernel.ReadStructure<short>(PartyContainer + (i * 0x1C0 + 0x27), 2);
                    MemberInfo dummy = new MemberInfo
                    {
                        Name = GetPartyMemberName(PartyContainer + (i * 0x1C0)),
                        HR = levels[0],
                        MR = levels[1],
                        Damage = playerDamages[i],
                        DamagePercentage = playerDamages[i] > 0 ? playerDamages[i] / (float)totalDamage : 0,
                        IsLeader = i == 0,
                    };
                    dummy.WeaponId = dummy.Name == Name && dummy.HR == Level ? WeaponID :
                        Kernel.Read<byte>(PartyContainer + (i * 0x1C0 + 0x33));

                    dummy.IsLocalPlayer = dummy.Name == Name && dummy.HR == Level;

                    PlayerParty[i].SetPlayerInfo(dummy);
                }

                PlayerParty.TotalDamage = totalDamage;
                GetQuestElapsedTime();
            }

        }

        private void GetQuestElapsedTime()
        {
            long TimerAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("ABNORMALITY_OFFSET"), Address.GetOffsets("AbnormalityOffsets"));
            float Timer = Kernel.Read<float>(TimerAddress + 0xC24);
            PlayerParty.ShowDPS = true;
            if (Timer > 0)
            {
                float multiplier;

                // Adjust timer based on Focus level, because fOR SOME REASON FOCUS AFFECTS IT?????
                switch (Math.Min(3, (int)Skills[(int)SetSkills.Focus].LevelGear))
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
                    default:
                        multiplier = 1f;
                        break;
                }
                PlayerParty.Epoch = TimeSpan.FromSeconds(Math.Min(Timer, TimeSpan.MaxValue.TotalSeconds) * multiplier);
            }
            else { PlayerParty.Epoch = TimeSpan.Zero; }
        }

        private int GetPartyMemberDamage(int playerIndex)
        {
            long DPSAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("DAMAGE_OFFSET"), Address.GetOffsets("DamageOffsets"));
            return Kernel.Read<int>(DPSAddress + (0x2A0 * playerIndex));
        }

        private string GetPartyMemberName(long NameAddress)
        {
            string PartyMemberName = Kernel.ReadString(NameAddress, 32);
            return PartyMemberName ?? PartyMemberName.Trim('\x00');
        }

        private void GetFertilizers()
        {
            long address = LEVEL_ADDRESS + Offsets.FertilizersOffset - 0xC;

            sItem[] fertilizers = Kernel.ReadStructure<sItem>(address, 4);

            for (int i = 0; i < fertilizers.Length; i++)
            {
                Harvest.Box[i].ID = fertilizers[i].ItemId;
                Harvest.Box[i].Amount = fertilizers[i].Amount;
            }
            UpdateHarvestBoxCounter(LEVEL_ADDRESS + Offsets.FertilizersOffset + (0x10 * 3) - 0xC);
        }

        private void UpdateHarvestBoxCounter(long LastFertAddress)
        {
            long address = LastFertAddress + Offsets.HarvestBoxOffset;
            Harvest.Max = GetHarvestBoxSize();
            int counter = 0;
            sItem[] elements = Kernel.ReadStructure<sItem>(address, Harvest.Max);
            foreach (sItem element in elements)
            {
                if (element.Amount > 0)
                    counter++;
            }
            Harvest.Counter = counter;
        }

        private int GetHarvestBoxSize()
        {
            var saveBase = LEVEL_ADDRESS - 0x90;
            var firstBits = Kernel.Read<uint>(saveBase + 0x102454) >> 0xd & 0b11;
            var lastBits = Kernel.Read<uint>(saveBase + 0x102570) >> 0x17 & 0b11;

            var upgrades = (firstBits << 2) | lastBits;
            return (BitUtils.CountBits(upgrades) + 1) * 10;
        }

        private void GetSteamFuel()
        {
            long NaturalFuelAddress = LEVEL_ADDRESS + Offsets.SteamFuelOffset;
            Activity.NaturalFuel = Kernel.Read<int>(NaturalFuelAddress);
            Activity.StoredFuel = Kernel.Read<int>(NaturalFuelAddress + 0x4);
        }

        private void GetArgosyData()
        {
            long ArgosyDaysAddress = LEVEL_ADDRESS + Offsets.ArgosyOffset;
            byte ArgosyDays = Kernel.Read<byte>(ArgosyDaysAddress);
            bool ArgosyInTown = ArgosyDays < 250;
            if (ArgosyDays >= 250) { ArgosyDays = (byte)(byte.MaxValue - ArgosyDays + 1); }
            Activity.SetArgosyInfo(ArgosyDays, ArgosyInTown);
        }

        private void GetTailraidersData()
        {
            long TailraidersDaysAddress = LEVEL_ADDRESS + Offsets.TailRaidersOffset;
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
                Address.GetAddress("BASE") + Address.GetAddress("ABNORMALITY_OFFSET"), Address.GetOffsets("AbnormalityOffsets"));

            float[] abnormDurationArray = Kernel.ReadStructure<float>(abnormalityBaseAddress + 0x38, 75);
            GetPlayerHuntingHornAbnormalities(abnormalityBaseAddress, abnormDurationArray);
            GetPlayerPalicoAbnormalities(abnormalityBaseAddress, abnormDurationArray);
            GetPlayerMiscAbnormalities(abnormalityBaseAddress);
            GetPlayerGearAbnormalities(abnormalityBaseAddress);
        }

        private void GetPlayerHuntingHornAbnormalities(long abnormalityBaseAddress, float[] cache)
        {
            // Gets the player abnormalities caused by HH
            foreach (AbnormalityInfo abnormality in AbnormalityData.HuntingHornAbnormalities)
            {
                UpdateAbnormality(abnormality, abnormalityBaseAddress, cache);
            }
        }

        private void GetPlayerPalicoAbnormalities(long abnormalityBaseAddress, float[] cache)
        {
            // Gets the player abnormalities caused by palico's skills
            foreach (AbnormalityInfo abnormality in AbnormalityData.PalicoAbnormalities)
            {
                UpdateAbnormality(abnormality, abnormalityBaseAddress, cache);
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
                Address.GetAddress("BASE") + Address.GetAddress("ABNORMALITY_OFFSET"),
                Address.GetOffsets("AbnormalityGearOffsets"));
            foreach (AbnormalityInfo abnormality in AbnormalityData.GearAbnormalities)
            {
                UpdateAbnormality(abnormality, (abnormality.IsGearBuff ? abnormalityGearBase : abnormalityBaseAddress));
            }
        }

        private void UpdateAbnormality(AbnormalityInfo info, long baseAddress, float[] cached = null)
        {
            const int firstHornBuffOffset = 0x38;
            long abnormalityAddress = baseAddress + info.Offset;
            float duration;
            if (cached != null)
            {
                duration = cached[(info.Offset - firstHornBuffOffset) / sizeof(float)];
            } else
            {
                duration = Kernel.Read<float>(abnormalityAddress);
            }


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

            if (duration <= 0 && !(hasConditions && info.IsInfinite))
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
            // Fix for arena dumb memory leak when choosing the class
            if (PlayerActionRef == "Village::FIELD_GO_YOKURYU")
            {
                return;
            }
            long AbnormAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("ABNORMALITY_OFFSET"), Address.GetOffsets("AbnormalityOffsets"));
            bool HasSafiBuff = Kernel.Read<int>(AbnormAddress + 0xA04) >= 1;
            int SafiCounter = HasSafiBuff ? Kernel.Read<int>(AbnormAddress + 0x7A8) : -1;
            long weaponAddress = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("WEAPON_MECHANICS_OFFSET"), Address.GetOffsets("WeaponMechanicsOffsets"));
            switch ((Classes)WeaponID)
            {
                case Classes.Greatsword:
                    GetGreatswordInformation(weaponAddress);
                    CurrentWeapon = Greatsword;
                    break;
                case Classes.SwordAndShield:
                    CurrentWeapon = SwordAndShield;
                    break;
                case Classes.DualBlades:
                    GetDualBladesInformation(weaponAddress);
                    CurrentWeapon = DualBlades;
                    break;
                case Classes.LongSword:
                    GetLongswordInformation(weaponAddress);
                    CurrentWeapon = Longsword;
                    break;
                case Classes.Hammer:
                    GetHammerInformation(weaponAddress);
                    CurrentWeapon = Hammer;
                    break;
                case Classes.HuntingHorn:
                    GetHuntingHornInformation(weaponAddress);
                    CurrentWeapon = HuntingHorn;
                    break;
                case Classes.Lance:
                    CurrentWeapon = Lance;
                    break;
                case Classes.GunLance:
                    GetGunLanceInformation(weaponAddress, AbnormAddress);
                    CurrentWeapon = GunLance;
                    break;
                case Classes.SwitchAxe:
                    GetSwitchAxeInformation(weaponAddress, AbnormAddress);
                    CurrentWeapon = SwitchAxe;
                    break;
                case Classes.ChargeBlade:
                    GetChargeBladeInformation(weaponAddress);
                    CurrentWeapon = ChargeBlade;
                    break;
                case Classes.InsectGlaive:
                    GetInsectGlaiveInformation(weaponAddress);
                    CurrentWeapon = InsectGlaive;
                    break;
                case Classes.Bow:
                    GetBowInformation(weaponAddress);
                    CurrentWeapon = Bow;
                    break;
                case Classes.HeavyBowgun:
                    GetHeavyBowgunInformation(weaponAddress);
                    CurrentWeapon = HeavyBowgun;
                    break;
                case Classes.LightBowgun:
                    GetLightBowgunInformation(weaponAddress);
                    CurrentWeapon = LightBowgun;
                    break;
            }

            if (CurrentWeapon != null)
            {
                CurrentWeapon.SafijiivaRegenCounter = SafiCounter;
                GetWeaponSharpness(weaponAddress);
            }

            ClassAddress = weaponAddress;
        }

        private void GetWeaponSharpness(long weaponAddress)
        {
            long weaponDataPtr = Kernel.ReadMultilevelPtr(Address.GetAddress("BASE") + Address.GetAddress("WEAPON_DATA_OFFSET"), Address.GetOffsets("WeaponDataOffsets"));

            if (weaponDataPtr == Kernel.NULLPTR)
                return;

            int rcx = Kernel.Read<int>(weaponAddress - 0x236C + 0x1D0C);
            // weaponDataPtr is our rax
            // mov  rax,[rax+rcx*8] ; This will give us the pointer to the weapon sharpness array
            // Weapon sharpness data is located in rax + 0x0C
            long weaponSharpnessPtr = Kernel.ReadMultilevelPtr(weaponDataPtr, new int[] { rcx * 8, 0x0C });
            short[] weaponSharpnessData = Kernel.ReadStructure<short>(weaponSharpnessPtr, 7);
            sSharpness weaponSharpness = Kernel.ReadStructure<sSharpness>(weaponAddress - 0x274);
            int maxLevel = weaponSharpnessData.Length - 1;
            for (int i = maxLevel; i > 0; i--)
            {
                if (weaponSharpnessData[i] == 0)
                {
                    maxLevel = i - 1;
                    break;
                }
            }

            CurrentWeapon.Sharpnesses = weaponSharpnessData;
            CurrentWeapon.SharpnessLevel = weaponSharpness.Level;
            CurrentWeapon.MaximumSharpness = weaponSharpnessData[maxLevel] - (50 - (Math.Min(Skills[(int)SetSkills.Handicraft].LevelGear, (byte)5) * 10));
            CurrentWeapon.Sharpness = weaponSharpness.Sharpness;
        }

        private void GetHuntingHornInformation(long weaponAddress)
        {
            sHuntingHornMechanics hhCore = Kernel.ReadStructure<sHuntingHornMechanics>(weaponAddress + 0xD4);
            sHuntingHornSong[] availableSongs = Kernel.ReadStructure<sHuntingHornSong>(weaponAddress - 0x1C, 10);

            HuntingHorn.UpdateInformation(hhCore, availableSongs, ActionId);
        }

        private void GetGreatswordInformation(long weaponAddress)
        {
            float chargeTimer = Kernel.Read<float>(weaponAddress - 0x1C);
            uint chargeLevel = Kernel.Read<uint>(weaponAddress - 0x14);
            Greatsword.IsWeaponSheated = Kernel.Read<byte>(weaponAddress - 0x18CB) == 0;
            Greatsword.ChargeLevel = chargeLevel;
            Greatsword.ChargeTimer = chargeTimer;
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
            float IaiSlash = Kernel.Read<float>(weaponAddress + 0xC);
            float HelmBreaker = Kernel.Read<float>(weaponAddress + 0x1C);
            Longsword.InnerGauge = gauge;
            Longsword.ChargeLevel = chargeLevel;
            Longsword.OuterGauge = chargeGauge;
            Longsword.HelmBreakerBlink = float.IsNaN(HelmBreaker) ? 0 : HelmBreaker;
            Longsword.IaiSlashBlink = float.IsNaN(IaiSlash) ? 0 : IaiSlash;
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
            float wyvernsfire = Kernel.Read<float>(AbnormalitiesAddress + 0xC20);
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
            sAmmo[] ammos = Kernel.ReadStructure<sAmmo>(weaponAddress + 0x34, 40);
            LightBowgunInformation data = new LightBowgunInformation
            {
                SpecialAmmoRegen = Kernel.Read<float>(weaponAddress + 0x4E0),
                GroundAmmo = Kernel.Read<int>(weaponAddress + 0x4DC),
                EquippedAmmo = Kernel.ReadStructure<sEquippedAmmo>(weaponAddress + 0x454)
            };
            LightBowgun.UpdateInformation(data, ammos);
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
                FocusLevel = Math.Min(3, (int)Skills[(int)SetSkills.Focus].LevelGear)
            };
            sAmmo[] ammos = Kernel.ReadStructure<sAmmo>(weaponAddress + 0x34, 40);
            HeavyBowgun.UpdateInformation(data, ammos);
        }
#endregion

        private float CalculatePowerProlongerMultiplier()
        {
            short level = (short)Math.Min(3, (int)(Skills[(int)SetSkills.PowerProlonger].LevelGear));
            if (level == 0) return 1.0f;

            if ((Classes)WeaponID == Classes.SwitchAxe || (Classes)WeaponID == Classes.DualBlades)
            {
                return 1.0f + ((float)Math.Pow(2, level - 1) / 10.0f) + (2 * (float)level / 10);
            } else
            {
                return 1.0f + (float)Math.Pow(2, level - 1) / 10.0f;
            }
        }

        #region Static Helpers

        public static Laurel GetLaurelFromLevel(int level)
        {
            if (level.IsWithin(0, 15)) return Laurel.Iron;
            if (level.IsWithin(16, 30)) return Laurel.Copper;
            if (level.IsWithin(31, 80)) return Laurel.Silver;
            if (level.IsWithin(81, 300)) return Laurel.Gold;
            if (level.IsWithin(301, 1000)) return Laurel.Diamond;

            return Laurel.Iron;
        }

        #endregion
    }
}
