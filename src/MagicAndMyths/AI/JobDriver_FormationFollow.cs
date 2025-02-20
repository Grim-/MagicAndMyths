using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_FormationFollow : JobDriver_FollowClose
    {
        private Hediff_UndeadMaster Hediff_UndeadMaster => (Hediff_UndeadMaster)this.TargetPawnA.health.GetOrAddHediff(MagicAndMythDefOf.DeathKnight_UndeadMaster);

        private List<Pawn> GetAllActiveShadows()
        {
            return Hediff_UndeadMaster.GetActiveCreatures();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            Toil formationToil = ToilMaker.MakeToil("FormationFollow");

            formationToil.tickAction = () =>
            {
                Pawn followee = this.TargetA.Pawn;
                float followRadius = this.job.followRadius;

                if (!this.pawn.pather.Moving || this.pawn.IsHashIntervalTick(30))
                {
                    List<Pawn> activeShadows = GetAllActiveShadows();
                    int formationIndex = activeShadows.IndexOf(this.pawn);

                    if (formationIndex == -1)
                    {
                        base.EndJobWith(JobCondition.Errored);
                        return;
                    }

                    IntVec3 targetCell = FormationUtils.GetFormationPosition(
                        Hediff_UndeadMaster.FormationType,
                        followee.Position.ToVector3(),
                        followee.Rotation,
                        formationIndex,
                        activeShadows.Count);

                    if (this.pawn.Position != targetCell)
                    {
                        if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                targetCell = CellFinder.StandableCellNear(targetCell, this.Map, 5);
                                if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (!this.pawn.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly))
                        {
                            base.EndJobWith(JobCondition.Incompletable);
                            return;
                        }

                        this.pawn.pather.StartPath(targetCell, PathEndMode.OnCell);
                        this.locomotionUrgencySameAs = followee;
                    }
                    else if (!followee.pather.Moving)
                    {
                        base.EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                }
            };

            formationToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return formationToil;
        }

        public override bool IsContinuation(Job j)
        {
            return this.job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
        }

        public static bool FarEnoughAndPossibleToStartJob(Pawn follower, Pawn followee, Hediff_UndeadMaster undeadMaster, float radius)
        {
            if (radius <= 0f)
            {
                Log.ErrorOnce($"Checking formation follow job with radius <= 0. pawn={follower.ToStringSafe<Pawn>()}",
                    follower.thingIDNumber ^ 843254009);
                return false;
            }

            var shadows = undeadMaster.GetActiveCreatures();
            int index = shadows.IndexOf(follower);

            if (index == -1)
                return false;

            IntVec3 targetCell = FormationUtils.GetFormationPosition(
                                undeadMaster.FormationType,
                                followee.Position.ToVector3(),
                                followee.Rotation,
                                index,
                                shadows.Count);

            return follower.CanReach(targetCell, PathEndMode.OnCell, Danger.Deadly);
        }
    }
}
