using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsColonistBehaviourAllowed : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn != null && pawn.IsControlledSummon(out Hediff_Undead undead))
            {
                return undead.CurrentState == SquadMemberState.AtEase;
            }
            return false;
        }

    }
}
