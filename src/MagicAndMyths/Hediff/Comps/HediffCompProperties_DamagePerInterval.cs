using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_DamagePerInterval : HediffCompProperties_BaseInterval
    {
        public DamageDef damageDef;
        public FloatRange damageAmount;
        public HediffCompProperties_DamagePerInterval()
        {
            compClass = typeof(HediffComp_DamagePerInterval);
        }
    }


    public class HediffComp_DamagePerInterval : HediffComp_BaseInterval
    {
        new public HediffCompProperties_DamagePerInterval Props => (HediffCompProperties_DamagePerInterval)props;

        protected override void OnInterval()
        {
            base.OnInterval();
            if (Props.damageDef != null)
            {
                this.Pawn.TakeDamage(new DamageInfo(Props.damageDef, Props.damageAmount.RandomInRange));
            }
        }
    }

}