using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class JobDriver_AttemptSolveDungeonObstacle : JobDriver
    {
        private const TargetIndex ObstacleInd = TargetIndex.A;

        // Reference to the obstacle component
        private Obstacle obstacle;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.GetTarget(ObstacleInd), job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(ObstacleInd);

            // Go to the obstacle
            yield return Toils_Goto.GotoThing(ObstacleInd, PathEndMode.InteractionCell);

            // Setup - find obstacle component
            Toil setupObstacle = new Toil();
            setupObstacle.initAction = () => {
                Obstacle thing = job.GetTarget(ObstacleInd).Thing as Obstacle;
                if (thing == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                obstacle = thing;

                // Check if this pawn can attempt any solution
                if (!obstacle.CanAttemptSolution(pawn))
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            };
            yield return setupObstacle;

            // Work on the obstacle
            Toil workOnObstacle = new Toil();
            workOnObstacle.tickAction = () => {
                if (obstacle == null || obstacle.IsSolved)
                {
                    EndJobWith(JobCondition.Succeeded);
                    return;
                }

                // Try to make progress using the obstacle's method
                bool madeProgress = obstacle.TryProgress(pawn);

                // If obstacle is now solved, end job
                if (obstacle.IsSolved)
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else if (!madeProgress)
                {
                    // No progress was made - pawn might not have right skills anymore
                    if (!obstacle.CanAttemptSolution(pawn))
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                }
            };

            // Add visual/audio feedback
            //workOnObstacle.WithEffect(() => EffecterDefOf.ConstructMetal, ObstacleInd);
            //workOnObstacle.PlaySustainerOrSound(() => SoundDefOf.Interact_Repair);

            workOnObstacle.defaultCompleteMode = ToilCompleteMode.Never;
            workOnObstacle.FailOnCannotTouch(ObstacleInd, PathEndMode.InteractionCell);

            yield return workOnObstacle;
        }
    }
}
