using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class CompMechanism : ThingComp
    {
        public abstract void Trigger();

        public virtual void Reset()
        {
         
        }
    }


}
