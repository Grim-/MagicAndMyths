using Verse;

namespace MagicAndMyths
{
    public class Hediff_Transformation : HediffWithComps
    {
        private Pawn OriginalPawn = null;

        public void SetOriginalPawn(Pawn Pawn)
        {
            OriginalPawn = Pawn;
        }

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            var transformationComp = Current.Game.GetComponent<GameComp_Transformation>();
            if (transformationComp != null)
            {
                if (transformationComp.IsTransformationPawn(this.pawn, out Pawn original))
                {
                    transformationComp.UnregisterTransformation(this.pawn);
                }

            }
            base.Notify_PawnDied(dinfo, culprit);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref OriginalPawn, "originalPawn");
        }
    }
}
