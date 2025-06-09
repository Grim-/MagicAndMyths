using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class WeaponAbility : Ability
    {
        public WeaponAbility()
        {

        }

        public WeaponAbility(Pawn pawn) : base(pawn)
        {
        }

        public WeaponAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept)
        {
        }

        public WeaponAbility(Pawn pawn, AbilityDef def) : base(pawn, def)
        {
        }

        public WeaponAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def)
        {
        }

        public bool HasPrimaryWeapon => this.pawn.equipment != null && this.pawn.equipment.PrimaryEq != null;
        public ThingWithComps Weapon => this.pawn.equipment.PrimaryEq.parent;

        public override bool GizmoDisabled(out string reason)
        {
            if (!HasPrimaryWeapon)
            {
                reason = $"Has no primary weapon equipped";
                return true;
            }

            if (!this.pawn.equipment.PrimaryEq.parent.def.IsMeleeWeapon)
            {
                reason = $"requires a melee weapon";
                return true;
            }

            return base.GizmoDisabled(out reason);
        }


    }
}