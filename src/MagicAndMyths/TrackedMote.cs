using Verse;

namespace MagicAndMyths
{
    public class TrackedMote
    {
        public MoteDualAttached Mote;
        public int RemainingTicks;
        public Thing SourceThing;
        public Thing TargetThing;

        public TrackedMote(MoteDualAttached mote, Thing source, Thing target, int lifetime)
        {
            Mote = mote;
            SourceThing = source;
            TargetThing = target;
            RemainingTicks = lifetime;
        }
    }
}
