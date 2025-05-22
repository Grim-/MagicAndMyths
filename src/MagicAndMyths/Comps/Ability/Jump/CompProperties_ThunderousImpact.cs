using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThunderousImpact : CompProperties_BaseJumpEffect
    {
        public CompProperties_ThunderousImpact()
        {
            compClass = typeof(CompAbilityEffect_ThunderousImpact);
        }
    }

    public class CompAbilityEffect_ThunderousImpact : CompAbilityEffect_BaseJumpEffect
    {
        CompProperties_ThunderousImpact Props => (CompProperties_ThunderousImpact)props;
        protected override void OnLand(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            base.OnLand(arg1, arg2, arg3);

            List<IntVec3> cells = GenRadial.RadialCellsAround(this.parent.pawn.Position, Props.landingRadius, true).ToList();

            cells = cells.OrderBy(x => x.DistanceTo(this.parent.pawn.Position)).ToList();

            StageVisualEffect.CreateStageEffect(cells, arg3.Map, 4, (IntVec3 cell, Map targetMap, int sectionIndex) =>
            {
                EffecterDefOf.WaterMist.Spawn(cell, arg3.Map);
                Pawn pawn = cell.GetFirstPawn(arg3.Map);

                if (sectionIndex == 3)
                {
                    LightningStrike.GenerateLightningStrike(targetMap, cell, 1, out IEnumerable<IntVec3> affectedCells);
                }

                if (pawn != null && pawn != this.parent.pawn)
                {
                    if (sectionIndex == 3)
                    {
                        pawn?.stances.stunner.StunFor(300, this.parent.pawn);
                    }


                }
            });
        }

    }
}
