using Verse;

namespace MagicAndMyths
{
    public class InvisiblePropertyWorker : ThingPropertyWorker
    {
        public override bool IsInvisible(Thing thing)
        {
            return true;
        }

        public override string GetDescription()
        {
            return "This object is invisible.";
        }
    }
}
