using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalNearMaster : ThinkNode_Conditional
    {
        public float MaxDistanceToMaster = 5;

        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.TryGetSquadLeader(out ISquadLeader squadLeader) && squadLeader.SquadLeader != null)
            {
                return pawn.Position.DistanceTo(squadLeader.SquadLeader.Position) <= MaxDistanceToMaster;
            }
            return false;
        }
    }
}
