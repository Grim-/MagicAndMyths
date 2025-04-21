using Verse;

namespace MagicAndMyths
{
    public class EtherealPropertyWorker : ThingPropertyWorker
    {
        protected int TicksBeforeVanish = 2400;
        protected int TickCount = 0;

        public override void CompTick(Thing thing)
        {
            base.CompTick(thing);
            TickCount++;
            if (TickCount >= TicksBeforeVanish)
            {
                //remove if stored anywhere
                //despawn if spawned etc
            }
        }

        public override string GetDescription()
        {
            return "This thing is temporary, it will disappear after a time";
        }
    }
}
