using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HunterPie.Core.Definitions;
using HunterPie.Core.Enums;
using HunterPie.Core.Monsters;
using HunterPie.Logger;
using HunterPie.Memory;
using HunterPie.Core.Events;

namespace HunterPie.Core
{
    public class Monster
    {
        // Private vars
        private long monsterAddress;
        private string id;
        private float health;
        private float stamina;
        private bool isTarget;
        private int isSelect; //0 = None, 1 = This, 2 = Other
        private float enrageTimer = 0;
        private float sizeMultiplier;
        private int actionId;
        private AlatreonState alatreonElement;

        private long MonsterAddress
        {
            get => monsterAddress;
            set
            {
                if (value != monsterAddress)
                {
                    if (monsterAddress != 0)
                    {
                        Id = null;
                        FoundEnrageInMemory = false;
                    }
                    monsterAddress = value;
                }
            }
        }
        public int MonsterNumber { get; private set; }

        // Monster basic info
        public string Name => GStrings.GetMonsterNameByID(Id) ?? "Missing Translation";

        private MonsterInfo MonsterInfo => MonsterData.MonstersInfo[GameId];

        public string Id
        {
            get => id;
            private set
            {
                if (!string.IsNullOrEmpty(value) && id != value)
                {
                    if (Health > 0)
                    {
                        id = value;

                        GetMonsterWeaknesses();
                        IsAlive = true;
                        CreateMonsterParts(MonsterInfo.MaxParts);
                        GetMonsterPartsInfo();
                        GetMonsterAilments();
                        GetMonsterSizeModifier();
                        CaptureThreshold = MonsterInfo.Capture;

                        IsActuallyAlive = true;
                        _onMonsterSpawn();
                    }
                }
                else if (string.IsNullOrEmpty(value) && id != value)
                {

                    id = value;
                    _onMonsterDespawn();
                    DestroyParts();
                    IsAlive = IsActuallyAlive = false;
                }
            }
        }
        public int GameId { get; set; }
        public float SizeMultiplier
        {
            get => sizeMultiplier;
            private set
            {
                if (value <= 0) return;
                if (value != sizeMultiplier)
                {
                    sizeMultiplier = value;
                    Debugger.Debug($"{Name} Size multiplier: {sizeMultiplier}");
                    _onCrownChange();
                }
            }
        }
        public string Crown => MonsterInfo.GetCrownByMultiplier(SizeMultiplier);
        public float MaxHealth { get; private set; }
        public float Health
        {
            get => health;
            private set
            {
                if (value != health)
                {
                    health = value;
                    _onHPUpdate();
                    if (value <= 0)
                    {
                        // Clears monster ID since it's dead
                        Id = null;
                        IsActuallyAlive = IsAlive = false;
                        DestroyParts();
                        _onMonsterDeath();
                    }
                }
            }
        }
        public Dictionary<string, int> Weaknesses { get; private set; }
        public float HPPercentage { get; private set; } = 1;
        public bool IsTarget
        {
            get => isTarget;
            private set
            {
                if (value != isTarget)
                {
                    isTarget = value;
                    _onTargetted();
                }
            }
        }
        public int IsSelect
        {
            get => isSelect;
            set
            {
                if (value != isSelect)
                {
                    isSelect = value;
                    _onTargetted();
                }
            }
        }
        public bool IsAlive { get; private set; }
        public bool IsActuallyAlive { get; private set; }

        public float EnrageTimer
        {
            get => enrageTimer;
            private set
            {
                if (value != enrageTimer)
                {
                    if (value > 0 && enrageTimer == 0)
                    {
                        _onEnrage();
                    }
                    else if (value == 0 && enrageTimer > 0)
                    {
                        _onUnenrage();
                    }
                    enrageTimer = value;
                    _OnEnrageUpdateTimerUpdate();
                }
            }
        }
        public float EnrageTimerStatic { get; private set; }
        public bool IsEnraged => enrageTimer > 0;
        private bool FoundEnrageInMemory { get; set; }

        public float Stamina
        {
            get => stamina;
            private set
            {
                if ((int)value != (int)stamina)
                {
                    stamina = value;
                    _OnStaminaUpdate();
                }
            }
        }
        public float MaxStamina { get; private set; }

        public float CaptureThreshold { get; private set; }
        public bool IsCaptured { get; private set; }

        public readonly bool[] AliveMonsters = { false, false, false };

        public int ActionId
        {
            get => actionId;
            set
            {
                if (actionId != value)
                {
                    // Do event
                    actionId = value;
                    Debugger.Debug($"{Id} -> {ActionReferenceName} (Action: {value})");
                }
            }
        }

        public string ActionReferenceName { get; private set; }

        public AlatreonState AlatreonElement
        {
            get => alatreonElement;
            private set
            {
                if (value != alatreonElement)
                {
                    alatreonElement = value;
                    _OnAlatreonElementShift();
                }
            }
        }

        public List<Part> Parts = new List<Part>();
        public List<Ailment> Ailments = new List<Ailment>();
        // Threading
        ThreadStart monsterInfoScanRef;
        Thread monsterInfoScan;

        #region Events
        public delegate void MonsterEnrageEvents(object source, MonsterUpdateEventArgs args);
        public delegate void MonsterEvents(object source, EventArgs args);
        public delegate void MonsterSpawnEvents(object source, MonsterSpawnEventArgs args);
        public delegate void MonsterUpdateEvents(object source, MonsterUpdateEventArgs args);

        public event MonsterSpawnEvents OnMonsterSpawn;

        public event MonsterEvents OnMonsterAilmentsCreate;
        public event MonsterEvents OnMonsterDespawn;
        public event MonsterEvents OnMonsterDeath;
        public event MonsterEvents OnTargetted;
        public event MonsterEvents OnCrownChange;

        public event MonsterUpdateEvents OnHPUpdate;
        public event MonsterUpdateEvents OnStaminaUpdate;

        public event MonsterEnrageEvents OnEnrage;
        public event MonsterEnrageEvents OnUnenrage;
        public event MonsterEnrageEvents OnEnrageTimerUpdate;

        // Used ONLY by Alatreon
        public event MonsterEvents OnAlatreonElementShift;


        protected virtual void _onMonsterSpawn()
        {
            MonsterSpawnEventArgs args = new MonsterSpawnEventArgs(this);
            OnMonsterSpawn?.Invoke(this, args);
        }

        protected virtual void _onMonsterDespawn() => OnMonsterDespawn?.Invoke(this, EventArgs.Empty);

        protected virtual void _onMonsterDeath() => OnMonsterDeath?.Invoke(this, EventArgs.Empty);

        protected virtual void _onHPUpdate() => OnHPUpdate?.Invoke(this, new MonsterUpdateEventArgs(this));

        protected virtual void _onTargetted() => OnTargetted?.Invoke(this, EventArgs.Empty);

        protected virtual void _onCrownChange() => OnCrownChange?.Invoke(this, EventArgs.Empty);

        protected virtual void _onEnrage() => OnEnrage?.Invoke(this, new MonsterUpdateEventArgs(this));

        protected virtual void _onUnenrage() => OnUnenrage?.Invoke(this, new MonsterUpdateEventArgs(this));

        protected virtual void _OnEnrageUpdateTimerUpdate() => OnEnrageTimerUpdate?.Invoke(this, new MonsterUpdateEventArgs(this));

        protected virtual void _OnStaminaUpdate() => OnStaminaUpdate?.Invoke(this, new MonsterUpdateEventArgs(this));

        protected virtual void _OnAlatreonElementShift() => OnAlatreonElementShift?.Invoke(this, EventArgs.Empty);

        protected virtual void _OnMonsterAilmentsCreate() => OnMonsterAilmentsCreate?.Invoke(this, EventArgs.Empty);
        #endregion

        public Monster(int initMonsterNumber) => MonsterNumber = initMonsterNumber;

        ~Monster()
        {
            Id = null;
            Weaknesses?.Clear();
        }

        public void StartThreadingScan()
        {
            monsterInfoScanRef = new ThreadStart(ScanMonsterInfo);
            monsterInfoScan = new Thread(monsterInfoScanRef)
            {
                Name = $"Kernel_Monster.{MonsterNumber}"
            };
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_MONSTER_SCANNER_INITIALIZED']").Replace("{MonsterNumber}", MonsterNumber.ToString()));
            monsterInfoScan.Start();
        }

        public void StopThread() => monsterInfoScan.Abort();

        private void ScanMonsterInfo()
        {
            while (Kernel.GameIsRunning)
            {
                GetMonsterAddress();
                GetMonsterId();
                GetMonsterSizeModifier();
                GetMonsterStamina();
                GetMonsterAilments();
                GetMonsterPartsInfo();
                GetPartsTenderizeInfo();
                GetMonsterAction();
                GetMonsterEnrageTimer();
                GetTargetMonsterAddress();
                GetAlatreonCurrentElement();
                Thread.Sleep(UserSettings.PlayerConfig.Overlay.GameScanDelay);
            }
            Thread.Sleep(1000);
            ScanMonsterInfo();
        }

        /// <summary>
        /// Removes all Parts and Ailments from this monster. This should be called whenever the monster despawns
        /// </summary>
        private void DestroyParts()
        {
            foreach (Part monsterPart in Parts)
            {
                monsterPart.Destroy();
            }
            Parts.Clear();
            foreach (Ailment monsterAilment in Ailments)
            {
                monsterAilment.Destroy();
            }
            Ailments.Clear();
        }

        private void GetMonsterAddress()
        {
            long Address = Memory.Address.BASE + Memory.Address.MONSTER_OFFSET;
            // This will give us the third monster's address, so we can find the second and first monster with it
            long ThirdMonsterAddress = Kernel.ReadMultilevelPtr(Address, Memory.Address.Offsets.MonsterOffsets);
            switch (MonsterNumber)
            {
                case 3:
                    MonsterAddress = ThirdMonsterAddress;
                    break;
                case 2:
                    MonsterAddress = Kernel.Read<long>(ThirdMonsterAddress - 0x30) + 0x40;
                    break;
                case 1:
                    MonsterAddress = Kernel.Read<long>(Kernel.Read<long>(ThirdMonsterAddress - 0x30) + 0x10) + 0x40;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets the current monster health and max health.
        /// </summary>
        private void GetMonsterHealth()
        {
            long MonsterHealthPtr = Kernel.Read<long>(MonsterAddress + Address.Offsets.MonsterHPComponentOffset);
            float MonsterTotalHealth = Kernel.Read<float>(MonsterHealthPtr + 0x60);
            float MonsterCurrentHealth = Kernel.Read<float>(MonsterHealthPtr + 0x64);

            if (MonsterCurrentHealth <= MonsterTotalHealth && MonsterTotalHealth > 0)
            {
                MaxHealth = MonsterTotalHealth;
                Health = MonsterCurrentHealth;
                HPPercentage = Health / MaxHealth == 0 ? 1 : Health / MaxHealth;
            }
            else
            {
                MaxHealth = 0;
                Health = 0;
                HPPercentage = 1;
            }
        }

        /// <summary>
        /// Gets the current monster EM and GameId
        /// </summary>
        private void GetMonsterId()
        {
            long NamePtr = Kernel.Read<long>(MonsterAddress + Address.Offsets.MonsterNamePtr);
            string MonsterEm = Kernel.ReadString(NamePtr + 0x0C, 64);
            if (!string.IsNullOrEmpty(MonsterEm))
            {
                // Validates the em string
                string[] MonsterEmParsed = MonsterEm.Split('\\');
                if (MonsterEmParsed.ElementAtOrDefault(3) == null)
                {
                    Id = null;
                    return;
                }

                MonsterEm = MonsterEmParsed.LastOrDefault();
                if (MonsterEm.StartsWith("em"))
                {
                    GameId = Kernel.Read<int>(MonsterAddress + Address.Offsets.MonsterGameIDOffset);

                    if (!MonsterData.MonstersInfo.ContainsKey(GameId))
                    {
                        if (!MonsterEm.StartsWith("ems"))
                        {
                            Debugger.Error($"Unknown Monster Detected: ID:{GameId} | ems: {MonsterEm}");
                        }
                        Id = null;
                        Health = 0;
                        MaxHealth = 0;
                        HPPercentage = 1;
                        return;
                    }
                    else
                    {
                        GetMonsterHealth();
                        if (Id != MonsterInfo.Em && Health > 0) Debugger.Debug($"Found new monster ID: {GameId} ({MonsterEm}) #{MonsterNumber} @ 0x{MonsterAddress:X}");
                        Id = MonsterInfo.Em;
                        return;
                    }
                }
            }
            Id = null;

        }

        /// <summary>
        /// Gets the monster action
        /// </summary>
        private void GetMonsterAction()
        {
            if (!IsAlive)
            {
                return;
            }
            // This is our rcx
            long actionPointer = MonsterAddress + 0x61C8;
            

            // This will give us the action id, we'll need it later on to get the action reference name
            int actionId = Kernel.Read<int>(actionPointer + 0xB0);

            // mov      rax,[rcx+rax*8+68] ;our rax is pretty much always 2
            actionPointer = Kernel.Read<long>(actionPointer + (2 * 8) + 0x68);
            
            // mov      rax,[rax+rdx*8]
            actionPointer = Kernel.Read<long>(actionPointer + actionId * 8);
            actionPointer = Kernel.Read<long>(actionPointer);

            // call     qword ptr [r8 + 20] ;r8 is our actionPointer
            actionPointer = Kernel.Read<long>(actionPointer + 0x20);
            uint actionOffset = Kernel.Read<uint>(actionPointer + 3);

            // lea      rax,[actionRef]
            long actionRef = actionPointer + actionOffset + 7;

            // cmp      [rax+08],rcx
            actionRef = Kernel.Read<long>(actionRef + 8);
            string actionRefString = Kernel.ReadString(actionRef, 64);

            ActionReferenceName = actionRefString;
            ActionId = actionId;
        }

        /// <summary>
        /// Gets monster size
        /// </summary>
        private void GetMonsterSizeModifier()
        {
            if (!IsAlive) return;
            float SizeModifier = Kernel.Read<float>(MonsterAddress + 0x7730);
            if (SizeModifier <= 0 || SizeModifier >= 2) SizeModifier = 1;
            SizeMultiplier = Kernel.Read<float>(MonsterAddress + 0x188) / SizeModifier;
        }

        /// <summary>
        /// Builds monster weakness dictionary
        /// </summary>
        private void GetMonsterWeaknesses() => Weaknesses = MonsterInfo.Weaknesses.ToDictionary(w => w.Id, w => w.Stars);

        private void GetMonsterEnrageTimer()
        {
            if (!IsAlive) return;

            sMonsterStatus enrage = Kernel.ReadStructure<sMonsterStatus>(MonsterAddress + 0x1BE30);
            EnrageTimer = enrage.Duration;
            EnrageTimerStatic = enrage.MaxDuration;

            if (!FoundEnrageInMemory && Ailments.Count > 0)
            {
                AilmentInfo info = MonsterData.GetAilmentInfoById(999);
                Ailment enrageTracker = new Ailment(MonsterAddress + 0x1BE30)
                {
                    Name = GStrings.GetAilmentNameByID(info.Name),
                    Group = info.Group,
                    Type = AilmentType.Status
                };
                enrageTracker.SetAilmentInfo(sMonsterStatus.Convert(enrage), 999);
                Ailments.Add(enrageTracker);
                FoundEnrageInMemory = true;
            }
            
        }

        private void GetTargetMonsterAddress()
        {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.UseLockonInsteadOfPin)
            {
                Int64 LockonAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.EQUIPMENT_OFFSET, Address.Offsets.PlayerLockonOffsets);
                // This will give us the monster target index
                int MonsterLockonIndex = Kernel.Read<int>(LockonAddress - 0x7C);
                if (MonsterLockonIndex == -1)
                {
                    IsTarget = false;
                    IsSelect = 0;
                    return;
                }
                // And this one will give us the actual monster index in that target slot
                LockonAddress = LockonAddress - 0x7C - 0x19F8;
                int MonsterIndexInSlot = Kernel.Read<int>(LockonAddress + (MonsterLockonIndex * 8));
                if (MonsterIndexInSlot > 2 || MonsterIndexInSlot < 0)
                {
                    IsTarget = false;
                    IsSelect = 0;
                    return;
                }
                // And then we get then we can finally get the monster index
                List<int> MonsterSlotIndexes = new List<int>();
                for (int i = 0; i < 3; i++) MonsterSlotIndexes.Add(Kernel.Read<int>(LockonAddress + 0x6C + (4 * i)));
                int[] MonsterIndexes = MonsterSlotIndexes.ToArray();
                if (MonsterIndexes[2] == -1 && MonsterIndexes[1] == -1 && AliveMonsters.Where(v => v == true).Count() == 1)
                {
                    IsTarget = IsAlive;
                }
                else if (MonsterIndexes[2] == -1 && MonsterIndexes[1] != -1 && AliveMonsters.Where(v => v == true).Count() == 2)
                {
                    if (!AliveMonsters[0])
                    {
                        IsTarget = MonsterIndexes[MonsterIndexInSlot] + 1 == (MonsterNumber - 1);
                    }
                    else if (!AliveMonsters[1])
                    {
                        IsTarget = MonsterIndexes[MonsterIndexInSlot] * 2 == (MonsterNumber - 1);
                    }
                    else
                    {
                        IsTarget = MonsterIndexes[MonsterIndexInSlot] == (MonsterNumber - 1);
                    }

                }
                else
                {
                    IsTarget = MonsterIndexes[MonsterIndexInSlot] == (MonsterNumber - 1);
                }
                IsSelect = IsTarget ? 1 : 2;
            }
            else
            {
                Int64 TargettedMonsterAddress = Kernel.ReadMultilevelPtr(Address.BASE + Address.MONSTER_SELECTED_OFFSET, Address.Offsets.MonsterSelectedOffsets);
                Int64 selectedPtr = Kernel.Read<long>(Address.BASE + Address.MONSTER_TARGETED_OFFSET); //probably want an offset for this
                bool isSelect = Kernel.Read<long>(selectedPtr + 0x128) != 0x0 && Kernel.Read<long>(selectedPtr + 0x130) != 0x0 && Kernel.Read<long>(selectedPtr + 0x160) != 0x0;
                Int64 SelectedMonsterAddress = Kernel.Read<long>(selectedPtr + 0x148);
                IsTarget = TargettedMonsterAddress == 0 ? SelectedMonsterAddress == MonsterAddress : TargettedMonsterAddress == MonsterAddress;

                if (!isSelect)
                {
                    IsSelect = 0; // nothing is selected
                }
                else if (IsTarget)
                {
                    IsSelect = 1; // this monster is selected
                }
                else
                {
                    IsSelect = 2; // another monster is selected
                }
            }

        }

        /// <summary>
        /// Creates monster parts based on how many parts it has in MonsterData.xml
        /// </summary>
        /// <param name="numberOfParts">Number of parts</param>
        private void CreateMonsterParts(int numberOfParts)
        {
            for (int i = 0; i < numberOfParts; i++)
            {
                Part part = new Part(MonsterInfo, MonsterInfo.Parts[i], i);
                Parts.Add(part);
            }
        }

        private void GetMonsterPartsInfo()
        {
            if (!IsAlive) return;

            long MonsterPartPtr = Kernel.Read<long>(MonsterAddress + 0x1D058);

            // If the Monster Part Ptr is still 0, then the monster hasn't fully spawn yet
            if (MonsterPartPtr == 0x00000000) return;

            long MonsterPartAddress = MonsterPartPtr + 0x40;
            long MonsterRemovablePartAddress = MonsterPartPtr + 0x1FC8;

            int NormalPartIndex = 0;
            uint RemovablePartIndex = 0;

            for (int pIndex = 0; pIndex < MonsterInfo.MaxParts; pIndex++)
            {
                Part CurrentPart = Parts[pIndex];
                PartInfo CurrentPartInfo = MonsterInfo.Parts[pIndex];

                if (CurrentPartInfo.IsRemovable)
                {
                    if (CurrentPart.Address > 0)
                    {
                        sMonsterRemovablePart MonsterRemovablePartData = Kernel.ReadStructure<sMonsterRemovablePart>(CurrentPart.Address);

                        // Alatreon explosion level
                        if (GameId == 87 && MonsterRemovablePartData.unk3.Index == 3)
                        {
                            MonsterRemovablePartData.Data.Counter = Kernel.Read<int>(MonsterAddress + 0x20920);
                        }

                        CurrentPart.SetPartInfo(MonsterRemovablePartData.Data);
                    }
                    else
                    {
                        while (MonsterRemovablePartAddress < MonsterRemovablePartAddress + 0x120 * 32)
                        {
                            // Every 15 parts there's a 8 bytes gap between the old removable part block
                            // and the next part block
                            if (Kernel.Read<long>(MonsterRemovablePartAddress) <= 0xA0)
                            {
                                MonsterRemovablePartAddress += 0x8;
                            }
                            sMonsterRemovablePart MonsterRemovablePartData = Kernel.ReadStructure<sMonsterRemovablePart>(MonsterRemovablePartAddress);

                            if (CurrentPartInfo.Skip || (MonsterRemovablePartData.unk3.Index == CurrentPartInfo.Index && MonsterRemovablePartData.Data.MaxHealth > 0))
                            {

                                CurrentPart.Address = MonsterRemovablePartAddress;
                                CurrentPart.IsRemovable = true;
                                CurrentPart.SetPartInfo(MonsterRemovablePartData.Data);

                                Debugger.Debug($"Removable Part Structure <{Name}> ({CurrentPart.Name}) [0x{MonsterRemovablePartAddress:X}]");
                                RemovablePartIndex++;
                                do
                                {
                                    MonsterRemovablePartAddress += 0x78;
                                } while (MonsterRemovablePartData.Equals(Kernel.ReadStructure<sMonsterRemovablePart>(MonsterRemovablePartAddress)));

                                break;
                            }
                            MonsterRemovablePartAddress += 0x78;
                        }
                    }
                }
                else
                {
                    if (CurrentPart.Address > 0)
                    {
                        sMonsterPart MonsterPartData = Kernel.ReadStructure<sMonsterPart>(CurrentPart.Address);
                        CurrentPart.SetPartInfo(MonsterPartData.Data);

                    }
                    else
                    {
                        sMonsterPart MonsterPartData = Kernel.ReadStructure<sMonsterPart>(MonsterPartAddress + (NormalPartIndex * 0x1F8));
                        CurrentPart.Address = MonsterPartAddress + (NormalPartIndex * 0x1F8);
                        CurrentPart.Group = CurrentPartInfo.GroupId;
                        CurrentPart.TenderizedIds = CurrentPartInfo.TenderizeIds;

                        CurrentPart.SetPartInfo(MonsterPartData.Data);

                        Debugger.Debug($"Part Structure <{Name}> ({CurrentPart.Name}) [0x{CurrentPart.Address:X}]");

                        NormalPartIndex++;
                    }
                }
            }
        }

        private void GetMonsterStamina()
        {
            if (!IsAlive) return;
            long MonsterStaminaAddress = MonsterAddress + 0x1C0F0;
            MaxStamina = Kernel.Read<float>(MonsterStaminaAddress + 0x4);
            float stam = Kernel.Read<float>(MonsterStaminaAddress);
            Stamina = stam <= MaxStamina ? stam : MaxStamina;
        }

        private void GetMonsterAilments()
        {
            if (!IsAlive) return;

            if (Ailments.Count > 0)
            {
                foreach (Ailment ailment in Ailments)
                {
                    sMonsterAilment updatedData;
                    switch (ailment.Type)
                    {
                        case AilmentType.Status:
                            sMonsterStatus updatedStatus = Kernel.ReadStructure<sMonsterStatus>(ailment.Address);
                            updatedData = sMonsterStatus.Convert(updatedStatus);
                            break;
                        default:
                            updatedData = Kernel.ReadStructure<sMonsterAilment>(ailment.Address);
                            break;
                    }
                     
                    ailment.SetAilmentInfo(updatedData);
                }

            }
            else
            {
                long MonsterAilmentListPtrs = MonsterAddress + 0x1BC40;
                long MonsterAilmentPtr = Kernel.Read<long>(MonsterAilmentListPtrs);
                while (MonsterAilmentPtr > 1)
                {

                    // There's a gap of 0x148 bytes between the pointer and the sMonsterAilment structure
                    sMonsterAilment AilmentData = Kernel.ReadStructure<sMonsterAilment>(MonsterAilmentPtr + 0x148);

                    if ((int)AilmentData.Id > MonsterData.AilmentsInfo.Count)
                    {
                        MonsterAilmentListPtrs += sizeof(long);
                        MonsterAilmentPtr = Kernel.Read<long>(MonsterAilmentListPtrs);
                        continue;
                    }

                    var AilmentInfo = MonsterData.GetAilmentInfoById(AilmentData.Id);
                    // Check if this Ailment can be skipped and therefore not be tracked at all
                    bool SkipElderDragonTrap = MonsterInfo.Capture == 0 && AilmentInfo.Group == "TRAP";
                    if (SkipElderDragonTrap || (AilmentInfo.CanSkip && !UserSettings.PlayerConfig.HunterPie.Debug.ShowUnknownStatuses))
                    {
                        MonsterAilmentListPtrs += sizeof(long);
                        MonsterAilmentPtr = Kernel.Read<long>(MonsterAilmentListPtrs);
                        continue;
                    }

                    Ailment MonsterAilment = new Ailment(MonsterAilmentPtr + 0x148)
                    {
                        Name = GStrings.GetAilmentNameByID(AilmentInfo.Name),
                        Group = AilmentInfo.Group,
                        Type = AilmentType.Ailment
                    };
                    MonsterAilment.SetAilmentInfo(AilmentData);

                    Debugger.Debug($"sMonsterAilment <{Name}> ({MonsterAilment.Name}) [0x{MonsterAilmentPtr + 0x148:X}]");

                    Ailments.Add(MonsterAilment);
                    MonsterAilmentListPtrs += sizeof(long);
                    MonsterAilmentPtr = Kernel.Read<long>(MonsterAilmentListPtrs);
                }
                // Depending on the scan delay, the OnMonsterSpawn event can be dispatched before the ailments are created.
                // To fix that, we dispatch a OnMonsterAilmentsCreate too.
                if (IsActuallyAlive && Ailments.Count > 0) _OnMonsterAilmentsCreate();
            }
        }

        private void GetAlatreonCurrentElement()
        {
            bool IsAlatreon = GameId == 87;
            if (!IsAlive || !IsAlatreon) return;

            int alatreonElement = Kernel.Read<int>(MonsterAddress + 0x20910);

            if (alatreonElement <= 3 && alatreonElement > 0)
            {
                AlatreonElement = (AlatreonState)alatreonElement;
            }

        }

        private void GetPartsTenderizeInfo()
        {
            if (!IsAlive || Parts.Count == 0) return;

            for (uint i = 0; i < 10; i++)
            {
                sTenderizedPart tenderizedData = Kernel.ReadStructure<sTenderizedPart>(MonsterAddress + 0x1C458 + (i * 0x40));

                if (tenderizedData.PartId != uint.MaxValue)
                {
                    //Debugger.Debug(tenderizedData.PartId);
                    foreach (Part validPart in Parts.Where(p => p.TenderizedIds != null && p.TenderizedIds.Contains(tenderizedData.PartId)))
                    {
                        validPart.SetTenderizeInfo(tenderizedData);
                    }
                }
            }
        }

    }
}
