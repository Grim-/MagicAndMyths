using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowableStartTimerOnImpact : CompProperties_Throwable
    {

        public CompProperties_ThrowableStartTimerOnImpact()
        {
            compClass = typeof(Comp_ThrowableStartTimerOnImpact);
        }
    }
    public class Comp_ThrowableStartTimerOnImpact : Comp_Throwable
    {
        CompProperties_ThrowableStartTimerOnImpact Props => (CompProperties_ThrowableStartTimerOnImpact)props;
        protected Comp_Explosive Explosive => this.parent.GetComp<Comp_Explosive>();

        public override void OnRespawn(IntVec3 position, Thing thing, Map map, Pawn throwingPawn)
        {
            base.OnRespawn(position, thing, map, throwingPawn);

            if (this.parent.TryGetComp<Comp_TimedDetonator>(out var comp))
            {
                comp.StartTimer();
            }

        }
    }
}