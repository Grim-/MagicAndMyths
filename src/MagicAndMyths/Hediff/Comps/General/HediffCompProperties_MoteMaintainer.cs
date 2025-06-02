using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_MoteMaintainer : HediffCompProperties
    {
        public bool spawnAttached = true;
        public ThingDef moteDef;

        public HediffCompProperties_MoteMaintainer()
        {
            compClass = typeof(HediffComp_MoteMaintainer);
        }
    }

    public class HediffComp_MoteMaintainer : HediffComp
    {
        private Mote effecter;

        public HediffCompProperties_MoteMaintainer Props => (HediffCompProperties_MoteMaintainer)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Props.moteDef != null && parent.pawn.Spawned)
            {
                SpawnMote();
            }
        }

        public override void CompPostPostRemoved()
        {
            if (effecter != null)
            {
                effecter.Destroy();
                effecter = null;
            }
            base.CompPostPostRemoved();
        }

        protected void SpawnMote()
        {
            effecter = (Mote)ThingMaker.MakeThing(Props.moteDef);
            if (Props.spawnAttached)
            {
                if (effecter is MoteAttached moteAttached)
                {
                    moteAttached.Attach(parent.pawn);
                }
            }
            effecter = (Mote)GenSpawn.Spawn(effecter, parent.pawn.Position, parent.pawn.Map);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (effecter != null && parent.pawn.Spawned)
            {
                effecter.Maintain();
            }
        }



        public override void CompExposeData()
        {
            base.CompExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (Props.moteDef != null && parent.pawn.Spawned)
                {
                    SpawnMote();
                }
            }
        }
    }

}