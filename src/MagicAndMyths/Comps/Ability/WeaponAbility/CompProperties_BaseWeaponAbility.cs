using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class CompProperties_BaseWeaponAbility : CompProperties_AbilityEffect
    {
        public FloatRange weaponDamageMulti = new FloatRange(1, 1);
    }

    public abstract class CompAbilityEffect_BaseWeaponAbility : CompAbilityEffect
    {
        CompProperties_BaseWeaponAbility Props => (CompProperties_BaseWeaponAbility)props;
        public DamageInfo GetWeaponDamage(Pawn attacker, Pawn target)
        {
            if (attacker.HasWeaponEquipped())
            {
                return this.parent.pawn.equipment.PrimaryEq.GetWeaponDamage(attacker, Props.weaponDamageMulti.RandomInRange);
            }

            return new DamageInfo(DamageDefOf.Cut, 10, 1);
        }
    }


}
