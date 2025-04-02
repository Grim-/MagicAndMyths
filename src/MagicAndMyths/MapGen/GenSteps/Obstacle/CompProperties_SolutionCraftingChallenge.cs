using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
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
