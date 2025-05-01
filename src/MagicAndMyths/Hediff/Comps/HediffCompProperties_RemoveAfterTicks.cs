using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_RemoveAfterTicks : HediffCompProperties
    {
        public int ticks = 2400;

        public HediffCompProperties_RemoveAfterTicks()
        {
            compClass = typeof(HediffComp_RemoveAfterTicks);
        }
    }

    public class HediffComp_RemoveAfterTicks : HediffComp
    {
        new public HediffCompProperties_RemoveAfterTicks Props => (HediffCompProperties_RemoveAfterTicks)props;
        protected int ticks = 0;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            ticks++;
            if (ticks >= Props.ticks)
            {
                this.Pawn.health.RemoveHediff(this.parent);
            }
        }


        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks");
        }

    }
}