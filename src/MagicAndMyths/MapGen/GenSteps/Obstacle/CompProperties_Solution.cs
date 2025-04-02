using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class CompProperties_Solution : CompProperties
    {
        public int workTicks = 100;
    }

    public abstract class CompSolution : ThingComp
    {
        protected Obstacle parentObstacle;
        public bool IsSolutionComplete = false;


        protected int currentWorkAmount = 0;
        public int CurrentWorkAmount
        {
            get
            {
                return currentWorkAmount;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parentObstacle = (Obstacle)parent;
        }

        public virtual bool CanAttemptSolution(Pawn pawn)
        {
            return !IsSolutionComplete;
        }

        public abstract bool TryTickProgress(Pawn pawn);

        public virtual void Reset()
        {
            IsSolutionComplete = false;
        }

        protected void CompleteSolution()
        {
            IsSolutionComplete = true;
            parentObstacle?.OnSolutionComplete();
        }

        protected void ReportProgress(float progressPercent)
        {
            parentObstacle?.ReportProgress(progressPercent);
        }

        protected void ReportFailure()
        {
            parentObstacle?.ReportFailure();
        }

        public virtual IEnumerable<Gizmo> GetSolutionGizmo()
        {
            yield break;
        }

        public virtual IEnumerable<FloatMenuOption> GetSolutionFloatOption(Pawn selectingPawn)
        {
            yield break;
        }
    }

}
