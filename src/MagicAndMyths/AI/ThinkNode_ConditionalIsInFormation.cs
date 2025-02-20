using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsInFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsControlledSummon() && pawn.GetMaster() != null)
            {
                Pawn master = pawn.GetMaster();
                Hediff_UndeadMaster undeadMaster = master.GetUndeadMaster();
                IntVec3 targetCell = undeadMaster.GetFormationPositionFor(pawn);
                return pawn.CanReach(targetCell, PathEndMode.Touch, Danger.Deadly);
            }
            return false;
        }

    }
}
