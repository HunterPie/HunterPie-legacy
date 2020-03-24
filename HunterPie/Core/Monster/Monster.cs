using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using HunterPie.Memory;
using HunterPie.Logger;


namespace HunterPie.Core {
    public class Monster {
        // Private vars
        private string _id;
        private float _currentHP;
        private float _Stamina;
        private bool _IsTarget;
        private float _enrageTimer = 0;
        
        private Int64 MonsterAddress;
        public int MonsterNumber { get; private set; }

        // Monster basic info
        public string Name {
            get { return GStrings.GetMonsterNameByID(ID); }
        }
        public string ID {
            get { return _id; }
            set {
                if (value != null && _id != value) {
                    if (CurrentHP > 0) {
                        _id = value;
                        // Static stuff that can be scanned only once
                        GetMonsterWeaknesses();
                        this.IsAlive = true;
                        CreateMonsterParts(MonsterData.GetMaxPartsByMonsterID(this.ID));
                        GetMonsterParts();
                        GetMonsterAilments();
                        GetMonsterSizeModifier();
                        CaptureThreshold = MonsterData.GetMonsterCaptureThresholdByID(this.ID);
                        // Only call this if monster is actually alive
                        _onMonsterSpawn();
                    }
                } else if (value == null && _id != value) {
                    _id = value;
                    this.HPPercentage = 1f;
                    this.IsTarget = false;
                    this.IsAlive = false;
                    _onMonsterDespawn();
                    Weaknesses.Clear();
                }
            }
        }
        public float SizeMultiplier { get; private set; }
        public string Crown {
            get { return MonsterData.GetMonsterCrownByMultiplier(ID, SizeMultiplier); }
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
                        this.ID = null;
                        this.IsAlive = false;
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
        public bool IsAlive = false;
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
            this.ID = null;
            Weaknesses?.Clear();
        }

        public void StartThreadingScan() {
            MonsterInfoScanRef = new ThreadStart(ScanMonsterInfo);
            MonsterInfoScan = new Thread(MonsterInfoScanRef) {
                Name = $"Scanner_Monster.{MonsterNumber}"
            };
            Debugger.Warn($"Initializing monster({MonsterNumber}) scanner...");
            MonsterInfoScan.Start();
        }

        public void StopThread() {
            MonsterInfoScan.Abort();
        }

        private void ScanMonsterInfo() {
            while (Scanner.GameIsRunning) {
                GetMonsterAddress();
                GetMonsterIDAndName();
                GetMonsterStamina();
                GetMonsterAilments();
                GetMonsterParts();
                GetMonsterEnrageTimer();
                GetTargetMonsterAddress();
                Thread.Sleep(200);
            }
            Thread.Sleep(1000);
            ScanMonsterInfo();
        }

        public void ClearParts() {
            Parts.Clear();
            Ailments.Clear();
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
                    MonsterAddress = Scanner.READ_LONGLONG(ThirdMonsterAddress - 0x30) + 0x40;
                    break;
                case 1:
                    MonsterAddress = Scanner.READ_LONGLONG(Scanner.READ_LONGLONG(ThirdMonsterAddress - 0x30) + 0x10) + 0x40;
                    break;
                default:
                    break;
            }
        }

        private void GetMonsterHp(string MonsterModel) {
            Int64 MonsterHPComponent = Scanner.READ_LONGLONG(this.MonsterAddress + Address.Offsets.MonsterHPComponentOffset);
            Int64 MonsterTotalHPAddress = MonsterHPComponent + 0x60;
            Int64 MonsterCurrentHPAddress = MonsterTotalHPAddress + 0x4;
            float f_TotalHP = Scanner.READ_FLOAT(MonsterTotalHPAddress);
            float f_CurrentHP = Scanner.READ_FLOAT(MonsterCurrentHPAddress);

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
            Int64 NamePtr = Scanner.READ_LONGLONG(this.MonsterAddress + Address.Offsets.MonsterNamePtr);
            string MonsterId = Scanner.READ_STRING(NamePtr + 0x0c, 64).Replace("\x00", "");
            if (MonsterId != "") {
                string[] MonsterID = MonsterId.Split('\\');
                if (MonsterID.Length < 4) {
                    this.ID = null;
                    return;
                }
                MonsterId = MonsterID.LastOrDefault()?.Trim('\x00');
                GetMonsterHp(MonsterId);
                if (MonsterId.StartsWith("em") && !MonsterId.StartsWith("ems")) {
                    if (MonsterId != this.ID && this.CurrentHP > 0) Debugger.Log($"Found new monster ID: {MonsterID[4]} #{MonsterNumber} @ 0x{MonsterAddress:X}");
                    this.ID = MonsterId;
                    return;
                }
            }
            this.ID = null;
            return;
        }

        private void GetMonsterSizeModifier() {
            float SizeModifier = Scanner.READ_FLOAT(MonsterAddress + 0x7730);
            if (SizeModifier <= 0 || SizeModifier >= 2) SizeModifier = 1;
            SizeMultiplier = Scanner.READ_FLOAT(MonsterAddress + 0x188) / SizeModifier;
        }

        private void GetMonsterWeaknesses() {
            Weaknesses = MonsterData.GetMonsterWeaknessById(this.ID);
        }

        private void GetMonsterEnrageTimer() {
            EnrageTimer = Scanner.READ_FLOAT(MonsterAddress + 0x1BDFC);
            EnrageTimerStatic = Scanner.READ_FLOAT(MonsterAddress + 0x1BDFC + 0x4);
        }

        private void GetTargetMonsterAddress() {
            Int64 TargettedMonsterAddress = Scanner.READ_MULTILEVEL_PTR(Address.BASE + Address.MONSTER_SELECTED_OFFSET, Address.Offsets.MonsterSelectedOffsets);
            this.IsTarget = TargettedMonsterAddress == this.MonsterAddress;
        }

        private void CreateMonsterParts(int numberOfParts) {
            Parts.Clear();
            for (int i = 0; i < numberOfParts; i++) {
                Part mPart = new Part();
                Parts.Add(mPart);
            }
        }

        private void GetMonsterParts() {
            if (!this.IsAlive) return;
            Int64 MonsterPartAddress = MonsterAddress + Address.Offsets.MonsterPartsOffset + Address.Offsets.FirstMonsterPartOffset;
            int nMaxParts = MonsterData.GetMaxPartsByMonsterID(this.ID);
            int nRemovableParts = MonsterData.GetMaxRemovablePartsByMonsterID(this.ID);
            byte TimesBroken;
            float Health;
            float MaxHealth;
            Int64 RemovablePartAddress = MonsterAddress + Address.Offsets.RemovablePartsOffset;
            for (int PartID = 0; PartID < nMaxParts; PartID++) {
                if (MonsterData.IsPartRemovable(ID, PartID)) {
                    
                    if (Parts.Count <= PartID && Parts[PartID].PartAddress > 0) {

                        TimesBroken = Scanner.READ_BYTE(Parts[PartID].PartAddress + 0x18);
                        MaxHealth = Scanner.READ_FLOAT(Parts[PartID].PartAddress + 0x10);
                        Health = Scanner.READ_FLOAT(Parts[PartID].PartAddress + 0x0C);

                        Parts[PartID].SetPartInfo(this.ID, PartID, TimesBroken, MaxHealth, Health);
                    } else {
                        for (int RemovablePartIndex = 0; RemovablePartIndex < 32; RemovablePartIndex++) {
                            if (Scanner.READ_INT(RemovablePartAddress) <= 10) {
                                RemovablePartAddress += 8;
                            }

                            bool IsAValidPart = Scanner.READ_INT(RemovablePartAddress + 0x6C) < nRemovableParts;

                            if (IsAValidPart && Scanner.READ_INT(RemovablePartAddress + 0x0C) > 0) {
                                TimesBroken = Scanner.READ_BYTE(RemovablePartAddress + 0x18);
                                MaxHealth = Scanner.READ_FLOAT(RemovablePartAddress + 0x10);
                                Health = Scanner.READ_FLOAT(RemovablePartAddress + 0x0C);

                                Parts[PartID].SetPartInfo(this.ID, PartID, TimesBroken, MaxHealth, Health);
                                Parts[PartID].PartAddress = RemovablePartAddress;
                                Parts[PartID].IsRemovable = true;

                                // Nergigante has the same values twice in a row, so we skip it to get 
                                // the removable tail values
                                if (Scanner.READ_FLOAT(RemovablePartAddress + 0x78 + 0x10) == MaxHealth && Scanner.READ_INT(RemovablePartAddress + 0x78 + 0x8) == Scanner.READ_INT(RemovablePartAddress+ 0x8)) {
                                    RemovablePartAddress += Address.Offsets.NextRemovablePart;
                                }

                                RemovablePartAddress += Address.Offsets.NextRemovablePart;
                                break;
                            }

                            RemovablePartAddress += Address.Offsets.NextRemovablePart;
                            continue;
                        }
                    }
                    continue;
                }

                TimesBroken = Scanner.READ_BYTE(MonsterPartAddress + Address.Offsets.MonsterPartBrokenCounterOffset);
                MaxHealth = Scanner.READ_FLOAT(MonsterPartAddress + 0x4); // Total health is 4 bytes ahead
                Health = Scanner.READ_FLOAT(MonsterPartAddress); 

                Parts[PartID].SetPartInfo(this.ID, PartID, TimesBroken, MaxHealth, Health);
                if (Parts[PartID].Group == null) Parts[PartID].Group = MonsterData.GetPartGroupByPartIndex(this.ID, PartID);
                MonsterPartAddress += Address.Offsets.NextMonsterPartOffset;
                
            }
        }
        
        private void GetMonsterStamina() {
            if (!IsAlive) return;
            Int64 MonsterStaminaAddress = MonsterAddress + 0x1C098;
            MaxStamina = Scanner.READ_FLOAT(MonsterStaminaAddress + 0x4);
            Stamina = Scanner.READ_FLOAT(MonsterStaminaAddress);
        }

        private void GetMonsterAilments() {
            if (!this.IsAlive) return;
            if (Ailments.Count > 0) {
                foreach (Ailment status in Ailments) {
                    if (status.Address == 0) {
                        continue;
                    }
                    float maxBuildup = Math.Max(0, Scanner.READ_FLOAT(status.Address + 0x1C8));
                    float currentBuildup = Math.Max(0, Scanner.READ_FLOAT(status.Address + 0x1B8));
                    float maxDuration = Math.Max(0, Scanner.READ_FLOAT(status.Address + 0x19C));
                    float currentDuration = Math.Max(0, Scanner.READ_FLOAT(status.Address + 0x1F8));
                    byte counter = Scanner.READ_BYTE(status.Address + 0x200);
                    status.SetAilmentInfo(status.ID, currentDuration, maxDuration, currentBuildup, maxBuildup, counter);
                }
            } else {
                Int64 StatusAddress = Scanner.READ_LONGLONG(MonsterAddress + 0x78);
                StatusAddress = Scanner.READ_LONGLONG(StatusAddress + 0x57A8);
                Int64 aHolder = StatusAddress;
                while (aHolder != 0) {
                    aHolder = Scanner.READ_LONGLONG(aHolder + 0x10);
                    if (aHolder != 0) {
                        StatusAddress = aHolder;
                    }
                }
                Int64 StatusPtr = StatusAddress + 0x40;
                int AilmentID = 0;
                while (StatusPtr != 0x0) {
                    Int64 MonsterInStatus = Scanner.READ_LONGLONG(StatusPtr + 0x188);
                    if (MonsterInStatus == MonsterAddress) {
                        System.Xml.XmlNode AilmentInfo = MonsterData.GetAilmentByIndex(AilmentID);
                        bool IsSkippable = AilmentInfo == null ? true : AilmentInfo.Attributes["Skip"].Value == "True";
                        if (IsSkippable) {
                            StatusPtr = Scanner.READ_LONGLONG(StatusPtr + 0x18);
                            AilmentID++;
                            continue;
                        } else {
                            float maxBuildup = Math.Max(0, Scanner.READ_FLOAT(StatusPtr + 0x1C8));
                            float currentBuildup = Math.Max(0, Scanner.READ_FLOAT(StatusPtr + 0x1B8));
                            float maxDuration = Math.Max(0, Scanner.READ_FLOAT(StatusPtr + 0x19C));
                            float currentDuration = Math.Max(0, Scanner.READ_FLOAT(StatusPtr + 0x1F8));
                            byte counter = Scanner.READ_BYTE(StatusPtr + 0x200);
                            Ailment mAilment = new Ailment {
                                Address = StatusPtr
                            };
                            mAilment.SetAilmentInfo(AilmentID, currentDuration, maxDuration, currentBuildup, maxBuildup, counter);
                            AilmentID++;
                            Ailments.Add(mAilment);
                        }
                    }
                    StatusPtr = Scanner.READ_LONGLONG(StatusPtr + 0x18);
                }
            }
        }

    }
}
