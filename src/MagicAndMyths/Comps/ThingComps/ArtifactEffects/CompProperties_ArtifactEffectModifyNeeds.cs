using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectModifyNeeds : CompProperties
    {
        public NeedDef need;
        public FloatRange amount = new FloatRange(1f, 1f);

        public CompProperties_ArtifactEffectModifyNeeds()
        {
            compClass = typeof(Comp_ArtifactEffectModifyNeeds);
        }
    }

    public class Comp_ArtifactEffectModifyNeeds : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectModifyNeeds Props => (CompProperties_ArtifactEffectModifyNeeds)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (Props.need == null)
                return;


            if (target.Thing != null)
            {
                if (target.Thing is Pawn targetPawn)
                {
                    if (targetPawn.needs != null)
                    {
                        if (targetPawn.needs.TryGetNeed(Props.need) is Need need)
                        {
                            need.CurLevel += Props.amount.RandomInRange;
                        }
                    }
                }
            }
        }
    }


}