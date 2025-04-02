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


    public class CompProperties_SolutionCraftingChallenge : CompProperties_Solution
    {
        public CompProperties_SolutionCraftingChallenge()
        {
            compClass = typeof(CompSolution_CraftingChallenge);
        }
    }


    public class CompSolution_CraftingChallenge : CompSolution
    {
       public CompProperties_Solution Props => (CompProperties_Solution)props;


        public override bool TryTickProgress(Pawn pawn)
        {
            currentWorkAmount++;
            if (currentWorkAmount >= Props.workTicks)
            {
                this.CompleteSolution();
                return true;
            }

            return false;
        }


        public override IEnumerable<FloatMenuOption> GetSolutionFloatOption(Pawn selectingPawn)
        {
            yield return new FloatMenuOption($"[Crafting:12] Craft a fake eye", () =>
            {
                Job job = JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_WorkSolution, this.parent);
                selectingPawn.jobs.StartJob(job, JobCondition.InterruptForced);
            });
        }
    }
}
