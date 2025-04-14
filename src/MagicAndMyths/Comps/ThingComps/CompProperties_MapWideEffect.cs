using Verse;

namespace MagicAndMyths
{
    public abstract class CompProperties_MapWideEffect : CompProperties
    {
        public IntRange ticksBetweenEffect = new IntRange(1250, 3000);
    }


    public abstract class Comp_MapWideEffect : ThingComp
    {
        private CompProperties_MapWideEffect Props => (CompProperties_MapWideEffect)props;
        protected int tickCounter = 0;
        protected virtual int tickInterval => Props.ticksBetweenEffect.RandomInRange;


        public override void CompTick()
        {
            base.CompTick();
            tickCounter++;
            if (tickCounter >= tickInterval)
            {
                DoMapWideEffect();
                tickCounter = 0;
            }
        }


        protected virtual void DoMapWideEffect()
        {

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tickCounter, "tickCounter", 0);
        }
    }

}