using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsCalledToArms : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsPartOfSquad(out ISquadMember squadMember) || squadMember.SquadLeader == null)
            {
                return false;
            }

            return squadMember.CurrentState == SquadMemberState.CalledToArms;
        }
    }
}
