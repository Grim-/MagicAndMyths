using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DamageWorkerCompProperties_HolyDamage : DamageWorkerCompProperties
    {
        public float undeadMultiplier = 3f;
        public HediffDef censureHediff;
        public float censureSeverityPerDamage = 0.02f;
        public float maxCensureSeverity = 0.3f;

        public DamageWorkerCompProperties_HolyDamage()
        {
            compClass = typeof(DamageWorkerComp_HolyDamage);
        }
    }

    public class DamageWorkerComp_HolyDamage : DamageWorkerComp
    {
        DamageWorkerCompProperties_HolyDamage Props => (DamageWorkerCompProperties_HolyDamage)props;

        public override bool ShouldApply(DamageInfo dinfo, Thing thing)
        {
            return thing is Pawn;
        }

        private bool IsUndead(Pawn pawn)
        {
            return false;
        }

        public override DamageInfo PreApply(DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn pawn)
            {
                if (IsUndead(pawn))
                {
                    dinfo.SetAmount(dinfo.Amount * Props.undeadMultiplier);
                }
                else
                {
                    if (Props.censureHediff != null)
                    {
                        Hediff censure = pawn.health.GetOrAddHediff(Props.censureHediff, null, null);
                        float severityIncrease = Mathf.Min(dinfo.Amount * Props.censureSeverityPerDamage, Props.maxCensureSeverity);
                        censure.Severity += 0.15f;
                    }
                    dinfo.SetAmount(5f);
                }
            }
            return dinfo;
        }
    }
}