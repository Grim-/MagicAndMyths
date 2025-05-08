//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Verse;

//namespace MagicAndMyths
//{
//    public class CompProperties_Enchant : CompProperties
//    {
//        public int maxEnchantsAllowed = 3;

//        public List<EnchantmentSlotConfig> enchantmentSlots;

//        public CompProperties_Enchant()
//        {
//            compClass = typeof(Comp_Enchant);
//        }
//    }

//    public class Comp_Enchant : ThingComp, IStatProvider, IDrawEquippedGizmos
//    {
//        private List<EnchantInstance> activeEnchants = new List<EnchantInstance>();
//        public List<EnchantInstance> ActiveEnchants => activeEnchants.ToList();

//        private bool initialized = false;
//        private CompProperties_Enchant Props => (CompProperties_Enchant)props;

//        protected Pawn _EquippedPawn = null;
//        public Pawn EquippedPawn => _EquippedPawn;
//        public bool HasEquipOwner => EquippedPawn != null;

//        public bool ParentIsMelee => this.parent.def.IsMeleeWeapon;
//        public bool ParentIsRanged => this.parent.def.IsRangedWeapon;

//        public bool HasMaximumEnchantsAllowed => activeEnchants.Count >= Props.maxEnchantsAllowed;
//        public int EnchantCount => this.activeEnchants.Count;

//        public override void PostSpawnSetup(bool respawningAfterLoad)
//        {
//            base.PostSpawnSetup(respawningAfterLoad);

//            if (!respawningAfterLoad)
//            {
//                if (!initialized)
//                {
//                    if (Props.enchantmentSlots != null && Props.enchantmentSlots.Count > 0)
//                    {
//                        InitializeEnchants();
//                    }
//                    initialized = true;
//                    Log.Message("Initialized enchantments");
//                }
//            }
//        }

//        private void InitializeEnchants()
//        {
//            if (activeEnchants == null)
//            {
//                activeEnchants = new List<EnchantInstance>();
//            }


//            if (Props.enchantmentSlots != null)
//            {
//                foreach (var slotConfig in Props.enchantmentSlots)
//                {
//                    if (slotConfig.enchantToAutoSlot != null)
//                    {
//                        AddEnchant(slotConfig.enchantToAutoSlot);
//                    }
//                }
//            }



//            for (int i = 0; i < Props.maxEnchantsAllowed; i++)
//            {
//                if (HasMaximumEnchantsAllowed)
//                {
//                    break;
//                }

//                AddEnchant(DefDatabase<EnchantDef>.AllDefs.Where(x => x.IsValidEquipmentType(this.parent)).RandomElement());

//            }
//        }

//        public bool HasActive(EnchantDef enchantDef)
//        {
//            return activeEnchants != null && activeEnchants.Any(x => x.def == enchantDef);
//        }

//        #region events
//        public override void PostDestroy(DestroyMode mode, Map previousMap)
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_ParentDestroyed();
//                }
//            }
//            base.PostDestroy(mode, previousMap);
//        }

//        public override void PostDeSpawn(Map map)
//        {
//            base.PostDeSpawn(map);
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_ParentDestroyed();
//                }
//            }
//        }

//        public override void PostDraw()
//        {
//            base.PostDraw();

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.PostDraw();
//                }
//            }
//        }

//        public virtual void EquipTick()
//        {
//            if (_EquippedPawn != null && activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.TickMateria(_EquippedPawn);
//                }
//            }
//        }

//        public override void Notify_Equipped(Pawn pawn)
//        {
//            base.Notify_Equipped(pawn);

//            _EquippedPawn = pawn;

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_Equipped(pawn);
//                }
//            }
//        }

//        public override void Notify_Unequipped(Pawn pawn)
//        {
//            base.Notify_Unequipped(pawn);

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_Unequipped(pawn);
//                }
//            }

//            _EquippedPawn = null;
//        }

//        public virtual void Notify_OwnerKilled()
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_OwnerKilled();
//                }
//            }
//        }

//        public override void Notify_KilledPawn(Pawn pawn)
//        {
//            base.Notify_KilledPawn(pawn);

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_KilledPawn(pawn);
//                }
//            }
//        }

//        public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
//        {
//            base.PostPreApplyDamage(ref dinfo, out absorbed);

//            bool isAbsorbed = absorbed;

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    if (enchant.Notify_PostPreApplyDamage(ref dinfo))
//                    {
//                        isAbsorbed = true;
//                    }
//                }
//            }

//            absorbed = isAbsorbed;
//        }

//        public virtual void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_ProjectileImpact(Attacker, Target, Projectile);
//                }
//            }
//        }

//        public virtual DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
//        {
//            DamageInfo damageInfo = new DamageInfo(Damage);

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    damageInfo = enchant.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
//                }
//            }

//            return damageInfo;
//        }

//        public virtual DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
//        {
//            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    damageResult = enchant.Notify_ApplyMeleeDamageToTarget(target, Attacker, DamageWorkerResult);
//                }
//            }

//            return damageResult;
//        }

//        public virtual void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_OwnerThoughtGained(Thought, otherPawn);
//                }
//            }
//        }

//        public virtual void Notify_OwnerThoughtLost(Thought Thought)
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_OwnerThoughtLost(Thought);
//                }
//            }
//        }

//        public virtual void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_OwnerHediffGained(Hediff, partRecord, dinfo, damageResult);
//                }
//            }
//        }

//        public virtual void Notify_OwnerHediffRemoved(Hediff Hediff)
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    enchant.Notify_OwnerHediffRemoved(Hediff);
//                }
//            }
//        }
//        #endregion

//        #region enchant management
//        public bool CanAddEnchant(EnchantDef enchantDef)
//        {
//            if (HasMaximumEnchantsAllowed)
//                return false;

//            return enchantDef.IsValidEquipmentType(this.parent);
//        }

//        public EnchantInstance AddEnchant(EnchantDef enchantDef, bool force = false)
//        {
//            if (!CanAddEnchant(enchantDef) && !force)
//                return null;

//            var enchantInstance = new EnchantInstance(enchantDef, this);
//            activeEnchants.Add(enchantInstance);

//            if (HasEquipOwner)
//            {
//                enchantInstance.Notify_Equipped(EquippedPawn);
//            }

//            enchantInstance.Notify_MateriaEquipped();
//            return enchantInstance;
//        }

//        public void RemoveEnchant(EnchantDef enchant, bool createMateriaItem = true)
//        {
//            if (enchant == null || !activeEnchants.Any(x => x.def == enchant))
//                return;
//            EnchantInstance enchantInstance = activeEnchants.First(x => x.def == enchant);
//            RemoveEnchant(enchantInstance, createMateriaItem);
//        }


//        public void RemoveEnchant(EnchantInstance enchantInstance, bool createMateriaItem = true)
//        {
//            if (enchantInstance == null || !activeEnchants.Contains(enchantInstance))
//                return;

//            enchantInstance.Notify_MateriaUnequipped();

//            if (HasEquipOwner)
//            {
//                enchantInstance.Notify_Unequipped(EquippedPawn);
//            }

//            activeEnchants.Remove(enchantInstance);
//        }

//        public int GetEnchantCount()
//        {
//            return activeEnchants?.Count ?? 0;
//        }
//        #endregion

//        #region Equipment Stat Modifiers
//        private IEnumerable<EnchantEffectDef_EquipmentStat> FindStatEffects(StatDef stat)
//        {
//            if (activeEnchants == null)
//                yield break;

//            foreach (var enchant in activeEnchants)
//            {
//                foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_EquipmentStat>())
//                {
//                    var def = (EnchantEffectDef_EquipmentStat)effect.def;
//                    if (def.statToAffect == stat)
//                    {
//                        yield return def;
//                    }
//                }
//            }
//        }

//        public override float GetStatFactor(StatDef stat)
//        {
//            float factor = base.GetStatFactor(stat);

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_EquipmentStat>())
//                    {
//                        var def = (EnchantEffectDef_EquipmentStat)effect.def;
//                        if (def.statToAffect == stat)
//                        {
//                            factor += def.StatFactors.GetStatFactorFromList(stat);
//                        }
//                    }
//                }
//            }

//            return factor;
//        }

//        public override float GetStatOffset(StatDef stat)
//        {
//            float offset = base.GetStatOffset(stat);

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_EquipmentStat>())
//                    {
//                        var def = (EnchantEffectDef_EquipmentStat)effect.def;

//                        if (effect.def is EnchantEffectDef_EquipmentStat equimentStatDef && def.statToAffect == stat)
//                        {
//                            offset += equimentStatDef.StatOffsets.GetStatOffsetFromList(stat);
//                        }
//                    }
//                }
//            }

//            return offset;
//        }

//        public override void GetStatsExplanation(StatDef stat, StringBuilder sb)
//        {
//            base.GetStatsExplanation(stat, sb);

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_EquipmentStat>())
//                    {
//                        var def = (EnchantEffectDef_EquipmentStat)effect.def;
//                        if (def.statToAffect == stat)
//                        {
//                            var offset = def.StatOffsets.GetStatOffsetFromList(stat);
//                            sb.AppendLine($"Enchant {effect}: ×{offset:F2}");
//                        }
//                    }
//                }
//            }
//        }
//        #endregion

//        #region Equip Pawn Stat Modifiers
//        public IEnumerable<StatModifier> GetStatOffsets(StatDef stat)
//        {
//            if (activeEnchants == null)
//                yield break;

//            foreach (var enchant in activeEnchants)
//            {
//                foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_PawnStatOffset>())
//                {
//                    var def = (EnchantEffectDef_PawnStatOffset)effect.def;
//                    if (def.statToAffect == stat)
//                    {
//                        yield return new StatModifier
//                        {
//                            stat = def.statToAffect,
//                            value = def.statOffset
//                        };
//                    }
//                }
//            }
//        }

//        public bool HasStatOffsetFor(StatDef stat)
//        {
//            if (activeEnchants == null)
//                return false;

//            foreach (var enchant in activeEnchants)
//            {
//                foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_PawnStatOffset>())
//                {
//                    var def = (EnchantEffectDef_PawnStatOffset)effect.def;
//                    if (def.statToAffect == stat)
//                    {
//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        public bool HasStatFactorFor(StatDef stat)
//        {
//            if (activeEnchants == null)
//                return false;

//            foreach (var enchant in activeEnchants)
//            {
//                foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_PawnStatFactor>())
//                {
//                    var def = (EnchantEffectDef_PawnStatFactor)effect.def;
//                    if (def.statToAffect == stat)
//                    {
//                        return true;
//                    }
//                }
//            }

//            return false;
//        }

//        public IEnumerable<StatModifier> GetStatFactors(StatDef stat)
//        {
//            if (activeEnchants == null)
//                yield break;

//            foreach (var enchant in activeEnchants)
//            {
//                foreach (var effect in enchant.GetEffectsOfType<EnchantEffect_PawnStatFactor>())
//                {
//                    var def = (EnchantEffectDef_PawnStatFactor)effect.def;
//                    if (def.statToAffect == stat)
//                    {
//                        yield return new StatModifier
//                        {
//                            stat = def.statToAffect,
//                            value = def.statFactor,
//                        };
//                    }
//                }
//            }
//        }

//        public string GetExplanation(StatDef stat)
//        {
//            StringBuilder result = new StringBuilder();

//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    foreach (var effect in enchant.ActiveEffects)
//                    {
//                        if (effect.def is EnchantEffectDef_PawnStat pawnStatDef)
//                        {
//                            if (pawnStatDef.statToAffect == stat)
//                            {
//                                result.AppendLine($"   {enchant.def.GetColouredLabel()}: ");
//                                result.AppendLine($"     {pawnStatDef.GetExplanationString()}");
//                            }
//                        }
//                    }
//                }
//            }

//            return result.ToString();
//        }
//        #endregion

//        public IEnumerable<Gizmo> GetEquippedGizmos()
//        {
//            if (activeEnchants != null)
//            {
//                foreach (var enchant in activeEnchants)
//                {
//                    foreach (var item in enchant.CompGetGizmosExtra())
//                    {
//                        yield return item;
//                    }
//                }
//            }
//        }

//        private string GetEquippedEnchantsInfo(bool includeDescription = false)
//        {
//            if (activeEnchants.NullOrEmpty())
//                return "No enchantments equipped";

//            var equippedEnchants = activeEnchants
//                .Select(enchant => includeDescription
//                    ? $"  [{enchant.def.GetColouredLabel()}] \n     {GetEffectDescription(enchant)}"
//                    : $"  [{enchant.def.GetColouredLabel()}]")
//                .ToList();

//            StringBuilder output = new StringBuilder();
//            output.AppendLine(string.Join(Environment.NewLine, equippedEnchants));
//            return output.ToString().TrimEnd();
//        }

//        private string GetEffectDescription(EnchantInstance enchant)
//        {
//            return string.Join("\n", enchant.ActiveEffects.Select(x => x.def.EffectDescription));
//        }

//        public override string CompInspectStringExtra()
//        {
//            return GetEquippedEnchantsInfo();
//        }

//        public override string GetDescriptionPart()
//        {
//            string equippedEnchantsString = GetEquippedEnchantsInfo(true);
//            return equippedEnchantsString;
//        }

//        public override void PostExposeData()
//        {
//            Scribe_Values.Look(ref initialized, "initialized", false);
//            Scribe_References.Look(ref _EquippedPawn, "equippedPawn");
//            Scribe_Collections.Look(ref activeEnchants, "activeEnchants", LookMode.Deep);

//            if (Scribe.mode == LoadSaveMode.PostLoadInit)
//            {
//                if (activeEnchants == null)
//                {
//                    activeEnchants = new List<EnchantInstance>();
//                    InitializeEnchants();
//                }

//                if (_EquippedPawn != null)
//                {
//                    foreach (var enchant in activeEnchants)
//                    {
//                        enchant.Notify_Equipped(_EquippedPawn);
//                    }
//                }
//            }
//        }
//    }
//}
