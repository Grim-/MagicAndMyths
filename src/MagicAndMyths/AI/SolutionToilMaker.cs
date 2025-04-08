using System;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public static class SolutionToilMaker
    {
        public static Toil MakeWorkOnSolutionToil(TargetIndex obstacleIndex, Func<Building_ObstacleBase> obstacleGetter, Func<SolutionWorker> solutionGetter)
        {
            Toil workOnSolution = new Toil();
            workOnSolution.defaultCompleteMode = ToilCompleteMode.Never;


            workOnSolution.WithProgressBar(obstacleIndex, () => {
                var solution = solutionGetter();
                if (solution == null) 
                    return 0f;

                return (float)solution.CurrentWorkAmount / solution.def.workTicks;
            });

            workOnSolution.tickAction = () =>
            {
                Pawn actor = workOnSolution.actor;
                Building_ObstacleBase obstacle = obstacleGetter();
                SolutionWorker solution = solutionGetter();

                if (obstacle == null || solution == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                if (solution.IsSolutionComplete(out bool wasSuccessful))
                {
                    if (wasSuccessful)
                    {
                        solution.OnSuccess(actor, obstacle);
                        obstacle.OnSolutionComplete(actor);
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    }
                    else
                    {
                        solution.OnFailure(actor, obstacle);
                        obstacle.OnSolutionFailed(actor, 1);
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    return;
                }

                bool madeProgress = solution.TryTickProgress(actor, obstacle);

                if (solution.IsSolutionComplete(out wasSuccessful))
                {
                    if (wasSuccessful)
                    {
                        solution.OnSuccess(actor, obstacle);
                        // DONT end the job yet let derived jobs handle what happens next
                        workOnSolution.actor.jobs.curDriver.ReadyForNextToil();
                    }
                    else
                    {
                        solution.OnFailure(actor, obstacle);
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                }
            };

            return workOnSolution;
        }
    }
}
