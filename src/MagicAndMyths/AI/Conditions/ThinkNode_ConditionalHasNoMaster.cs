using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalHasNoMaster : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.SquadLeader == null;
        }
    }
}
