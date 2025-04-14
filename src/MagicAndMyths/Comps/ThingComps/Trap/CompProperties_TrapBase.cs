using Verse;

namespace MagicAndMyths
{
    public class CompProperties_TrapBase : CompProperties
    {
        public CompProperties_TrapBase()
        {
            compClass = typeof(Comp_TrapBase);
        }

    }

    public class Comp_TrapBase : ThingComp
    {
        ///recieves signals from sensors and mechanisms
        ///may trigger triggercomps in response
        ///

        public virtual void OnTrapSensorTriggered(Pawn pawn)
        {

        }
    }
}