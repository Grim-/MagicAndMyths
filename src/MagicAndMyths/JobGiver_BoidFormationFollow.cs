using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobGiver_BoidFormationFollow : JobGiver_AIFollowMaster
    {

        protected override Pawn GetFollowee(Pawn pawn)
        {
            if (pawn.IsControlledSummon())
            {
                return pawn.GetMaster();
            }
            return null;
        }

        protected override float GetRadius(Pawn pawn)
        {
            return pawn.GetMaster().GetUndeadMaster().FollowDistance;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn followee = GetFollowee(pawn);
            if (followee == null || !followee.Spawned)
            {
                return null;
            }

            Hediff_UndeadMaster undeadMaster = followee.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster) as Hediff_UndeadMaster;
            if (undeadMaster == null)
            {
                return null;
            }

            var activeUndead = undeadMaster.GetActiveCreatures();
            if (activeUndead == null || !activeUndead.Contains(pawn))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(ThorDefOf.Thor_BoidFormationFollow, followee);
            job.expiryInterval = 100;
            job.followRadius = undeadMaster.FollowDistance;
            job.SetTarget(TargetIndex.A, followee);
            return job;
        }
    }
}
