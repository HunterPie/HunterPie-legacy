using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using HunterPie.Core.Monsters;
using HunterPie.Memory;
using HunterPie.Logger;


namespace HunterPie.Core {
    public class Monster {
        // Private vars
        private string _id;
        private float _currentHP;
        private float _Stamina;
        private bool _IsTarget;
        private int _IsSelect; //0 = None, 1 = This, 2 = Other
        private float _enrageTimer = 0;
        private float _SizeMultiplier;

        private Int64 MonsterAddress;
        public int MonsterNumber { get; private set; }

        // Monster basic info
        public string Name => GStrings.GetMonsterNameByID(Id);

        private MonsterInfo MonsterInfo => MonsterData.MonstersInfo[GameId];

        public string Id {
            get => _id;
            set {
                if (value != null && _id != value) {
                    if (CurrentHP > 0) {
                        _id = value;
                        // Static stuff that can be scanned only once
                        GetMonsterWeaknesses();
                        this.IsAlive = true;
                        CreateMonsterParts(MonsterInfo.MaxParts);
                        GetMonsterParts();
                        Ailments.Clear();
                        // For whatever reason, in Guiding Lands some monsters
                        // take some time to load their ailments in memory
                        GetMonsterAilments();
                        while (Ailments.Count == 0)
                        {
                            GetMonsterAilments();
                            Thread.Sleep(500);
                        }
                            
                        GetMonsterSizeModifier();
                        CaptureThreshold = MonsterInfo.Capture;
                        // Only call this if monster is actually alive
                        IsActuallyAlive = true;
                        _onMonsterSpawn();
                    }
                } else if (value == null && _id != value) {
                    _id = value;
                    this.HPPercentage = 1f;
                    this.IsTarget = false;
                    this.IsActuallyAlive = this.IsAlive = false;
                    _onMonsterDespawn();
                    Weaknesses.Clear();
                }
            }
        }
        public int GameId { get; set; }
        public float SizeMultiplier {
            get { return _SizeMultiplier; }
            set {
                if (value <= 0) return;
                if (value != _SizeMultiplier) {
                    _SizeMultiplier = value;
                    Debugger.Debug($"{Name} Size multiplier: {_SizeMultiplier}");
                    _onCrownChange();
                }
            }
        }
        public string Crown {
            get { return  MonsterInfo.GetCrownByMultiplier(SizeMultiplier); }
        }
        public float TotalHP { get; private set; }
        public float CurrentHP {
            get { return _currentHP; }
            set {
                if (value != _currentHP) {
                    _currentHP = value;
                    _onHPUpdate();
                    if (value <= 0) {
                        // Clears monster ID since it's dead
                        Id = null;
                        IsActuallyAlive = IsAlive = false;
                        _onMonsterDeath();
                    }
                }
            }
        }
        public Dictionary<string, int> Weaknesses { get; private set; }
        public float HPPercentage { get; private set; } = 1;
        public bool IsTarget {
            get { return _IsTarget; }
            set {
                if (value != _IsTarget) {
                    _IsTarget = value;
                    _onTargetted();
                }
            }
        }
        public int IsSelect
        {
            get { return _IsSelect; }
            set {
                if (value != _IsSelect) {
                    _IsSelect = value;
                    _onTargetted();
                }
            }
        }
        public bool IsAlive = false;
        public bool IsActuallyAlive;
        public float EnrageTimer {
            get { return _enrageTimer; }
            set {
                if (value != _enrageTimer) {
                    if (value > 0 && _enrageTimer == 0) {
                        _onEnrage();
                    } else if (value == 0 && _enrageTimer > 0) {
                        _onUnenrage();
                    }
                    _enrageTimer = value;
                    _OnEnrageUpdateTimerUpdate();
                }
            }
        }
        public float CaptureThreshold { get; private set; }
        public float EnrageTimerStatic { get; private set; }
        public bool IsEnraged {
            get { return _enrageTimer > 0; }
        }
        public float Stamina {
            get { return _Stamina; }
            set {
                if ((int)value != (int)_Stamina) {
                    _Stamina = value;
                    _OnStaminaUpdate();
                }
            }
        }
        public float MaxStamina { get; private set; }

        public List<Part> Parts = new List<Part>();
        public List<Ailment> Ailments = new List<Ailment>();
        // Threading
        ThreadStart MonsterInfoScanRef;
        Thread MonsterInfoScan;

        // Game events
        public delegate void MonsterEnrageEvents(object source, MonsterUpdateEventArgs args);
        public delegate void MonsterEvents(object source, EventArgs args);
        public delegate void MonsterSpawnEvents(object source, MonsterSpawnEventArgs args);
        public delegate void MonsterUpdateEvents(object source, MonsterUpdateEventArgs args);
        public event MonsterSpawnEvents OnMonsterSpawn;
        public event MonsterEvents OnMonsterDespawn;
        public event MonsterEvents OnMonsterDeath;
        public event MonsterUpdateEvents OnHPUpdate;
        public event MonsterUpdateEvents OnStaminaUpdate;
        public event MonsterEvents OnTargetted;
        public event MonsterEnrageEvents OnEnrage;
        public event MonsterEnrageEvents OnUnenrage;
        public event MonsterEnrageEvents OnEnrageTimerUpdate;
        public event MonsterEvents OnCrownChange;


        protected virtual void _onMonsterSpawn() {
            MonsterSpawnEventArgs args = new MonsterSpawnEventArgs(this);
            OnMonsterSpawn?.Invoke(this, args);
        }

        protected virtual void _onMonsterDespawn() {
            OnMonsterDespawn?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onMonsterDeath() {
            OnMonsterDeath?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onHPUpdate() {
            OnHPUpdate?.Invoke(this, new MonsterUpdateEventArgs(this));
        }

        protected virtual void _onTargetted() {
            OnTargetted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onCrownChange() {
            OnCrownChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void _onEnrage() {
            OnEnrage?.Invoke(this, new MonsterUpdateEventArgs(this));
        }

        protected virtual void _onUnenrage() {
            OnUnenrage?.Invoke(this, new MonsterUpdateEventArgs(this));
        }

        protected virtual void _OnEnrageUpdateTimerUpdate() {
            OnEnrageTimerUpdate?.Invoke(this, new MonsterUpdateEventArgs(this));
        }

        protected virtual void _OnStaminaUpdate() {
            OnStaminaUpdate?.Invoke(this, new MonsterUpdateEventArgs(this));
        }

        public Monster(int initMonsterNumber) {
            MonsterNumber = initMonsterNumber;
        }

        ~Monster() {
            Id = null;
            Weaknesses?.Clear();
        }

        public void StartThreadingScan() {
            MonsterInfoScanRef = new ThreadStart(ScanMonsterInfo);
            MonsterInfoScan = new Thread(MonsterInfoScanRef) {
                Name = $"Scanner_Monster.{MonsterNumber}"
            };
            Debugger.Warn(GStrings.GetLocalizationByXPath("/Console/String[@ID='MESSAGE_MONSTER_SCANNER_INITIALIZED']").Replace("{MonsterNumber}", MonsterNumber.ToString()));
            MonsterInfoScan.Start();
        }

        public void StopThread() {
            MonsterInfoScan.Abort();
        }

        private void ScanMonsterInfo() {
            while (Scanner.GameIsRunning) {
                GetMonsterAddress();
                GetMonsterIDAndName();
                GetMonsterSizeModifier();
                GetMonsterStamina();
                GetMonsterAilments();
                GetMonsterParts();
                GetMonsterEnrageTimer();
                GetTargetMonsterAddress();
                Thread.Sleep(Math.Max(50, UserSettings.PlayerConfig.Overlay.GameScanDelay));
            }
            Thread.Sleep(1000);
            ScanMonsterInfo();
        }

        public void ClearParts() {
            Parts.Clear();
            Ailments.Clear();
#if DEBUG
            Debugger.Log($"Cleared parts: {Parts.Count} | {Ailments.Count}");
#endif
        }

        private void GetMonsterAddress() {
            Int64 Address = Memory.Address.BASE + Memory.Address.MONSTER_OFFSET;
            // This will give us the third monster's address, so we can find the second and first monster with it
            Int64 ThirdMonsterAddress = Scanner.READ_MULTILEVEL_PTR(Address, Memory.Address.Offsets.MonsterOffsets);
            switch (MonsterNumber) {
                case 3:
                    MonsterAddress = ThirdMonsterAddress;
                    break;
                case 2:
                    MonsterAddress = Scanner.Read<long>(ThirdMonsterAddress - 0x30) + 0x40;
                    break;
                case 1:
                    MonsterAddress = Scanner.Read<long>(Scanner.Read<long>(ThirdMonsterAddress - 0x30) + 0x10) + 0x40;
                    break;
                default:
                    break;
            }
        }

        private void GetMonsterHp(string MonsterModel) {
            Int64 MonsterHPComponent = Scanner.Read<long>(this.MonsterAddress + Address.Offsets.MonsterHPComponentOffset);
            Int64 MonsterTotalHPAddress = MonsterHPComponent + 0x60;
            Int64 MonsterCurrentHPAddress = MonsterTotalHPAddress + 0x4;
            float f_TotalHP = Scanner.Read<float>(MonsterTotalHPAddress);
            float f_CurrentHP = Scanner.Read<float>(MonsterCurrentHPAddress);
            if ((MonsterModel != null) && f_CurrentHP <= f_TotalHP && f_CurrentHP > 0 && !MonsterModel.StartsWith("ems")) {
                this.TotalHP = f_TotalHP;
                this.CurrentHP = f_CurrentHP;
                this.HPPercentage = f_CurrentHP / f_TotalHP == 0 ? 1 : f_CurrentHP / f_TotalHP;
            } else {
                this.TotalHP = 0.0f;
                this.CurrentHP = 0.0f;
                this.HPPercentage = 1f;
            }
        }

        private void GetMonsterIDAndName() {
            Int64 NamePtr = Scanner.Read<long>(this.MonsterAddress + Address.Offsets.MonsterNamePtr);
            string MonsterId = Scanner.READ_STRING(NamePtr + 0x0c, 64).Replace("\x00", "");
            if (MonsterId != "") {
                string[] MonsterID = MonsterId.Split('\\');
                if (MonsterID.Length < 4) {
                    Id = null;
                    return;
                }
                MonsterId = MonsterID.LastOrDefault()?.Trim('\x00');
                GetMonsterHp(MonsterId);
                if (MonsterId.StartsWith("em") && !MonsterId.StartsWith("ems")) {

                    GameId = Scanner.Read<int>(MonsterAddress + Address.Offsets.MonsterGameIDOffset);

                    MonsterId = MonsterInfo.Em;

                    if (MonsterId != Id && CurrentHP > 0) Debugger.Debug($"Found new monster ID: {Scanner.READ_STRING(NamePtr + 0x0c, 64).Replace("\x00", "")} #{MonsterNumber} @ 0x{MonsterAddress:X}");
                    Id = MonsterId;
                    return;
                }
            }
            Id = null;
            return;
        }

        private void GetMonsterSizeModifier() {
            if (!IsAlive) return;
            float SizeModifier = Scanner.Read<float>(MonsterAddress + 0x7730);
            if (SizeModifier <= 0 || SizeModifier >= 2) SizeModifier = 1;
            SizeMultiplier = Scanner.Read<float>(MonsterAddress + 0x188) / SizeModifier;
        }

        private void GetMonsterWeaknesses() {
            Weaknesses = MonsterInfo.Weaknesses.ToDictionary(w => w.Id, w => w.Stars);
        }

        private void GetMonsterEnrageTimer() {
            EnrageTimer = Scanner.Read<float>(MonsterAddress + 0x1BE54);
            EnrageTimerStatic = Scanner.Read<float>(MonsterAddress + 0x1BE54 + 0x4);
        }

        private void GetTargetMonsterAddress() {
            Int64 TargettedMonsterAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.MONSTER_SELECTED_OFFSET, Address.Offsets.MonsterSelectedOffsets);

            Int64 selectedPtr = Scanner.Read<long>(Address.BASE + Address.MONSTER_TARGETED_OFFSET); //probably want an offset for this
            bool isSelect = Scanner.Read<long>(selectedPtr + 0x128) != 0x0 && Scanner.Read<long>(selectedPtr + 0x130) != 0x0 && Scanner.Read<long>(selectedPtr + 0x160) != 0x0;
            Int64 SelectedMonsterAddress = Scanner.Read<long>(selectedPtr + 0x148);
            IsTarget = TargettedMonsterAddress == 0 ? SelectedMonsterAddress == this.MonsterAddress : TargettedMonsterAddress == this.MonsterAddress;

            if (!isSelect) {
                this.IsSelect = 0; // nothing is selected
            } else if (IsTarget) {
                this.IsSelect = 1; // this monster is selected
            } else {
                this.IsSelect = 2; // another monster is selected
            }
        }

        private void CreateMonsterParts(int numberOfParts) {
            Parts.Clear();
            for (int i = 0; i < numberOfParts; i++) {
                Part part = new Part(MonsterInfo, MonsterInfo.Parts[i], i);
                Parts.Add(part);
            }
        }

        private void GetMonsterParts()
        {
            if (!this.IsAlive) return;
            long monsterPartAddress = MonsterAddress + Address.Offsets.MonsterPartsOffset + Address.Offsets.FirstMonsterPartOffset;
            int partsCount = MonsterInfo.MaxParts;
            int nRemovableParts = MonsterInfo.MaxRemovableParts;
            byte TimesBroken;
            float Health;
            float MaxHealth;

            for (int partId = 0; partId < partsCount; partId++)
            {
                Part part = Parts[partId];
                PartInfo partInfo = MonsterInfo.Parts[partId];
                if (partInfo.IsRemovable)
                {

                    if (part.PartAddress > 0)
                    {
                        TimesBroken = Scanner.Read<byte>(part.PartAddress + 0x18);
                        MaxHealth = Scanner.Read<float>(part.PartAddress + 0x10);
                        Health = Scanner.Read<float>(part.PartAddress + 0x0C);

                        part.SetPartInfo(TimesBroken, MaxHealth, Health);
                    }
                    else
                    {
                        Int64 removablePartAddress = MonsterAddress + Address.Offsets.RemovablePartsOffset;
                        for (int removablePartIndex = 0; removablePartIndex < 32; removablePartIndex++)
                        {
                            if (Scanner.Read<int>(removablePartAddress) <= 10)
                            {
                                removablePartAddress += 8;
                            }

                            bool IsAValidPart = Scanner.Read<int>(removablePartAddress + 0x6C) < nRemovableParts;

                            if (IsAValidPart && Scanner.Read<int>(removablePartAddress + 0x0C) > 0)
                            {
                                TimesBroken = Scanner.Read<byte>(removablePartAddress + 0x18);
                                MaxHealth = Scanner.Read<float>(removablePartAddress + 0x10);
                                Health = Scanner.Read<float>(removablePartAddress + 0x0C);

                                part.SetPartInfo(TimesBroken, MaxHealth, Health);
                                part.PartAddress = removablePartAddress;
                                part.IsRemovable = true;

                                // Some monsters have the same removable part value in the next removable part struct
                                // so we skip the ones with the same values.
                                while (
                                    Scanner.Read<float>(removablePartAddress + Address.Offsets.NextRemovablePart + 0x0C) == Health &&
                                    Scanner.Read<float>(removablePartAddress + Address.Offsets.NextRemovablePart + 0x10) == MaxHealth &&
                                    Scanner.Read<int>(removablePartAddress + Address.Offsets.NextRemovablePart + 0x8) == Scanner.Read<int>(removablePartAddress + 0x8))
                                {
                                    removablePartAddress += Address.Offsets.NextRemovablePart;
                                }

                                removablePartAddress += Address.Offsets.NextRemovablePart;
                                break;
                            }

                            removablePartAddress += Address.Offsets.NextRemovablePart;
                        }
                    }
                    continue;
                }

                TimesBroken = Scanner.Read<byte>(monsterPartAddress + Address.Offsets.MonsterPartBrokenCounterOffset);
                MaxHealth = Scanner.Read<float>(monsterPartAddress + 0x4); // Total health is 4 bytes ahead
                Health = Scanner.Read<float>(monsterPartAddress);

                part.SetPartInfo(TimesBroken, MaxHealth, Health);

                if (part.Group == null) part.Group = partInfo.GroupId;
                monsterPartAddress += Address.Offsets.NextMonsterPartOffset;
            }
        }

        private void GetMonsterStamina() {
            if (!IsAlive) return;
            Int64 MonsterStaminaAddress = MonsterAddress + 0x1C0F0;
            MaxStamina = Scanner.Read<float>(MonsterStaminaAddress + 0x4);
            float stam = Scanner.Read<float>(MonsterStaminaAddress);
            Stamina = stam <= MaxStamina ? stam : MaxStamina;
        }

        private void GetMonsterAilments() {
            if (!this.IsAlive) return;
            if (Ailments.Count > 0) {
                foreach (Ailment status in Ailments)
                {
                    if (status.Address == 0) continue;

                    float maxBuildup = Math.Max(0, Scanner.Read<float>(status.Address + 0x1C8));
                    float currentBuildup = Math.Max(0, Scanner.Read<float>(status.Address + 0x1B8));
                    float maxDuration = Math.Max(0, Scanner.Read<float>(status.Address + 0x19C));
                    float currentDuration = Math.Max(0, Scanner.Read<float>(status.Address + 0x1F8));
                    byte counter = Scanner.Read<byte>(status.Address + 0x200);

                    status.SetAilmentInfo(status.ID, currentDuration, maxDuration, currentBuildup, maxBuildup, counter);
                }
            } else {
                Int64 StatusAddress = Scanner.Read<long>(MonsterAddress + 0x78);
                StatusAddress = Scanner.Read<long>(StatusAddress + 0x57A8);
                Int64 aHolder = StatusAddress;
                while (aHolder != 0) {
                    aHolder = Scanner.Read<long>(aHolder + 0x10);
                    if (aHolder != 0) {
                        StatusAddress = aHolder;
                    }
                }
                Int64 StatusPtr = StatusAddress + 0x40;
                while (StatusPtr != 0x0)
                {

                    Int64 MonsterInStatus = Scanner.Read<long>(StatusPtr + 0x188);

                    if (MonsterInStatus == MonsterAddress) {

                        int ID = Scanner.Read<int>(StatusPtr + 0x198);

                        if (ID > MonsterData.AilmentsInfo.Count || ID < 0)
                        {
                            StatusPtr = Scanner.Read<long>(StatusPtr + 0x18);
                            continue;
                        }

                        var AilmentInfo = MonsterData.AilmentsInfo.ElementAt(ID);

                        // Skip traps for non-capturable monsters
                        if (MonsterInfo.Capture == 0 && AilmentInfo.Group == "TRAP") continue;

                        if (AilmentInfo.CanSkip && !UserSettings.PlayerConfig.HunterPie.Debug.ShowUnknownStatuses)
                        {
                            StatusPtr = Scanner.Read<long>(StatusPtr + 0x18);
                            continue;
                        } else
                        {

                            float maxBuildup = Math.Max(0, Scanner.Read<float>(StatusPtr + 0x1C8));
                            float currentBuildup = Math.Max(0, Scanner.Read<float>(StatusPtr + 0x1B8));
                            float maxDuration = Math.Max(0, Scanner.Read<float>(StatusPtr + 0x19C));
                            float currentDuration = Math.Max(0, Scanner.Read<float>(StatusPtr + 0x1F8));
                            byte counter = Scanner.Read<byte>(StatusPtr + 0x200);

                            Ailment mAilment = new Ailment {
                                Address = StatusPtr
                            };
                            mAilment.SetAilmentInfo(ID, currentDuration, maxDuration, currentBuildup, maxBuildup, counter);
                            Ailments.Add(mAilment);
                        }
                    }
                    StatusPtr = Scanner.Read<long>(StatusPtr + 0x18);
                }
            }
        }

    }
}
