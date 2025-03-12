using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalHasSquadLeader : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.IsPartOfSquad(out ISquadMember squadLeader) && squadLeader != null;
        }
    }
}
