using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_AttemptSolveDungeonObstacle : JobDriver
    {
        private const TargetIndex ObstacleInd = TargetIndex.A;
        private Building_ObstacleBase obstacle;
        private SolutionWorker solution;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(ObstacleInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(ObstacleInd);
            yield return Toils_Goto.GotoThing(ObstacleInd, PathEndMode.InteractionCell);

            Toil setupObstacle = new Toil();
            setupObstacle.initAction = () =>
            {
                Log.Message("Starting to work on obstacle solution");
                obstacle = job.GetTarget(ObstacleInd).Thing as Building_ObstacleBase;
                if (obstacle == null)
                {
                    Log.Error("Obstacle is null");
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                if (obstacle.WorkedSolution == null)
                {
                    Log.Error("Obstacle has no worked solution to be worked");
                    EndJobWith(JobCondition.Incompletable);
                }
                else
                {
                    solution = obstacle.WorkedSolution;
                }
            };
            yield return setupObstacle;

            yield return SolutionToilMaker.MakeWorkOnSolutionToil(
                ObstacleInd,
                () => obstacle,
                () => solution
            );
        }
    }
}
