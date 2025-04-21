using Verse;

namespace MagicAndMyths
{
    public class StatPropertyWorker : ThingPropertyWorker
    {
        public override void PostSpawnSetup(Thing thing, bool respawningAfterLoad)
        {
            base.PostSpawnSetup(thing, respawningAfterLoad);
        }

        public override string GetDescription()
        {
            return "Slowly regenerate health points over time";
        }
    }
}
