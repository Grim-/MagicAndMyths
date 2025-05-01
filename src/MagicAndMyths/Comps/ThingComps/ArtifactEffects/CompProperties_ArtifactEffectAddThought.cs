using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectAddThought : CompProperties
    {
        public ThoughtDef thoughtDef;

        public CompProperties_ArtifactEffectAddThought()
        {
            compClass = typeof(Comp_ArtifactEffectAddThought);
        }
    }

    public class Comp_ArtifactEffectAddThought : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectAddThought Props => (CompProperties_ArtifactEffectAddThought)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (Props.thoughtDef == null)
                return;

            if (target.Thing != null)
            {
                if (target.Thing is Pawn targetPawn && targetPawn.needs != null && targetPawn.needs.mood != null && targetPawn.needs.mood.thoughts != null)
                {
                    targetPawn.needs.mood.thoughts.memories.TryGainMemory(Props.thoughtDef);
                }
            }
        }
    }
}