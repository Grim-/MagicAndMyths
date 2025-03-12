using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobGiver_SquadDefendPoint : JobGiver_AIDefendPoint
    {
        //protected override bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null)
        //{
        //    if (pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.DefendPoint != IntVec3.Invalid)
        //    {
        //        dest = squadMember.DefendPoint;
        //        return true;
        //    }
        //    return base.TryFindShootingPosition(pawn, out dest, verbToUse);
        //}

        protected override IntVec3 GetFlagPosition(Pawn pawn)
        {
            if (pawn.IsPartOfSquad(out ISquadMember squadMember) && squadMember.DefendPoint != IntVec3.Invalid)
            {
                return squadMember.DefendPoint;
            }

            return base.GetFlagPosition(pawn);
        }
    }
}
