using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class ThinkNode_ConditionalSquadHasTarget : ThinkNode_Conditional
    {
        public int checkTileRadius = 10;

        protected override bool Satisfied(Pawn pawn)
        {

            if (pawn == null)
            {
                return false;
            }

            if (!pawn.Spawned)
            {
                return false;
            }

            if (!pawn.TryGetSquadLeader(out ISquadLeader squadLeader))
            {
                Log.Message("squadLeader is null");
                return false;
            }
            if (pawn.mindState?.enemyTarget != null || squadLeader.SquadLeader.mindState?.enemyTarget != null)
            {
                return true;
            }

            if (pawn.Spawned && squadLeader.SquadLeader.Spawned)
            {
                if (PawnUtility.EnemiesAreNearby(pawn, checkTileRadius, true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
