using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobGiver_SummonedCreatureFormationFollow : JobGiver_AIFollowMaster
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
            if (followee == null)
            {
                Log.Error($"Followee is null for {pawn.LabelShort}");
                return null;
            }

            if (!followee.Spawned)
            {
                Log.Message($"Followee {followee.LabelShort} is not spawned");
                return null;
            }

            Hediff_UndeadMaster undeadMaster = (Hediff_UndeadMaster)followee.health.hediffSet.GetFirstHediffOfDef(ThorDefOf.DeathKnight_UndeadMaster);

            if (undeadMaster == null)
            {
                return null;
            }

            var activeUndead = undeadMaster.GetActiveCreatures();
            if (activeUndead == null || !activeUndead.Contains(pawn))
            {
                return null;
            }
            if (!JobDriver_FormationFollow.FarEnoughAndPossibleToStartJob(pawn, followee, undeadMaster, GetRadius(pawn)))
            {
                return null;
            }

            Job job = JobMaker.MakeJob(ThorDefOf.Thor_FormationFollow, followee);
            job.expiryInterval = 200;
            job.followRadius = undeadMaster.FollowDistance;
            job.SetTarget(TargetIndex.A, followee);
            job.reportStringOverride = $"Following {undeadMaster.pawn.LabelCap} in formation";
            return job;
        }
    }
}
