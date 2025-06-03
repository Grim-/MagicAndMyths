using Verse;

namespace MagicAndMyths
{
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
