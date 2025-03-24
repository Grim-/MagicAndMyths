//using RimWorld;
//using Verse;
//using Verse.AI;

//namespace MagicAndMyths
//{
//    public class ThinkNode_ConditionalSelfOrMasterHasTarget : ThinkNode_Conditional
//    {
//        protected override bool Satisfied(Pawn pawn)
//        {

//            if (pawn == null)
//            {
//                //Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn is null");
//                return false;
//            }

//            if (!pawn.Spawned)
//            {
//                //Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn is not spawned");
//                return false;
//            }

//            if (!pawn.IsPartOfSquad(out ISquadMember squadMember))
//            {
//                return false;
//            }

//            if (squadMember.SquadLeader.SquadLeaderPawn == null)
//            {
//                // Log.Message("Master is null");
//                return false;
//            }

//            if (pawn.mindState?.enemyTarget != null || squadMember.SquadLeader.SquadLeaderPawn.mindState?.enemyTarget != null)
//            {
//               // Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn or master has target");
//                return true;
//            }

//            if (pawn.Spawned && squadMember.SquadLeader.SquadLeaderPawn.Spawned)
//            {
//                if (pawn.Faction != null && PawnUtility.EnemiesAreNearby(pawn, 10, true))
//                {
//                    //Log.Message("ThinkNode_ConditionalSelfOrMasterHasTarget pawn or master has target nearby");
//                    return true;
//                }
//            }

//            return false;
//        }
//    }
//}
