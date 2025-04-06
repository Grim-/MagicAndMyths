using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{

    /// <summary>
    /// An obstacle has solutions and mechanisms
    /// solutions are ways to circumvent or complete what is required of the obstacle, an obstacle will have multiple solutions
    /// mechanisms are simply components that do things in reaction to other things, start a timer, trigger a trap, whatever
    /// </summary>

    public class Obstacle : ThingWithComps
    {
        // Cached comps
        private List<CompSolution> solutionComps;
        private List<CompMechanism> mechanismComps;


        public bool IsSolved = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            solutionComps = new List<CompSolution>();
            mechanismComps = new List<CompMechanism>();

            foreach (var comp in AllComps)
            {
                if (comp is CompSolution solution)
                    solutionComps.Add(solution);

                if (comp is CompMechanism mechanism)
                    mechanismComps.Add(mechanism);
            }
        }


        public bool CanAttemptSolution(Pawn pawn)
        {
            if (IsSolved)
                return false;

            foreach (var solution in solutionComps)
            {
                if (solution.CanAttemptSolution(pawn))
                    return true;
            }

            return false;
        }

        public bool TryProgress(Pawn pawn)
        {
            if (IsSolved)
                return false;

            bool anyProgress = false;

            foreach (var solution in solutionComps)
            {
                if (solution.IsSolutionComplete)
                    continue;

                if (solution.CanAttemptSolution(pawn))
                {
                    bool madeProgress = solution.TryTickProgress(pawn);
                    anyProgress |= madeProgress;

                    if (solution.IsSolutionComplete)
                    {
                        OnSolutionComplete();
                        return true;
                    }
                }
            }

            return anyProgress;
        }

        public void OnSolutionComplete()
        {
            if (IsSolved)
                return;

            IsSolved = true;

            foreach (var mechanism in mechanismComps)
            {
                mechanism.OnSolutionComplete();
            }


            Log.Message("completed obstacle solution");
        }

        public void ReportProgress(float progressPercent)
        {
            if (IsSolved)
                return;

            foreach (var mechanism in mechanismComps)
            {
                mechanism.OnProgress(progressPercent);
            }
        }

        public void ReportFailure()
        {
            if (IsSolved)
                return;


            foreach (var mechanism in mechanismComps)
            {
                mechanism.OnSolutionFailed();
            }
        }

        public void ForceSolve()
        {
            OnSolutionComplete();
        }

        public void Reset()
        {
            IsSolved = false;

            foreach (var solution in solutionComps)
            {
                solution.Reset();
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (var item in base.GetFloatMenuOptions(selPawn))
            {
                yield return item;
            }

            foreach (var comp in solutionComps)
            {
                foreach (var item in comp.GetSolutionFloatOption(selPawn))
                {
                    yield return item;
                }            
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref IsSolved, "isSolved", false);
        }
    }
}
