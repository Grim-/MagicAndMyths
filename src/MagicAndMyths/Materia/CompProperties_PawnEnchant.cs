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
        public List<ActiveEnchantData> ActiveEnchants => activeEnchants?.ToList() ?? new List<ActiveEnchantData>();

        private Pawn Pawn => (Pawn)parent;

        public void AddEnchant(EnchantInstance enchant, ThingWithComps source)
        {
            if (enchant == null || source == null)
                return;

            bool isActive = true;

            if (enchant.def != null && enchant.def.isUnique &&
                activeEnchants.Any(x => x.EnchantInstance != null &&
                x.EnchantInstance.def == enchant.def && x.IsActive))
            {
                isActive = false;
            }

            if (!activeEnchants.Any(data => data.EnchantInstance == enchant))
            {
                var enchantData = new ActiveEnchantData(enchant, source, isActive);
                activeEnchants.Add(enchantData);

                enchant.Notify_Equipped(Pawn);
            }
        }

        private void TryActivateNextUniqueEnchant(EnchantDef enchantDef)
        {
            if (enchantDef == null)
                return;

            var inactiveEnchant = activeEnchants.FirstOrDefault(x =>
                x.EnchantInstance != null && x.EnchantInstance.def == enchantDef && !x.IsActive);

            if (inactiveEnchant != null && inactiveEnchant.EnchantInstance != null)
            {
                inactiveEnchant.IsActive = true;
                inactiveEnchant.EnchantInstance.Notify_Equipped(Pawn);
            }
        }

        public void RemoveEnchant(EnchantInstance enchant)
        {
            if (enchant == null)
                return;

            var enchantData = activeEnchants.FirstOrDefault(data => data.EnchantInstance == enchant);
            if (enchantData != null && enchantData.EnchantInstance != null)
            {
                enchantData.EnchantInstance.Notify_Unequipped(Pawn);
                activeEnchants.Remove(enchantData);

                TryActivateNextUniqueEnchant(enchant.def);
            }
        }

        public void RemoveEnchantsFromSource(ThingWithComps source)
        {
            if (source == null)
                return;

            var enchantsToRemove = activeEnchants
                .Where(data => data.EnchantSource == source)
                .ToList();

            foreach (var enchantData in enchantsToRemove)
            {
                if (enchantData.EnchantInstance != null)
                {
                    RemoveEnchant(enchantData.EnchantInstance);
                }
            }
        }

        public void EquipTick()
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.TickMateria(Pawn);
                }
            }
        }

        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);

            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_KilledPawn(pawn);
                }
            }
        }

        public void Notify_OwnerKilled()
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_OwnerKilled();
                }
            }
        }

        public void Notify_ProjectileImpact(Pawn Attacker, Thing Target, Projectile Projectile)
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_ProjectileImpact(Attacker, Target, Projectile);
                }
            }
        }

        public bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            bool isAbsorbed = false;
            if (activeEnchants == null)
                return isAbsorbed;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null &&
                    enchantData.EnchantInstance.Notify_PostPreApplyDamage(ref dinfo))
                {
                    isAbsorbed = true;
                }
            }
            return isAbsorbed;
        }

        public DamageInfo Notify_ProjectileApplyDamageToTarget(DamageInfo Damage, Pawn Attacker, Thing Target, Projectile Projectile)
        {
            DamageInfo damageInfo = new DamageInfo(Damage);
            if (activeEnchants == null)
                return damageInfo;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    damageInfo = enchantData.EnchantInstance.Notify_ProjectileApplyDamageToTarget(damageInfo, Attacker, Target, Projectile);
                }
            }
            return damageInfo;
        }

        public DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, DamageWorker.DamageResult DamageWorkerResult)
        {
            DamageWorker.DamageResult damageResult = new DamageWorker.DamageResult();
            if (activeEnchants == null)
                return damageResult;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    damageResult = enchantData.EnchantInstance.Notify_ApplyMeleeDamageToTarget(target, Attacker, DamageWorkerResult);
                }
            }
            return damageResult;
        }

        public void Notify_OwnerThoughtGained(Thought Thought, Pawn otherPawn)
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_OwnerThoughtGained(Thought, otherPawn);
                }
            }
        }

        public void Notify_OwnerThoughtLost(Thought Thought)
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_OwnerThoughtLost(Thought);
                }
            }
        }

        public void Notify_OwnerHediffGained(Hediff Hediff, BodyPartRecord partRecord, DamageInfo? dinfo, DamageWorker.DamageResult damageResult)
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_OwnerHediffGained(Hediff, partRecord, dinfo, damageResult);
                }
            }
        }

        public void Notify_OwnerHediffRemoved(Hediff Hediff)
        {
            if (activeEnchants == null)
                return;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    enchantData.EnchantInstance.Notify_OwnerHediffRemoved(Hediff);
                }
            }
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (activeEnchants == null)
                yield break;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    foreach (var gizmo in enchantData.EnchantInstance.CompGetGizmosExtra())
                    {
                        yield return gizmo;
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
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    foreach (var effect in enchantData.EnchantInstance.GetEffectsOfType<EnchantEffect_PawnStat>().Where(
                        x => x != null && x.StatDef != null &&
                        x.StatDef.modifierType == StatModifierType.Offset))
                    {
                        var def = (EnchantEffectDef_PawnStat)effect.def;
                        if (def != null && def.statToAffect == stat)
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

        public IEnumerable<StatModifier> GetStatFactors(StatDef stat)
        {
            if (activeEnchants == null)
                yield break;

            foreach (var enchantData in activeEnchants)
            {
                if (enchantData != null && enchantData.IsActive &&
                    enchantData.EnchantInstance != null)
                {
                    foreach (var effect in enchantData.EnchantInstance.GetEffectsOfType<EnchantEffect_PawnStat>().Where(
                        x => x != null && x.StatDef != null &&
                        x.StatDef.modifierType == StatModifierType.Factor))
                    {
                        var def = (EnchantEffectDef_PawnStat)effect.def;
                        if (def != null && def.statToAffect == stat)
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
                    if (enchantData != null && enchantData.IsActive &&
                        enchantData.EnchantInstance != null)
                    {
                        foreach (var effect in enchantData.EnchantInstance.ActiveEffects)
                        {
                            if (effect != null && effect.def is EnchantEffectDef_PawnStat pawnStatDef)
                            {
                                if (pawnStatDef != null && pawnStatDef.statToAffect == stat)
                                {
                                    result.AppendLine($"   {enchantData.EnchantInstance.def?.GetColouredLabel() ?? "Unknown"}: ");
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
                foreach (var enchantData in activeEnchants.Where(x => x != null && x.IsActive &&
                                                                      x.EnchantInstance != null))
                {
                    sb.AppendLine($"[{enchantData.EnchantSource?.LabelShortCap ?? "Unknown"}]  [{enchantData.EnchantInstance.def?.GetColouredLabel() ?? "Unknown"}]");
                }

                // Then list inactive ones
                if (activeEnchants.Any(x => x != null && !x.IsActive))
                {
                    sb.AppendLine("Inactive enchantments:");
                    foreach (var enchantData in activeEnchants.Where(x => x != null && !x.IsActive &&
                                                                          x.EnchantInstance != null))
                    {
                        sb.AppendLine($"[INACTIVE] - [{enchantData.EnchantSource?.LabelShortCap ?? "Unknown"}] [{enchantData.EnchantInstance.def?.GetColouredLabel() ?? "Unknown"}]");
                    }
                }

                return sb.ToString().TrimEnd();
            }

            return String.Empty;
        }

        //doesnt actually save them, they are reapplied on a reload by the providers
        //public override void PostExposeData()
        //{
        //  //  Scribe_Collections.Look(ref activeEnchants, "activeEnchants", LookMode.Deep);
        //}
    }
}