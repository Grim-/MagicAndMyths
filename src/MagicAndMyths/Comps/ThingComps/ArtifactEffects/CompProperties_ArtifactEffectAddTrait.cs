using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectAddTrait : CompProperties
    {
        public TraitDef traitDef;
        public int traitDegree = 0;
        public bool forceTrait = true;

        public CompProperties_ArtifactEffectAddTrait()
        {
            compClass = typeof(Comp_ArtifactEffectAddTrait);
        }
    }

    public class Comp_ArtifactEffectAddTrait : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectAddTrait Props => (CompProperties_ArtifactEffectAddTrait)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (Props.traitDef == null)
                return;

            if (target.Thing != null)
            {
                if (target.Thing is Pawn targetPawn && targetPawn.story != null)
                {
                    if (!targetPawn.story.traits.HasTrait(Props.traitDef))
                    {
                        targetPawn.story.traits.GainTrait(new Trait(Props.traitDef, Props.traitDegree, Props.forceTrait));
                    }
                }
            }
        }

        public override bool CanApply(Pawn user, LocalTargetInfo target, Thing item, ref string reason)
        {
            if (target.Thing is Pawn targetPawn)
            {
                if (targetPawn.HostileTo(user))
                {
                    reason = "Target must be friendly";
                    return false;
                }
            }
            else
            {
                reason = "Target must be a pawn";
                return false;
            }

            return base.CanApply(user, target, item, ref reason);
        }
    }


}