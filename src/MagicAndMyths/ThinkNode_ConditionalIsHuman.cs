using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsHuman : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && pawn.RaceProps.Humanlike;
        }
    }
}
