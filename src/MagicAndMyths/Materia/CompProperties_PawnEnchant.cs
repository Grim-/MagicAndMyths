using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PawnEnchant : CompProperties
    {
        public CompProperties_PawnEnchant()
        {
            compClass = typeof(Comp_PawnEnchant);
        }
    }

    public class ActiveEnchantData : IExposable
    {
        public EnchantInstance EnchantInstance;
        public ThingWithComps EnchantSource;

        public bool IsActive = true;

        public ActiveEnchantData()
        {
        }

        public ActiveEnchantData(EnchantInstance enchantInstance, ThingWithComps source, bool isActive = true)
        {
            EnchantInstance = enchantInstance;
            EnchantSource = source;
            IsActive = isActive;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref EnchantInstance, "enchantInstance");
            Scribe_References.Look(ref EnchantSource, "enchantSource");
            Scribe_Values.Look(ref IsActive, "isActive", true);
        }
    }

    public class Comp_PawnEnchant : ThingComp, IStatProvider
    {
        private List<ActiveEnchantData> activeEnchants = new List<ActiveEnchantData>();
        public List<ActiveEnchantData> ActiveEnchants => activeEnchants.ToList();

        private Pawn Pawn => (Pawn)parent;

        public void AddEnchant(EnchantInstance enchant, ThingWithComps source)
        {
            bool isActive = true;

            if (enchant.def.isUnique && activeEnchants.Any(x => x.EnchantInstance.def == enchant.def && x.IsActive))
            {
                // Found an active one already - set this new one as inactive
                isActive = false;
            }

            if (!activeEnchants.Any(data => data.EnchantInstance == enchant))
            {
                var enchantData = new ActiveEnchantData(enchant, source, isActive);
                activeEnchants.Add(enchantData);

                if (isActive)
                {
                    enchant.Notify_Equipped(Pawn);
                }
            }
        }

        private void TryActivateNextUniqueEnchant(EnchantDef enchantDef)
        {
            var inactiveEnchant = activeEnchants.FirstOrDefault(x =>
                x.EnchantInstance.def == enchantDef && !x.IsActive);

            if (inactiveEnchant != null)
            {
                inactiveEnchant.IsActive = true;
                inactiveEnchant.EnchantInstance.Notify_Equipped(Pawn);
            }
        }

        public void RemoveEnchant(EnchantInstance enchant)
        {
            var enchantData = activeEnchants.FirstOrDefault(data => data.EnchantInstance == enchant);
            if (enchantData != null)
            {
                EnchantDef defToActivate = null;
                if (enchantData.EnchantInstance.def.isUnique && enchantData.IsActive)
                {
                    defToActivate = enchantData.EnchantInstance.def;
                }

                if (enchantData.IsActive)
                {
                    enchantData.EnchantInstance.Notify_Unequipped(Pawn);
                }

                activeEnchants.Remove(enchantData);

                if (defToActivate != null)
                {
                    TryActivateNextUniqueEnchant(defToActivate);
                }
            }
        }

        public void RemoveEnchantsFromSource(ThingWithComps source)
        {
            // Clone the list to avoid modification during iteration
            var enchantsToRemove = activeEnchants
                .Where(data => data.EnchantSource == source)
                .ToList();

            foreach (var enchantData in enchantsToRemove)
            {
                RemoveEnchant(enchantData.EnchantInstance);
            }
        }

        public void EquipTick()
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.TickMateria(Pawn);
                    }
                }
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);

            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_KilledPawn(pawn);
                    }
                }
            }
        }

        public void Notify_OwnerKilled()
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_OwnerKilled();
                    }
                }
            }
        }

        public void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_ProjectileImpact(Attacker, Target, Projectile);
                    }
                }
            }
        }

        public bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            bool isAbsorbed = false;
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive && enchantData.EnchantInstance.Notify_PostPreApplyDamage(ref dinfo))
                    {
                        isAbsorbed = true;
                    }
                }
            }
            return isAbsorbed;
        }

        public DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damageInfo = new DamageInfo(Damage);
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        damageInfo = enchantData.EnchantInstance.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
                    }
                }
            }
            return damageInfo;
        }

        public DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        damageResult = enchantData.EnchantInstance.Notify_ApplyMeleeDamageToTarget(target, Attacker, DamageWorkerResult);
                    }
                }
            }
            return damageResult;
        }

        public void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_OwnerThoughtGained(Thought, otherPawn);
                    }
                }
            }
        }

        public void Notify_OwnerThoughtLost(Thought Thought)
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_OwnerThoughtLost(Thought);
                    }
                }
            }
        }

        public void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_OwnerHediffGained(Hediff, partRecord, dinfo, damageResult);
                    }
                }
            }
        }

        public void Notify_OwnerHediffRemoved(Hediff Hediff)
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        enchantData.EnchantInstance.Notify_OwnerHediffRemoved(Hediff);
                    }
                }
            }
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        foreach (var gizmo in enchantData.EnchantInstance.CompGetGizmosExtra())
                        {
                            yield return gizmo;
                        }
                    }
                }
            }
        }

        #region Stat Provider
        public IEnumerable<StatModifier> GetStatOffsets(StatDef stat)
        {
            if (activeEnchants == null)
                yield break;

            foreach (var enchantData in activeEnchants)
            {
               
                if (enchantData.IsActive)
                {
                    foreach (var effect in enchantData.EnchantInstance.GetEffectsOfType<EnchantEffect_PawnStatOffset>())
                    {
                        var def = (EnchantEffectDef_PawnStatOffset)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return new StatModifier
                            {
                                stat = def.statToAffect,
                                value = effect.GetStatOffset(stat)
                            };
                        }
                    }
                }
            }
        }

        public bool HasStatOffsetFor(StatDef stat)
        {
            if (activeEnchants == null)
                return false;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData.IsActive)
                {
                    foreach (var effect in enchantData.EnchantInstance.GetEffectsOfType<EnchantEffect_PawnStatOffset>())
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
            if (activeEnchants == null)
                return false;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData.IsActive)
                {
                    foreach (var effect in enchantData.EnchantInstance.GetEffectsOfType<EnchantEffect_PawnStatFactor>())
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
            if (activeEnchants == null)
                yield break;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData.IsActive)
                {
                    foreach (var effect in enchantData.EnchantInstance.GetEffectsOfType<EnchantEffect_PawnStatFactor>())
                    {
                        var def = (EnchantEffectDef_PawnStatFactor)effect.def;
                        if (def.statToAffect == stat)
                        {
                            yield return new StatModifier
                            {
                                stat = def.statToAffect,
                                value = effect.GetStatFactor(stat),
                            };
                        }
                    }
                }
            }
        }

        public string GetExplanation(StatDef stat)
        {
            StringBuilder result = new StringBuilder();

            if (activeEnchants != null)
            {
                foreach (var enchantData in activeEnchants)
                {
                    if (enchantData.IsActive)
                    {
                        foreach (var effect in enchantData.EnchantInstance.ActiveEffects)
                        {
                            if (effect.def is EnchantEffectDef_PawnStat pawnStatDef)
                            {
                                if (pawnStatDef.statToAffect == stat)
                                {
                                    result.AppendLine($"   {enchantData.EnchantInstance.def.GetColouredLabel()}: ");
                                    result.AppendLine($"     {effect.GetExplanationString()}");
                                }
                            }
                        }
                    }
                }
            }

            return result.ToString();
        }
        #endregion

        public override string CompInspectStringExtra()
        {
            if (Prefs.DevMode)
            {
                if (activeEnchants.NullOrEmpty())
                    return "No enchantments active";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Active enchantments:");

                // First list active enchants
                foreach (var enchantData in activeEnchants.Where(x => x.IsActive))
                {
                    sb.AppendLine($"[{enchantData.EnchantSource.LabelShortCap}]  [{enchantData.EnchantInstance.def.GetColouredLabel()}]");
                }

                // Then list inactive ones
                if (activeEnchants.Any(x => !x.IsActive))
                {
                    sb.AppendLine("Inactive enchantments:");
                    foreach (var enchantData in activeEnchants.Where(x => !x.IsActive))
                    {
                        sb.AppendLine($"[INACTIVE] - [{enchantData.EnchantSource.LabelShortCap}] [{enchantData.EnchantInstance.def.GetColouredLabel()}]");
                    }
                }

                return sb.ToString().TrimEnd();
            }

            return String.Empty;
        }

        public override void PostExposeData()
        {
            Scribe_Collections.Look(ref activeEnchants, "activeEnchants", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (activeEnchants == null)
                {
                    activeEnchants = new List<ActiveEnchantData>();
                }
            }
        }
    }
}