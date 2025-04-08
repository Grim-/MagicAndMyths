using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    /// <summary>
    /// This allows different types of building bases to work as obstacles.
    /// </summary>
    //public class Obstacle : ThingWithComps, IObstacle
    //{
    //    // Cached comps
    //    private List<CompSolution> solutionComps;
    //    private List<CompMechanism> mechanismComps;

    //    public List<CompSolution> SolutionComps => solutionComps;

    //    public List<CompMechanism> MechanismComps => mechanismComps;

    //    private bool isSolved = false;

    //    public bool IsSolved => isSolved;


    //    private float _SolutionProgress = 0;
    //    public float SolutionProgress =>Mathf.Clamp01(_SolutionProgress);

    //    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    //    {
    //        base.SpawnSetup(map, respawningAfterLoad);
    //        solutionComps = new List<CompSolution>();
    //        mechanismComps = new List<CompMechanism>();
    //        foreach (var comp in AllComps)
    //        {
    //            if (comp is CompSolution solution)
    //                solutionComps.Add(solution);
    //            if (comp is CompMechanism mechanism)
    //                mechanismComps.Add(mechanism);
    //        }
    //    }

    //    public virtual bool CanAttemptSolution(Pawn pawn)
    //    {
    //        if (IsSolved)
    //            return false;
    //        foreach (var solution in solutionComps)
    //        {
    //            if (solution.CanAttemptSolution(pawn))
    //                return true;
    //        }
    //        return false;
    //    }

    //    public virtual bool TryTickProgress(Pawn pawn)
    //    {
    //        if (IsSolved)
    //            return false;
    //        bool anyProgress = false;
    //        foreach (var solution in solutionComps)
    //        {
    //            if (solution.IsSolutionComplete)
    //                continue;
    //            if (solution.CanAttemptSolution(pawn))
    //            {
    //                bool madeProgress = solution.TryTickProgress(pawn);
    //                anyProgress |= madeProgress;
    //                if (solution.IsSolutionComplete)
    //                {
    //                    OnSolutionComplete(pawn);
    //                    return true;
    //                }
    //            }
    //        }
    //        return anyProgress;
    //    }

    //    public virtual void OnSolutionComplete(Pawn pawn)
    //    {
    //        if (IsSolved)
    //            return;
    //        isSolved = true;
    //        foreach (var mechanism in mechanismComps)
    //        {
    //            mechanism.OnSolutionComplete();
    //        }

    //        Log.Message("Completed obstacle solution");
    //    }

    //    public virtual void ReportProgress(float progressPercent)
    //    {
    //        if (IsSolved)
    //            return;
    //        foreach (var mechanism in mechanismComps)
    //        {
    //            mechanism.OnProgress(progressPercent);
    //        }
    //    }
    //    public virtual void ReportSuccess(Pawn pawn)
    //    {

    //    }

    //    public virtual void ReportFailure(Pawn pawn)
    //    {
    //        if (IsSolved)
    //            return;
    //        foreach (var mechanism in mechanismComps)
    //        {
    //            mechanism.OnSolutionFailed();
    //        }
    //    }

    //    public virtual void ForceSolve()
    //    {
    //        isSolved = true;
    //    }

    //    public virtual void Reset()
    //    {
    //        isSolved = false;
    //        foreach (var solution in solutionComps)
    //        {
    //            solution.Reset();
    //        }
    //    }

    //    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
    //    {
    //        if (IsSolved)
    //            yield break;
    //        foreach (var comp in solutionComps)
    //        {
    //            foreach (var item in comp.GetSolutionFloatOption(selPawn))
    //            {
    //                yield return item;
    //            }
    //        }
    //    }

    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        Scribe_Values.Look(ref isSolved, "isSolved", false);
    //    }

   
    //}
}
