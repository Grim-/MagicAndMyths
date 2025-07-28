using EMF;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_FlameSweep : CompProperties_AbilityEffect
    {
        public int range = 5;
        public float angle = 45f;

        public FloatRange fireStartChance = new FloatRange(0.5f, 0.6f);
        public IntRange fireSize = new IntRange(5,5);

        public FriendlyFireSettings friendlyFireSettings;

        public CompProperties_FlameSweep()
        {
            compClass = typeof(CompAbilityEffect_FlameSweep);
        }
    }

    public class CompAbilityEffect_FlameSweep : CompAbilityEffect
    {
        CompProperties_FlameSweep Props => (CompProperties_FlameSweep)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;
            List<IntVec3> cells = TargetUtil.GetCellsInCone(this.parent.pawn.Position, target.Cell, (int)this.parent.verb.EffectiveRange, Props.angle);
            cells = cells.OrderBy(x => x.DistanceTo(this.parent.pawn.Position)).ToList();

            StageVisualEffect.CreateStageEffect(cells, map, Random.Range(8, 15), (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                Pawn attacker = this.parent.pawn;
                DefDatabase<EffecterDef>.GetNamed("MagicAndMyths_Explosion").Spawn(cell, targetMap);

                if (Rand.Value <= 0.6f)
                {
                    if (Rand.Value <= Props.fireStartChance.RandomInRange)
                    {
                        FireUtility.TryStartFireIn(cell, targetMap, Props.fireSize.RandomInRange, this.parent.pawn);
                    }
                }

                Pawn pawn = cell.GetFirstPawn(map);
                if (pawn != null && pawn != attacker && pawn.CanTargetThing(this.parent.pawn.Faction, Props.friendlyFireSettings))
                {
                    pawn.TakeDamage(new DamageInfo(DamageDefOf.Flame, 10));
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
