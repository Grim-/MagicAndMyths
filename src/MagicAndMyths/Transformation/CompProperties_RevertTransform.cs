using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_RevertTransform : CompProperties_AbilityEffect
    {
        public CompProperties_RevertTransform()
        {
            compClass = typeof(CompAbilityEffect_RevertTransform);
        }
    }

    public class CompAbilityEffect_RevertTransform : CompAbilityEffect
    {
        new CompProperties_RevertTransform Props => (CompProperties_RevertTransform)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.pawn?.Map == null)
                return;

            var transformationComp = Current.Game.GetComponent<GameComp_Transformation>();

            if (transformationComp != null)
            {
                if (transformationComp.IsTransformationPawn(parent.pawn, out Pawn original))
                {
                    if (transformationComp.UnregisterTransformation(parent.pawn))
                    {
                          
                    }
                }
            }
        }
    }
}
