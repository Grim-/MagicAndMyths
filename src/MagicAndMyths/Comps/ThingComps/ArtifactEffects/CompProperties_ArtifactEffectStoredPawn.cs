using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectStoredPawn : CompProperties
    {
        public CompProperties_ArtifactEffectStoredPawn()
        {
            compClass = typeof(Comp_ArtifactEffectStoredPawn);
        }
    }


    public class Comp_ArtifactEffectStoredPawn : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectStoredPawn Props => (CompProperties_ArtifactEffectStoredPawn)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (target.Thing == null || !(target.Thing is Pawn targetPawn))
                return;

            Comp_PawnStorage pawnStorage = this.parent.GetComp<Comp_PawnStorage>();
            if (pawnStorage != null)
            {
                if (pawnStorage.HasStored)
                {
                    pawnStorage.ReleasePawn(target.Cell, user.Map);
                }
                else
                {
                    if (targetPawn != null)
                    {
                        pawnStorage.StorePawn(targetPawn);
                    }
                }
            }
        }
    }
}