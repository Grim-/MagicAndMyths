using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Enchant : CompProperties
    {
        public int maxEnchantsAllowed = 3;

        public List<EnchantmentSlotConfig> enchantmentSlots;

        public CompProperties_Enchant()
        {
            compClass = typeof(Comp_Enchant);
        }
    }

    public class Comp_Enchant : ThingComp, IStatProvider, IDrawEquippedGizmos
    {
        private List<EnchantSlot> activeEnchants = new List<EnchantSlot>();
        public List<EnchantSlot> MateriaSlots => activeEnchants.ToList();

        private bool initializedSlots = false;
        private CompProperties_Enchant Props => (CompProperties_Enchant)props;

        protected Pawn _EquippedPawn = null;
        public Pawn EquippedPawn => _EquippedPawn;
        public bool HasEquipOwner => EquippedPawn != null;


        public bool ParentIsMelee => this.parent.def.IsMeleeWeapon;
        public bool ParentIsRanged => this.parent.def.IsRangedWeapon;


        public bool HasMaximumSlotsAllowed => OccupiedSlotsCount >= SlotCount;

        public int OccupiedSlotsCount => this.MateriaSlots.Where(x => x.IsOccupied).Count();
        public int EmptySlots => this.MateriaSlots.Where(x => !x.IsOccupied).Count();

        public int SlotCount => this.MateriaSlots.Count;



        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                if (!initializedSlots)
                {
                    //if (Props.materiaSlots == null || Props.materiaSlots.Count == 0)
                    //{
                    //   GenerateMateriaSlots();
                    //}
                    //else
                    //{
                    //    CreateMateriaSlots();
                    //}
                    GenerateMateriaSlots();
                    initializedSlots = true;
                    Log.Message("Generated slots");
                }
            }
        }

        private void GenerateMateriaSlots()
        {
            for (int i = 0; i < Props.maxEnchantsAllowed; i++)
            {
                EnchantSlot enchantSlot = AddMateriaSlot(1);

                if (enchantSlot != null)
                {
                    EnchantDef enchantDef = DefDatabase<EnchantDef>.AllDefs.RandomElement();
                    if (enchantDef.isUnique && !HasActive(enchantDef) || !enchantDef.isUnique)
                    {
                        enchantSlot.EquipMateria(enchantDef);
                    }                 
                }
            }
        }
        protected void CreateMateriaSlots()
        {
            if (activeEnchants == null || activeEnchants.Count == 0)
            {
                activeEnchants = new List<EnchantSlot>();
                for (int i = 0; i < Props.enchantmentSlots.Count; i++)
                {
                    AddMateriaSlot(Props.enchantmentSlots[i]);
                }
            }
        }


        public bool HasActive(EnchantDef enchantDef)
        {
            return activeEnchants.Any(x => x.IsOccupied && x.SlottedMateria.def == enchantDef);
        }

        #region events
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_ParentDestroyed();
            });
            base.PostDestroy(mode, previousMap);
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_ParentDestroyed();
            });      
        }
        public override void PostDraw()
        {
            base.PostDraw();

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.PostDraw();
            });
        }

        public virtual void EquipTick()
        {
            if (_EquippedPawn != null)
            {
                foreach (var item in MateriaSlots)
                {
                    if (item.SlottedMateria != null)
                    {
                        item.SlottedMateria.TickMateria(_EquippedPawn);
                    }
                }
            }
        }
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            //Log.Message("Materia Comp Notify_Equipped");

            _EquippedPawn = pawn;

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_Equipped(pawn);
            });
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            //Log.Message("Materia Comp Notify_Unequipped");
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_Unequipped(pawn);
            });

            _EquippedPawn = null;
        }

        public virtual void Notify_OwnerKilled()
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerKilled();
            });
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);

            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_KilledPawn(pawn);
            });
        }

        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(ref dinfo, out absorbed);

            var allMateria = MateriaSlots;

            bool isAbsorbed = absorbed;

            foreach (var slot in allMateria)
            {
                if (slot.IsOccupied)
                {
                    if (slot.SlottedMateria.Notify_PostPreApplyDamage(ref dinfo))
                    {
                        isAbsorbed = true;
                    }
                }
            }

            absorbed = isAbsorbed;
        }

        public virtual void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_ProjectileImpact(Attacker, Target, Projectile);
            });
        }
        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damageInfo = new DamageInfo(Damage);

            DoForAllMateria((Slot) =>
            {
                damageInfo =  Slot.SlottedMateria.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
            });

            return damageInfo;
        }
        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();

            foreach (var item in MateriaSlots)
            {
                if (item.SlottedMateria != null)
                {
                    damageResult = item.SlottedMateria.Notify_ApplyMeleeDamageToTarget(target, Attacker, DamageWorkerResult);
                }
            }

            return damageResult;
        }

        public virtual void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerThoughtGained(Thought, otherPawn);
            });

        }

        public virtual void Notify_OwnerThoughtLost(Thought Thought)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerThoughtLost(Thought);
            });
        }

        public virtual void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerHediffGained(Hediff, partRecord, dinfo, damageResult);
            });
        }
        public virtual void Notify_OwnerHediffRemoved(Hediff Hediff)
        {
            DoForAllMateria((Slot) =>
            {
                Slot.SlottedMateria.Notify_OwnerHediffRemoved(Hediff);
            });
        }
        #endregion

        #region materia


        public bool HasFreeValidMateriaSlotFor(EnchantDef materia)
        {
            return activeEnchants != null && activeEnchants.Any(slot => !slot.IsOccupied && slot.CanAccept(materia));
        }

        public bool CanAcceptSlotType(MateriaTypeDef materiaType)
        {
            return activeEnchants != null && activeEnchants.Any(slot => slot.AcceptsType(materiaType));
        }

        protected virtual void DoForAllMateria(Action<EnchantSlot> actionToDo)
        {
            foreach (var item in MateriaSlots)
            {
                if (item.SlottedMateria != null)
                {
                    actionToDo?.Invoke(item);
                }
            }
        }

        public void EquipMateria(EnchantDef materiaDef, EnchantSlot materiaslot, bool force = false)
        {
            if (materiaslot == null || materiaslot.IsOccupied)
                return;

            materiaslot.EquipMateria(materiaDef);
        }

        public void EquipMateria(EnchantDef materiaDef)
        {
            var slot = activeEnchants.FirstOrDefault(s => !s.IsOccupied && s.CanAccept(materiaDef));
            if (slot == null)
                return;
            EquipMateria(materiaDef, slot);

            if (EquippedPawn != null)
                MateriaManager.NotifyMateriaChanged(EquippedPawn);
        }

        public void UnequipMateria(EnchantSlot materiaslot, bool createMateriaItem = true)
        {
            if (materiaslot == null || !materiaslot.IsOccupied)
                return;

            materiaslot.UnequipMateria();

            if (EquippedPawn != null)
                MateriaManager.NotifyMateriaChanged(EquippedPawn);
        }



        public EnchantSlot AddMateriaSlot(int slotLevel = 1)
        {
            EnchantSlot materiaSlot = new EnchantSlot(this.parent, this, slotLevel);
            activeEnchants.Add(materiaSlot);

            return materiaSlot;
        }
        public void AddMateriaSlot(EnchantmentSlotConfig SlotConfig)
        {
            EnchantSlot materiaSlot = new EnchantSlot(this.parent, this, SlotConfig.slotLevel);
  
            activeEnchants.Add(materiaSlot);

            if (SlotConfig.enchantToAutoSlot != null)
            {
                EquipMateria(SlotConfig.enchantToAutoSlot, materiaSlot, true);
            }

            materiaSlot.SetLockStatus(SlotConfig.slotIsLocked);
        }
        public bool CanEquipMateria(EnchantDef materia)
        {
            return HasFreeValidMateriaSlotFor(materia);
        }
        public int GetSlotCount()
        {
            return MateriaSlots.Count;
        }
        public int GetOccupiedSlotCount()
        {
            return MateriaSlots.Where(x => x.IsOccupied).Count();
        }
        public int GetSlotsOfLevelCount(int Level)
        {
            return MateriaSlots.Where(x => x.SlotLevel == Level).Count();
        }

        #endregion

        #region Equipment Stat Modifiers
        private IEnumerable<EnchantEffectDef_EquipmentStat> FindStatEffects(StatDef stat)
        {
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_EquipmentStat>())
                    {
                        var def = (EnchantEffectDef_EquipmentStat)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return def;
                        }
                    }
                }
            }
        }

        public override float GetStatFactor(StatDef stat)
        {
            float factor = base.GetStatFactor(stat);
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_EquipmentStat>())
                    {
                        var def = (EnchantEffectDef_EquipmentStat)effect.def;
                        if (def.statToAffect == stat)
                        {
                            factor += def.StatFactors.GetStatFactorFromList(stat);
                        }

                    }
                }
            }
            return factor;
        }

        public override float GetStatOffset(StatDef stat)
        {
            float offset = base.GetStatOffset(stat);
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_EquipmentStat>())
                    {
                        var def = (EnchantEffectDef_EquipmentStat)effect.def;

                        if (effect.def is EnchantEffectDef_EquipmentStat equimentStatDef && def.statToAffect == stat)
                        {
                            offset += equimentStatDef.StatOffsets.GetStatOffsetFromList(stat);
                        }

                    }
                }
            }
            return offset;
        }

        public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
        {
            base.GetStatsExplanation(stat, sb);

            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_EquipmentStat>())
                    {
                        var def = (EnchantEffectDef_EquipmentStat)effect.def;
                        if (def.statToAffect == stat)
                        {
                            var offset = def.StatOffsets.GetStatOffsetFromList(stat);
                            sb.AppendLine($"Materia {effect}: ×{offset:F2}");
                        }
                    }
                }
            }
        }
        #endregion

        #region Equip Pawn Stat Modifiers

        public IEnumerable<StatModifier> GetStatOffsets(StatDef stat)
        {
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_PawnStatOffset>())
                    {
                        var def = (EnchantEffectDef_PawnStatOffset)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return new StatModifier
                            {
                                stat = def.statToAffect,
                                value = def.statOffset
                            };
                        }
                    }
                }
            }
        }

        public bool HasStatOffsetFor(StatDef stat)
        {
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_PawnStatOffset>())
                    {
                        var def = (EnchantEffectDef_PawnStatOffset)effect.def;
                        if (def.statToAffect == stat)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        public bool HasStatFactorFor(StatDef stat)
        {
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_PawnStatFactor>())
                    {
                        var def = (EnchantEffectDef_PawnStatFactor)effect.def;
                        if (def.statToAffect == stat)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<StatModifier> GetStatFactors(StatDef stat)
        {
            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.GetEffectsOfType<EnchantEffect_PawnStatFactor>())
                    {
                        var def = (EnchantEffectDef_PawnStatFactor)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return new StatModifier
                            {
                                stat = def.statToAffect,
                                value = def.statFactor,
                            };
                        }
                    }
                }
            }
        }
        public string GetExplanation(StatDef stat)
        {
            StringBuilder result = new StringBuilder();

            foreach (var slot in activeEnchants)
            {
                if (slot.IsOccupied && slot.SlottedMateria != null)
                {
                    foreach (var effect in slot.SlottedMateria.ActiveEffects)
                    {
                        if (effect.def is EnchantEffectDef_PawnStat pawnStatDef)
                        {
                            if (pawnStatDef.statToAffect == stat)
                            {
                                result.AppendLine($"   {slot.SlottedMateria.def.GetColouredLabel()}: ");
                                result.AppendLine($"     {pawnStatDef.GetExplanationString()}");
                            }
                        }
                    }
                }
               // result.AppendLine($"\n");
            }

            return result.ToString();
        }

        #endregion

        public IEnumerable<Gizmo> GetEquippedGizmos()
        {
            if (activeEnchants != null)
            {
                foreach (var slot in activeEnchants)
                {
                    if (slot.SlottedMateria != null)
                    {
                        foreach (var item in slot.SlottedMateria.CompGetGizmosExtra())
                        {
                            yield return item;
                        }
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            if (this.MateriaSlots.Count > 0)
            {
                yield return new Command_Action()
                {
                    defaultLabel = $"Manage Materia",
                    icon = MateriaPatchClass.MateriaIcon,
                    action = () =>
                    {
                        OpenMateriaWindow(this.parent, this.EquippedPawn, this);
                    }
                };
            }
        }

        private string GetEquippedMateriaInfo(bool includeDescription = false)
        {
            if (activeEnchants.NullOrEmpty() || !activeEnchants.Any(s => s.IsOccupied))
                return "No materia equipped";

            var equippedMateria = activeEnchants
                .Where(s => s.IsOccupied)
                .Select(s => includeDescription
                    ? $"  [{s.SlottedMateria.def.GetColouredLabel()}] \n     {s.EffectDescription}"
                    : $"  [{s.SlottedMateria.def.GetColouredLabel()}]")
                .ToList();

            StringBuilder output = new StringBuilder();
            //output.AppendLine($"Equipped materia  ({OccupiedSlotsCount} / {MateriaSlotGenerator.GetMaxSlotsAllowedFor(this.parent.def)}):");
            output.AppendLine(string.Join(Environment.NewLine, equippedMateria));
            return output.ToString().TrimEnd();
        }
        public override string CompInspectStringExtra()
        {
            return GetEquippedMateriaInfo();
        }
        public override string GetDescriptionPart()
        {
            string equippedMateriaString = GetEquippedMateriaInfo(true);
            if (Prefs.DevMode)
            {
                var thing = this.parent;
                return equippedMateriaString + "\n" + ScoringUtil.GetScoreString(thing);
            }
            return equippedMateriaString;
        }
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref initializedSlots, "initializedSlots", false);
            Scribe_References.Look(ref _EquippedPawn, "equippedPawn");
            Scribe_Collections.Look(ref activeEnchants, "materiaSlots", LookMode.Deep);


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (activeEnchants == null)
                {
                    CreateMateriaSlots();
                }

                if (_EquippedPawn != null)
                {
                    DoForAllMateria((Slot) =>
                    {
                        Slot.SlottedMateria.Notify_Equipped(_EquippedPawn);
                    });
                }
            }
        }
        public static void OpenMateriaWindow(Thing Thing, Pawn EquippedPawn, Comp_Enchant MateriaComp, Type displayType = null)
        {
            Find.WindowStack.Add(new Window_MateriaSelection(Thing, EquippedPawn, MateriaComp, displayType));
        }
    }
}
