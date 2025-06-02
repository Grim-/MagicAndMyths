using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_DealDamage : HediffCompProperties
    {
        public FloatRange damage;
        public DamageDef damageDef;

        public HediffCompProperties_DealDamage()
        {
            compClass = typeof(HediffComp_DealDamage);
        }
    }

    public class HediffComp_DealDamage : HediffComp_OnMeleeAttackEffect
    {
        HediffCompProperties_DealDamage Props => (HediffCompProperties_DealDamage)props;
        protected override void OnMeleeAttack(Verb_MeleeAttackDamage MeleeAttackVerb, LocalTargetInfo Target)
        {
            base.OnMeleeAttack(MeleeAttackVerb, Target);

            if (Target.Thing != null)
            {
                Target.Thing.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
            }
        }
    }


 
}