using Verse;

namespace MagicAndMyths
{
    public class DamageWorker_Tranquilizer : DamageWorker_AddInjury
    {
        DamageDef_Tranquilizer Def => (DamageDef_Tranquilizer)def;

        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            DamageResult result = base.Apply(dinfo, thing);
            if (thing != null && thing is Pawn pawn && !pawn.Dead && pawn.health != null)
            {
                float severityToAdd = dinfo.Amount * 0.05f;

                HediffDef tranqDef = Def.tranqHediff;
                if (tranqDef != null)
                {
                    Hediff hediff = pawn.health.GetOrAddHediff(tranqDef);
                    if (hediff != null)
                    {
                        hediff.Severity += severityToAdd;
                    }

                }
            }

            return result;
        }
    }



    public class DamageWorker_AddOrUpdateHediffSeverity : DamageWorker_AddInjury
    {
        public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                Hediff hediff = pawn.health.GetOrAddHediff(this.def.hediff, null, dinfo);
                hediff.Severity += 0.1f;
            }

            return base.Apply(dinfo, thing);
        }

        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
        }
    }
}
