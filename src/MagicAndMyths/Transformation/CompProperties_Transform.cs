using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_Transform : CompProperties_AbilityEffect
    {
        public PawnKindDef kindDef;

        public CompProperties_Transform()
        {
            compClass = typeof(CompAbilityEffect_Transform);
        }
    }

    public class CompAbilityEffect_Transform : CompAbilityEffect
    {
        new CompProperties_Transform Props => (CompProperties_Transform)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.pawn?.Map == null)
                return;
            var transformationComp = Current.Game.GetComponent<GameComp_Transformation>();

            if (transformationComp != null)
            {
                if (transformationComp.HasTransformationFor(parent.pawn))
                {
                    return;
                }
            }

            if (transformationComp.RegisterTransformation(parent.pawn, Props.kindDef, out Pawn transformationPawn))
            {
                if (transformationPawn.drafter != null)
                {
                    transformationPawn.drafter.Drafted = true;
                }

                if (transformationPawn.story != null)
                {
                    transformationPawn.story.skinColorOverride = parent.pawn.story.HairColor;
                }

                transformationPawn.Name = parent.pawn.Name;
            }
        }
    }
}
