using Verse;

namespace MagicAndMyths
{
    // Example comp: Damage amplification based on conditions (PreApply)
    public class DamageWorkerCompProperties_DamageAmplifier : DamageWorkerCompProperties
    {
        public float multiplier = 1.5f;
        public HediffDef requiredHediff;
        public bool targetMustHaveHediff = true;

        public DamageWorkerCompProperties_DamageAmplifier()
        {
            compClass = typeof(DamageWorkerComp_DamageAmplifier);
        }
    }


    public class DamageWorkerComp_DamageAmplifier : DamageWorkerComp
    {
        DamageWorkerCompProperties_DamageAmplifier Props => (DamageWorkerCompProperties_DamageAmplifier)props;

        public override bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            if (Props.requiredHediff == null) return true;

            if (thing is Pawn pawn)
            {
                bool hasHediff = pawn.health.hediffSet.HasHediff(Props.requiredHediff);
                return Props.targetMustHaveHediff ? hasHediff : !hasHediff;
            }

            return false;
        }

        public override DamageInfo PreApply(DamageInfo dinfo, Thing thing)
        {
            // Modify damage before it's applied
            dinfo.SetAmount(dinfo.Amount * Props.multiplier);
            return dinfo;
        }
    }

}