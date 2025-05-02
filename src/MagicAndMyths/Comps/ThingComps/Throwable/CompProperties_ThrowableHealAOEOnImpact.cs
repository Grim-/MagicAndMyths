using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{


    public class CompProperties_ThrowableHealAOEOnImpact : CompProperties_Throwable
    {
        public FloatRange healAmount = new FloatRange(10, 10);
        public int maxTargets = 4;
        public bool splitHealAmountBetweenTargets = true;
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

            List<Thing> pawnsInRange = GenRadial.RadialDistinctThingsAround(position, map, Props.radius, true).ToList();
            int targetcount = 0;
            float heal = Props.healAmount.RandomInRange;

            //if (Props.splitHealAmountBetweenTargets)
            //{
            //    heal = heal / 
            //}

            foreach (var item in pawnsInRange)
            {
                if (item is Pawn pawn)
                {
                    if (!pawn.Spawned || pawn.Dead)
                    {
                        continue;
                    }

                    if (targetcount >= Props.maxTargets)
                    {
                        break;
                    }

                    pawn.QuickHeal(heal);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, $"Healed +{heal}", Color.green, 3f);
                    targetcount++;
                }
                else continue;
            }
        }
    }
}