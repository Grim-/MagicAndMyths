using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class CompProperties_SweepingStrike: CompProperties_AbilityEffect
    {
        public int range = 10;
        public float angle = 45f;
        public FloatRange weaponDamageMulti = new FloatRange(1, 1);
        public CompProperties_SweepingStrike()
        {
            compClass = typeof(CompAbilityEffect_SweepingStrike);
        }
    }

    public class CompAbilityEffect_SweepingStrike : CompAbilityEffect
    {
        CompProperties_SweepingStrike Props => (CompProperties_SweepingStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            List<IntVec3> cells = TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle);
            cells = cells.OrderBy(x => x.DistanceTo(this.parent.pawn.Position)).ToList();

            StageVisualEffect.CreateStageEffect(cells, map, Random.Range(8, 15), (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                Pawn attacker = this.parent.pawn;
                EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, map);;
                Pawn pawn = cell.GetFirstPawn(map);

                if (pawn != null && pawn != attacker)
                {
                    DamageInfo damage = new DamageInfo(DamageDefOf.Flame, 10, 1);

                    if (this.parent.pawn.HasWeaponEquipped())
                    {
                        damage = this.parent.pawn.equipment.PrimaryEq.GetWeaponDamage(attacker);
                    }

                    pawn.TakeDamage(damage);
                }
            });
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);


            GenDraw.DrawFieldEdges(TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle));
        }
    }
}
