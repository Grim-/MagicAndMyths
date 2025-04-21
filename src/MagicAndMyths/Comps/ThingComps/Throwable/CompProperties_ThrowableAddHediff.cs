using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableAddHediff : CompProperties_ThrowableAffectPawns
    {
        public HediffDef hediffDef;
        public float severity = 1.0f;
        public BodyPartDef bodyPartDef = null;

        public CompProperties_ThrowableAddHediff()
        {
            compClass = typeof(Comp_ThrowableAddHediff);
        }
    }

    public class Comp_ThrowableAddHediff : Comp_ThrowableAffectPawns
    {
        new public CompProperties_ThrowableAddHediff Props => (CompProperties_ThrowableAddHediff)props;

        protected override void AffectThing(Thing thing, Pawn throwingPawn)
        {
            Pawn pawn = thing as Pawn;
            if (pawn == null || Props.hediffDef == null)
                return;

            pawn.health.AddHediff(Props.hediffDef, null, null, null);
        }
    }

}