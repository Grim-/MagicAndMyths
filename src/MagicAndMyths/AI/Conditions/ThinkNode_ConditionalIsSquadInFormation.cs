using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsSquadInFormation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null)
            {
                return false;
            }

            if (!pawn.IsPartOfSquad(out ISquadMember squadLeader))
            {
                return false;
            }

            if (squadLeader.SquadLeader == null)
            {
                return false;
            }

            return squadLeader.SquadLeader.InFormation;
        }
    }
}
