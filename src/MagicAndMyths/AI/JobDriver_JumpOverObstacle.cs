using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_JumpOverObstacle : JobDriver
    {
        private const TargetIndex ObstacleInd = TargetIndex.A;
        private const TargetIndex LandingCellInd = TargetIndex.B;

        private Building_ObstacleBase obstacle;
        private SolutionWorker jumpSolution;

        private Vector3 startPos;
        private Vector3 endPos;
        private IntVec3 landingCell;
        private float jumpProgress = 0f;
        private float jumpHeight = 1f;
        private int jumpDurationTicks = 15;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(ObstacleInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(ObstacleInd);

            yield return Toils_Goto.GotoThing(ObstacleInd, PathEndMode.Touch);

            Toil setupJump = new Toil();
            setupJump.initAction = () =>
            {
                obstacle = job.GetTarget(ObstacleInd).Thing as Building_ObstacleBase;
                if (obstacle == null)
                {
                    Log.Error("Obstacle is null in jump job");
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                jumpSolution = obstacle.WorkedSolution;
                if (jumpSolution == null)
                {
                    Log.Error("Jump solution is null");
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                IntVec3 obstaclePos = jumpSolution.parent.Position;
                IntVec3 pawnPos = pawn.Position;

                IntVec3 direction = obstaclePos - pawnPos;
                direction = new IntVec3(
                    Mathf.Clamp(direction.x, -1, 1),
                    0,
                    Mathf.Clamp(direction.z, -1, 1)
                );

                landingCell = obstaclePos + direction;

                job.SetTarget(LandingCellInd, landingCell);

            };
            yield return setupJump;

            Toil workToil = SolutionToilMaker.MakeWorkOnSolutionToil(
                ObstacleInd,
                () => obstacle,
                () => jumpSolution
            );

            yield return workToil;

            Toil performJump = new Toil();
            performJump.defaultCompleteMode = ToilCompleteMode.Delay;
            performJump.defaultDuration = jumpDurationTicks;
            performJump.tickAction = () =>
            {
                if (!jumpSolution.IsSolutionComplete(out bool success) || !success)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                jumpProgress += 1f / jumpDurationTicks;
                jumpProgress = Mathf.Clamp01(jumpProgress);

                if (jumpProgress >= 1f)
                {
                    pawn.Position = landingCell;
                    pawn.Notify_Teleported(false, true);
                    EndJobWith(JobCondition.Succeeded);
                }
            };

            performJump.AddFinishAction(() =>
            {
                if (jumpSolution.IsSolutionComplete(out bool success) && success)
                {
                    pawn.Position = landingCell;
                    pawn.Notify_Teleported(false, true);
                }
            });

            yield return performJump;
        }
    }
}
