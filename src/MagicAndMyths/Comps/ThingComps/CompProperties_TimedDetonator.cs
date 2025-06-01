using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TimedDetonator : CompProperties
    {
        public Vector3 offset = Vector3.zero;
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
        protected bool started = false;


        public void StartTimer()
        {
            started = true;
        }

        public void StopTimer(bool reset = false)
        {
            started = false;

            if (reset)
            {
                tickCount = 0;
            }

            if (fuseEffect != null)
            {
                fuseEffect.Cleanup();
                fuseEffect = null;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Explosive == null || !started)
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
            Scribe_Values.Look(ref started, "started");
            Scribe_Values.Look(ref tickCount, "tickCount");
        }
    }
}