using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableHealAOEOnImpact : CompProperties_Throwable
    {
        public float explosionRadius = 3f;
        public FloatRange healAmount = new FloatRange(10, 10);
        public int maxTargets = 4;

        public CompProperties_ThrowableHealAOEOnImpact()
        {
            compClass = typeof(Comp_ThrowableHealAOEOnImpact);
        }
    }

    public class Comp_ThrowableHealAOEOnImpact : Comp_Throwable
    {
        CompProperties_ThrowableHealAOEOnImpact Props => (CompProperties_ThrowableHealAOEOnImpact)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            List<Pawn> pawnsInRange = GenRadial.RadialDistinctThingsAround(position, map, Props.explosionRadius, true).Where(x => x is Pawn).Cast<Pawn>().ToList();
            int targetcount = 0;

            foreach (var item in pawnsInRange)
            {
                if (!item.Spawned || item.Dead)
                {
                    continue;
                }

                if (targetcount >= Props.maxTargets)
                {
                    break;
                }

                item.QuickHeal(Props.healAmount.RandomInRange);
                targetcount++;
            }
        }
    }
}