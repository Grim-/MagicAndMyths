using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_DealWeaponDamage : CompProperties_BaseWeaponAbility
    {
        public CompProperties_DealWeaponDamage()
        {
            compClass = typeof(CompAbilityEffect_DealWeaponDamage);
        }
    }

    public class CompAbilityEffect_DealWeaponDamage : CompAbilityEffect_BaseWeaponAbility
    {
        CompProperties_DealWeaponDamage Props => (CompProperties_DealWeaponDamage)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Thing is Pawn targetPawn)
            {
                DealDamage(this.parent.pawn, targetPawn);
            }
        }


        protected virtual void DealDamage(Pawn Attacker, Pawn target)
        {
            if (Attacker.HasWeaponEquipped())
            {
                target.TakeDamage(Attacker.equipment.PrimaryEq.GetWeaponDamage(this.parent.pawn, Props.weaponDamageMulti.RandomInRange));
            }
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!this.parent.pawn.HasWeaponEquipped())
            {
                reason = "You must have a weapon equipped";
                return false;
            }

            return base.GizmoDisabled(out reason);
        }
    }
}
