using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TimedDetonator : CompProperties
    {
        public int ticksToDetonate = 300;
        public CompProperties_TimedDetonator()
        {
            compClass = typeof(Comp_TimedDetonator);
        }
    }

    public class Comp_TimedDetonator : ThingComp
    {
        CompProperties_TimedDetonator Props => (CompProperties_TimedDetonator)props;


        protected Comp_Explosive Explosive => this.parent.GetComp<Comp_Explosive>();
        protected Effecter fuseEffect = null;


        protected int tickCount = 0;

        public override void CompTick()
        {
            base.CompTick();
            if (Explosive == null)
            {
                return;
            }


            if (fuseEffect == null)
            {
                fuseEffect = EffecterDefOf.ConstructMetal.SpawnAttached(this.parent, this.parent.Map);
            }

            fuseEffect.EffectTick(this.parent, this.parent);

            tickCount++;
            if (tickCount >= Props.ticksToDetonate)
            {
                Explosive.Detonate();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tickCount, "tickCount");
        }
    }
}