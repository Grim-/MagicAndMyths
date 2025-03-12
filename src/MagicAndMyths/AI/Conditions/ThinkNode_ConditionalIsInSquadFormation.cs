using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsInSquadFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember != null)
            {
                IntVec3 targetCell = squadMember.SquadLeader.GetFormationPositionFor(pawn);
                return pawn.Position.InHorDistOf(targetCell, 1);
            }
            return false;
        }

    }
}
