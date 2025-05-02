using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableAddHediff : CompProperties_Throwable
    {
        public HediffDef hediffDef;
        public float severity = 1.0f;
        public BodyPartDef bodyPartDef = null;
        public bool applyInRadius = true;
        public bool splitSeverityAmongTargets = true;
        public bool firstTargetOnly = false;

        public CompProperties_ThrowableAddHediff()
        {
            compClass = typeof(Comp_ThrowableAddHediff);
        }
    }

    public class Comp_ThrowableAddHediff : Comp_Throwable
    {
        public CompProperties_ThrowableAddHediff Props => (CompProperties_ThrowableAddHediff)props;

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            if (Props.hediffDef == null)
            {
                return;
            }



            Pawn singlePawn = position.GetFirstPawn(map);


            if (Props.firstTargetOnly && singlePawn != null)
            {
                ApplyHediffToPawn(singlePawn, Props.severity);
            }
            else
            {
                List<Pawn> affectedPawns = new List<Pawn>();

                foreach (IntVec3 cell in GenRadial.RadialCellsAround(position, Props.radius, true))
                {
                    if (cell.InBounds(map))
                    {
                        foreach (Thing t in cell.GetThingList(map))
                        {
                            if (t is Pawn p)
                            {
                                affectedPawns.Add(p);
                            }
                        }
                    }
                }

                float appliedSeverity = Props.severity;
                if (Props.splitSeverityAmongTargets && affectedPawns.Count > 0)
                {
                    appliedSeverity = Props.severity / affectedPawns.Count;
                }

                foreach (Pawn p in affectedPawns)
                {
                    ApplyHediffToPawn(p, appliedSeverity);
                }
            }
        }

        private void ApplyHediffToPawn(Pawn pawn, float severity)
        {
            BodyPartRecord targetPart = null;
            if (Props.bodyPartDef != null)
            {
                targetPart = pawn.health.hediffSet.GetNotMissingParts()
                    .FirstOrDefault(x => x.def == Props.bodyPartDef);
            }

            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn, targetPart);

            if (hediff.def.initialSeverity > 0)
            {
                hediff.Severity = severity;
            }

            pawn.health.AddHediff(hediff, targetPart);
        }
    }
}