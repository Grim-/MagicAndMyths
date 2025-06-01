using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_OnHitConeDamage : EnchantEffectDef_OnHitBase
    {
        public FloatRange chanceToTrigger = new FloatRange(1, 1);
        public int length = 3;
        public float angle = 90f;

        public EnchantEffectDef_OnHitConeDamage()
        {
            workerClass = typeof(EnchantEffect_OnHitConeDamage);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Has a {chanceToTrigger.min * 100} - {chanceToTrigger.max * 100} % chance to trigger dealing the weapons damage in a {angle} degree cone facing your attack target.";
            }
        }


    }

    public class EnchantEffect_OnHitConeDamage : EnchantWorker
    {
        EnchantEffectDef_OnHitConeDamage Def => (EnchantEffectDef_OnHitConeDamage)def;

        public override DamageWorker.DamageResult Notify_ApplyMeleeDamageToTarget(LocalTargetInfo target, Pawn Attacker, ref DamageWorker.DamageResult damageResult)
        {
            if (Def.hitMode == OnHitMode.Melee && Rand.Value <= Def.chanceToTrigger.RandomInRange)
            {
                StageVisualEffect.CreateConalStageEffect(Attacker.Position, target.Cell, Def.length, Def.angle, Attacker.Map, Rand.Range(1, 5), (IntVec3 cell, Map map, int section) =>
                {
                    Pawn attacker = Attacker;
                    EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, map);
                    Pawn pawn = cell.GetFirstPawn(map);

                    if (pawn != null && pawn != attacker)
                    {
                        DamageInfo damage = new DamageInfo(DamageDefOf.Flame, 10, 1);

                        if (attacker.HasWeaponEquipped())
                        {
                            damage = attacker.equipment.PrimaryEq.GetWeaponDamage(attacker);
                        }
                        pawn.TakeDamage(damage);
                    }
                });
            }

            return damageResult;
        }
    }
}