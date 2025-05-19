using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_LightningRing : CompProperties_AbilityEffect
    {
        public int delayTicks = 15;
        public List<LightningRingConfig> rings = new List<LightningRingConfig>
        {
            new LightningRingConfig(4, 3f),
            new LightningRingConfig(5, 5f),
            new LightningRingConfig(7, 7f)
        };

        public CompProperties_LightningRing()
        {
            compClass = typeof(Comp_LightningRing);
        }
    }

    public class Comp_LightningRing : CompAbilityEffect
    {
        new CompProperties_LightningRing Props => (CompProperties_LightningRing)props;


        private LightningRingBehavior LightningRing = null;
        private IntVec3 origin;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.pawn?.Map == null)
                return;
            LightningRing = new LightningRingBehavior(Props.rings, target.Cell, this.parent.pawn.Map, Props.delayTicks);
        }

        public override void CompTick()
        {
            base.CompTick();

            if (LightningRing != null)
            {
                LightningRing.Tick();

                if (LightningRing.IsFinished)
                {
                    LightningRing = null;
                }
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return parent.pawn?.Map != null && base.Valid(target, throwMessages);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Deep.Look(ref LightningRing, "lightningRing");
        }
    }
}
