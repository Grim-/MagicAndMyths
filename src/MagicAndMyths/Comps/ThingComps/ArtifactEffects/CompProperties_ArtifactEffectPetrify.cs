using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectPetrify : CompProperties
    {

        public CompProperties_ArtifactEffectPetrify()
        {
            compClass = typeof(Comp_ArtifactEffectPetrify);
        }
    }


    public class Comp_ArtifactEffectPetrify : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectPetrify Props => (CompProperties_ArtifactEffectPetrify)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (target.Thing != null && target.Thing is Pawn pawn)
            {
                PetrifiedStatue.PetrifyPawn(MagicAndMythDefOf.MagicAndMyths_PetrifiedStatue, pawn, pawn.Position, user.Map);
            }
        }
    }
}