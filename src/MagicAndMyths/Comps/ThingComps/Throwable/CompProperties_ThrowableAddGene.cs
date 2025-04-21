using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableAddGene : CompProperties_ThrowableAffectPawns
    {
        public GeneDef geneDef;

        public CompProperties_ThrowableAddGene()
        {
            compClass = typeof(Comp_ThrowableAddGene);
        }
    }

    public class Comp_ThrowableAddGene : Comp_ThrowableAffectPawns
    {
        new public CompProperties_ThrowableAddGene Props => (CompProperties_ThrowableAddGene)props;

        protected override void AffectThing(Thing thing, Pawn throwingPawn)
        {
            Pawn pawn = thing as Pawn;
            if (pawn == null || Props.geneDef == null)
                return;

            if (pawn.genes != null && !pawn.genes.HasActiveGene(Props.geneDef))
            {
                pawn.genes.AddGene(Props.geneDef, true);
            }

        }
    }
}