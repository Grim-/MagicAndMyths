using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsSquadDefensive : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember != null)
            {
                return squadMember.SquadLeader.HostilityResponse == SquadHostility.Defensive;
            }
            return false;
        }
    }
}
