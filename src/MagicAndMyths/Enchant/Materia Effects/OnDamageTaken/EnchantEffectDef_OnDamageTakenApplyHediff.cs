using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_OnDamageTakenApplyHediff : EnchantEffectDef
    {
        public HediffDef hediff;
        public FloatRange chanceToApply = new FloatRange(0.2f, 0.4f);
        public FloatRange severity = new FloatRange(1, 1);

        public EnchantEffectDef_OnDamageTakenApplyHediff()
        {
            workerClass = typeof(EnchantEffect_OnDamageTakenApplyHediff);
        }
    }


    public class EnchantEffect_OnDamageTakenApplyHediff : EnchantWorker
    {
        EnchantEffectDef_OnDamageTakenApplyHediff Def => (EnchantEffectDef_OnDamageTakenApplyHediff)def;

        public override bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            if (Def.hediff != null && dinfo.Instigator != null)
            {
                if (dinfo.Instigator is Pawn pawn)
                {
                    if (Rand.Value <= Def.chanceToApply.RandomInRange)
                    {
                        Hediff hediff = pawn.health.AddHediff(Def.hediff);
                        if (hediff != null)
                        { 
                            hediff.Severity = Def.severity.RandomInRange;
                        }
                    }
                }

            }

            return base.Notify_PostPreApplyDamage(ref dinfo);
        }

    }
}