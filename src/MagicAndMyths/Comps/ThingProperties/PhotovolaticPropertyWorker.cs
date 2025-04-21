using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class PhotovolaticPropertyWorker : ThingPropertyWorker
    {
        public override void CompTick(Thing thing)
        {
            base.CompTick(thing);

            if (thing.IsHashIntervalTick(2400))
            {
                if (CanRecharge())
                {
                    OnRechargeTick();
                }
            }
        }


        protected virtual void OnRechargeTick()
        {

        }


        protected virtual bool CanRecharge()
        {
            return !parent.Position.Roofed(parent.Map) && GenCelestial.IsDaytime(GenCelestial.CurCelestialSunGlow(parent.Map));
        }
    }
}
