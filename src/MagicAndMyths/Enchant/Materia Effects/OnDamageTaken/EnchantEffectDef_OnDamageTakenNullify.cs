using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_OnDamageTakenNullify : EnchantEffectDef
    {
        public FloatRange chance = new FloatRange(0.2f, 0.4f);
        public EffecterDef triggerEffect = null;
        public EnchantEffectDef_OnDamageTakenNullify()
        {
            workerClass = typeof(EnchantEffect_OnDamageTakenNullify);
        }
    }

    public class EnchantEffect_OnDamageTakenNullify : EnchantWorker
    {
        EnchantEffectDef_OnDamageTakenNullify Def => (EnchantEffectDef_OnDamageTakenNullify)def;

        public override bool Notify_PostPreApplyDamage(ref DamageInfo dinfo)
        {
            bool wasAbsorbed = base.Notify_PostPreApplyDamage(ref dinfo);
            if (Rand.Value <= Def.chance.RandomInRange)
            {
                if (Def.triggerEffect != null)
                {
                    Def.triggerEffect.Spawn(this.EquippingPawn, this.EquippingPawn.Map);
                }

                wasAbsorbed = true;
            }
            return wasAbsorbed;
        }

    }
}