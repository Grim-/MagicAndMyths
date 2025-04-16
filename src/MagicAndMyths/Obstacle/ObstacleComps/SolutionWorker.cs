using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public abstract class SolutionWorker
    {
        public Thing parent;
        public PitfallSolutionDef def;
        protected int currentWorkAmount = 0;
        public int CurrentWorkAmount
        {
            get => currentWorkAmount;
        }

        protected bool _IsSolutionComplete = false;
        protected bool _WasSuccessful = false;
        public virtual bool CanPawnAttempt(Pawn pawn, Building_ObstacleBase obstacle)
        {
            return !obstacle.IsSolved;
        }

        public abstract int GetPawnBonus(Pawn pawn);

        public virtual bool TryTickProgress(Pawn pawn, Building_ObstacleBase obstacle)
        {
            currentWorkAmount++;
            if (currentWorkAmount >= def.workTicks)
            {
                bool success = RollCheck(pawn, this.def.difficultyLevel, obstacle, out RollCheckOutcome outCome);
                CompleteSolution(pawn, success);
                return true;
            }
            return false;
        }
        protected void CompleteSolution(Pawn pawn, bool success)
        {
            _IsSolutionComplete = true;
            _WasSuccessful = success;
        }

        public virtual bool IsSolutionComplete(out bool success)
        {
            success = _WasSuccessful;
            return _IsSolutionComplete;
        }

        public virtual string GetCheckDescription(Pawn pawn)
        {
            int dc = def.difficultyLevel;
            int bonus = GetPawnBonus(pawn);
            return DCUtility.FormatDCCheck(dc, bonus);
        }

        public virtual string GetRelevantSkillName()
        {
            return def.relevantSkill?.label ?? def.relevantStat?.label ?? "Skill";
        }
        public abstract void OnSuccess(Pawn pawn, Building_ObstacleBase pitfallTile);
        public abstract void OnFailure(Pawn pawn, Building_ObstacleBase pitfallTile);
        public virtual string GetSuccessMessage(Pawn pawn)
        {
            return $"{pawn.LabelShort} successfully completed a challenge.";
        }

        public virtual string GetFailureMessage(Pawn pawn)
        {
            return $"{pawn.LabelShort} failed to complete a challenge.";
        }


        /// <summary>
        /// Returns success if RollOutCome meets or beats DC
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="obstacle"></param>
        /// <returns></returns>
        protected bool RollCheck(Pawn pawn, int dc, Building_ObstacleBase obstacle, out RollCheckOutcome rollOutcome)
        {
            int bonus = GetPawnBonus(pawn);
            rollOutcome = DCUtility.RollAgainstDC(bonus);
            return rollOutcome.LastRoll >= dc;
        }

        public virtual void Reset()
        {
            currentWorkAmount = 0;
        }
    }
}
