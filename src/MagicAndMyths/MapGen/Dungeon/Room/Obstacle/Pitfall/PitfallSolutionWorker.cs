using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public abstract class PitfallSolutionWorker : SolutionWorker
    {
        public Building_PitfallTile PitfallTile;

        public virtual IEnumerable<FloatMenuOption> GetSolutionFloatOption(Pawn pawn, Building_ObstacleBase obstacle)
        {
            string checkInfo = GetCheckDescription(pawn);
            string optionLabel = $"[{GetRelevantSkillName()}] {def.label} {checkInfo}";

            yield return new FloatMenuOption(optionLabel, () =>
            {
                StartWorking(pawn, obstacle);
            });
        }

        protected virtual void StartWorking(Pawn pawn, Building_ObstacleBase obstacle)
        {
            Job job = JobMaker.MakeJob(def.jobDef, obstacle);
            obstacle.SetCurrentWorkedSolution(this);
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }
    }
}
