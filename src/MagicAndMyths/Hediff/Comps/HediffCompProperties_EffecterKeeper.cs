using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_EffecterKeeper : HediffCompProperties
    {
        public EffecterDef effecterDef;

        public HediffCompProperties_EffecterKeeper()
        {
            compClass = typeof(HediffComp_EffecterKeeper);
        }
    }


    public class HediffComp_EffecterKeeper : HediffComp
    {
        private Effecter effecter;

        public HediffCompProperties_EffecterKeeper Props => (HediffCompProperties_EffecterKeeper)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (Props.effecterDef != null && parent.pawn.Spawned)
            {
                effecter = Props.effecterDef.Spawn();
                effecter.Trigger(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (effecter != null && parent.pawn.Spawned)
            {
                effecter.EffectTick(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
            else if (effecter == null && Props.effecterDef != null && parent.pawn.Spawned)
            {
                effecter = Props.effecterDef.Spawn();
                effecter.Trigger(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (effecter != null)
            {
                effecter.Cleanup();
                effecter = null;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Props.effecterDef != null && parent.pawn.Spawned)
            {
                effecter = Props.effecterDef.Spawn();
                effecter.Trigger(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
        }
    }

}