using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class PitfallSolutionWorker_Jump : PitfallSolutionWorker
    {
        public override int GetPawnBonus(Pawn pawn)
        {
            return DCUtility.GetStatBonus(pawn, StatDefOf.MoveSpeed);
        }

        public override bool TryTickProgress(Pawn pawn, Building_ObstacleBase obstacle)
        {
            float movementCapability = pawn.GetStatValue(StatDefOf.MoveSpeed);
            float speedFactor = Mathf.Clamp(movementCapability / 4.5f, 0.5f, 2f);
            currentWorkAmount += Mathf.RoundToInt(speedFactor * 3);

            if (currentWorkAmount >= def.workTicks)
            {
                CompleteSolution(pawn, RollCheck(pawn, 5, obstacle, out RollCheckOutcome rollCheckOutcome));
                return true;
            }
            return false;
        }

        public override string GetSuccessMessage(Pawn pawn)
        {
            return $"{pawn.LabelShort} successfully leaped over the pitfall trap!";
        }

        public override string GetFailureMessage(Pawn pawn)
        {
            return $"{pawn.LabelShort} failed to jump over the pitfall and fell in!";
        }

        public override void OnSuccess(Pawn pawn, Building_ObstacleBase pitfallTile)
        {
         
        }

        public override void OnFailure(Pawn pawn, Building_ObstacleBase pitfallTile)
        {
          
        }
    }
}
