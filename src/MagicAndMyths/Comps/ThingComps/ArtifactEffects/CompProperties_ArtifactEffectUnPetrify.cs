using Verse;

namespace MagicAndMyths
{



    public class CompProperties_ArtifactEffectUnPetrify : CompProperties
    {
        public CompProperties_ArtifactEffectUnPetrify()
        {
            compClass = typeof(Comp_ArtifactEffectUnPetrify);
        }
    }

    public class Comp_ArtifactEffectUnPetrify : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectUnPetrify Props => (CompProperties_ArtifactEffectUnPetrify)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (target.Thing != null && target.Thing is PetrifiedStatue petrifiedStatue)
            {
                petrifiedStatue.UnpetrifyThing();
            }
        }

        public override bool CanApply(Pawn user, LocalTargetInfo TargetInfo, Thing item, ref string reason)
        {
            PetrifiedStatue petrifiedStatue = TargetInfo.Thing as PetrifiedStatue;

            if (petrifiedStatue == null)
            {
                reason = "Must target a petrified statue.";
                return false;
            }
            return base.CanApply(user, TargetInfo, item, ref reason);
        }
    }
}