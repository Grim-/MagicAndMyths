using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalIsCalledToArms : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            if (pawn == null || !pawn.IsControlledSummon(out Hediff_Undead undead) || undead.Master == null)
            {
                return false;
            }

            return undead.CalledToArms;
        }
    }
}
