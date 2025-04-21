using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableApplyDamage : CompProperties_ThrowableAffectPawns
    {
        public DamageDef damageDef;
        public FloatRange damageAmount = new FloatRange(10f, 10f);
        public bool armorPenetration = false;

        public CompProperties_ThrowableApplyDamage()
        {
            compClass = typeof(Comp_ThrowableApplyDamage);
        }
    }


    public class Comp_ThrowableApplyDamage : Comp_ThrowableAffectPawns
    {
        new public CompProperties_ThrowableApplyDamage Props => (CompProperties_ThrowableApplyDamage)props;

        protected override void AffectThing(Thing thing, Pawn throwingPawn)
        {
            if (Props.damageDef == null)
                return;

            float damage = Props.damageAmount.RandomInRange;

            DamageInfo dinfo = new DamageInfo(
                Props.damageDef,
                damage,
                Props.armorPenetration ? 1.0f : 0f,
                -1f,
                throwingPawn);

            thing.TakeDamage(dinfo);
        }
    }
}