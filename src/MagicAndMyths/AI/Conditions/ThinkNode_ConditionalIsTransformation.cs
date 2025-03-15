using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsTransformation : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn != null && Current.Game.GetComponent<GameComp_Transformation>().IsTransformationPawn(pawn, out Pawn original);
        }
    }
}
