using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ArtifactEffectAddGene : CompProperties
    {
        public GeneDef geneDef;
        public bool addAsXenogene = true;

        public CompProperties_ArtifactEffectAddGene()
        {
            compClass = typeof(Comp_ArtifactEffectAddGene);
        }
    }

    public class Comp_ArtifactEffectAddGene : Comp_BaseAritfactEffect
    {
        private CompProperties_ArtifactEffectAddGene Props => (CompProperties_ArtifactEffectAddGene)props;

        public override void Apply(Pawn user, LocalTargetInfo target, Thing item)
        {
            if (Props.geneDef == null)
                return;

            if (target.Thing != null)
            {
                if (target.Thing is Pawn targetPawn && targetPawn.genes != null)
                {
                    if (!targetPawn.genes.HasActiveGene(Props.geneDef))
                    {
                        targetPawn.genes.AddGene(Props.geneDef, Props.addAsXenogene);
                    }
                }
            }
        }
    }


}