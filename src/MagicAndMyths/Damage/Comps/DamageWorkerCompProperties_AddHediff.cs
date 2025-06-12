using Verse;
using static Verse.DamageWorker;

namespace MagicAndMyths
{
    public class DamageWorkerCompProperties_AddHediff : DamageWorkerCompProperties
    {
        public HediffDef hediffDef;
        public FloatRange severityRange = new FloatRange(1f, 1f);
        public float chance = 1f;
        public bool onlyLivingPawns = true;

        public DamageWorkerCompProperties_AddHediff()
        {
            compClass = typeof(DamageWorkerComp_AddHediff);
        }
    }

    public class DamageWorkerComp_AddHediff : DamageWorkerComp
    {
        DamageWorkerCompProperties_AddHediff Props => (DamageWorkerCompProperties_AddHediff)props;

        public override bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            if (Props.onlyLivingPawns && !(thing is Pawn pawn && pawn.RaceProps.IsFlesh))
                return false;

            return Rand.Chance(Props.chance);
        }

        public override DamageResult PostApply(DamageInfo dinfo, Thing thing, DamageResult result)
        {
            if (thing is Pawn pawn && Props.hediffDef != null)
            {
                float severity = Props.severityRange.RandomInRange;
                pawn.health.AddHediff(Props.hediffDef, null, null, null);

                if (pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef) is Hediff hediff)
                {
                    hediff.Severity = severity;
                }
            }
            return result;
        }
    }
}