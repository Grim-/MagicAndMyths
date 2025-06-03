using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_DealDamage : HediffCompProperties
    {
        public FloatRange damage;
        public DamageDef damageDef;

        public bool useWeaponDamageIfAvailable = true;
        public bool overrideWeaponDamageDef = false;

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
                if (Props.useWeaponDamageIfAvailable && this.Pawn.HasWeaponEquipped())
                {
                    DamageInfo damage = this.Pawn.equipment.PrimaryEq.GetWeaponDamage(this.Pawn, Props.damage.RandomInRange);
                    if (Props.overrideWeaponDamageDef && Props.damageDef != null) damage.Def = Props.damageDef;
                    Target.Thing.TakeDamage(damage);
                }
                else
                {
                    Target.Thing.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
                }
                
            }
        }
    }


}