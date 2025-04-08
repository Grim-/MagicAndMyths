using RimWorld;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public abstract class Building_ObstacleBase : Building
    {
        protected bool _IsSolved = false;
        public virtual bool IsSolved => false;
        public abstract SolutionWorker WorkedSolution { get; }
        public virtual bool CanBeWorked => WorkedSolution == null;
        public abstract void SetCurrentWorkedSolution(SolutionWorker compSolution);

        public virtual void StartWorkingSolution(Pawn pawn, SolutionWorker compSolution)
        {
            SetCurrentWorkedSolution(compSolution);
            Job job = JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_WorkSolution, this);
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }

        public virtual void OnSolutionComplete(Pawn pawn)
        {

        }

        public virtual void OnSolutionFailed(Pawn pawn, float failScale)
        {

        }

        public virtual void ForceSolve()
        {
            _IsSolved = true;

        }

        public virtual void Reset()
        {
            _IsSolved = false;
        }
    }
}
