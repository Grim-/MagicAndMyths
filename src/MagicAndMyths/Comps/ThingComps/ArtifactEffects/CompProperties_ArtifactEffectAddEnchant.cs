using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectAddEnchant : CompProperties
    {
        public EnchantDef enchantDef;

        public CompProperties_ArtifactEffectAddEnchant()
        {
            compClass = typeof(Comp_ArtifactEffectAddEnchant);
        }
    }

    public class Comp_ArtifactEffectAddEnchant : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectAddEnchant Props => (CompProperties_ArtifactEffectAddEnchant)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (Props.enchantDef == null)
                return;

            if (target.Thing != null)
            {
                if (target.Thing.TryGetComp(out Comp_Enchant compEnchant))
                {
                    if (compEnchant.CanEquipMateria(Props.enchantDef))
                    {
                        compEnchant.EquipMateria(Props.enchantDef);
                    }
                }
            }
        }

        public override bool CanApply(Pawn user, LocalTargetInfo TargetInfo, Thing item, ref string reason)
        {
            if (!TargetInfo.Thing.def.IsApparel && !TargetInfo.Thing.def.IsWeapon && !TargetInfo.Thing.def.HasComp<CompEquippable>())
            {
                reason = "Taget must be equippable";
                return false;
            }
            return base.CanApply(user, TargetInfo, item, ref reason);
        }

        public override bool ValidateTarget(LocalTargetInfo TargetInfo)
        {
            return TargetInfo.HasThing && TargetInfo.Thing.def.IsApparel || TargetInfo.Thing.def.IsWeapon || TargetInfo.Thing.def.HasComp<CompEquippable>();
        }
    }
}