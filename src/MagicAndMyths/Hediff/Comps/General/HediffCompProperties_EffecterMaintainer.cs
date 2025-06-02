using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_EffecterMaintainer : HediffCompProperties
    {
        public bool spawnAttached = true;
        public EffecterDef effecterDef;

        public HediffCompProperties_EffecterMaintainer()
        {
            compClass = typeof(HediffComp_EffecterMaintainer);
        }
    }


    public class HediffComp_EffecterMaintainer : HediffComp
    {
        private Effecter effecter;

        public HediffCompProperties_EffecterMaintainer Props => (HediffCompProperties_EffecterMaintainer)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Props.effecterDef != null && parent.pawn.Spawned)
            {
                if (Props.spawnAttached)
                {
                    effecter = Props.effecterDef.SpawnAttached(parent.pawn, parent.pawn.Map);
                }
                else
                {
                    effecter = Props.effecterDef.Spawn();
                }

                effecter.Trigger(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
        }

        public override void CompPostPostRemoved()
        {
            if (effecter != null)
            {
                effecter.ForceEnd();
                effecter.Cleanup();
                
                effecter = null;
            }
            base.CompPostPostRemoved();

        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (effecter != null && parent.pawn.Spawned)
            {
                effecter.EffectTick(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
        }



        public override void CompExposeData()
        {
            base.CompExposeData();
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Props.effecterDef != null && parent.pawn.Spawned)
            {
                if (Props.spawnAttached)
                {
                    effecter = Props.effecterDef.SpawnAttached(parent.pawn, parent.pawn.Map);
                }
                else
                {
                    effecter = Props.effecterDef.Spawn();
                }

                effecter.Trigger(new TargetInfo(parent.pawn), new TargetInfo(parent.pawn));
            }
        }
    }



}