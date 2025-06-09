using Verse;

namespace MagicAndMyths
{
    public class Comp_PawnActionBase : ThingComp
    {
        public virtual bool CanPerformAction(Pawn pawn)
        {
            if (pawn.DeadOrDowned || pawn.Destroyed)
            {
                return false;
            }

            return true;
        }
    }
}