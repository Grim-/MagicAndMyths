using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsInFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.TryGetSquadLeader(out ISquadLeader squadLeader) && squadLeader.SquadLeader != null)
            {
                IntVec3 targetCell = squadLeader.GetFormationPositionFor(pawn);
                return pawn.Position.InHorDistOf(targetCell, squadLeader.FollowDistance);
            }
            return false;
        }

    }
}
