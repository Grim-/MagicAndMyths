using Verse;

namespace MagicAndMyths
{
    public class RegenerativePropertyWorker : ThingPropertyWorker
    {
        public override void CompTick(Thing thing)
        {
            base.CompTick(thing);

            if (thing.Spawned && thing.IsHashIntervalTick(2400))
            {
                if (thing.def.useHitPoints)
                {
                    thing.HitPoints += 2;
                }
            }
        }

        public override string GetDescription()
        {
            return "Slowly regenerate health points over time";
        }
    }
}
