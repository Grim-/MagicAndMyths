using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class PitfallSolutionWorker_BridgeGap : PitfallSolutionWorker
    {
        public override int GetPawnBonus(Pawn pawn)
        {
            return DCUtility.CalculateSkillBonus(pawn, SkillDefOf.Crafting);
        }

        public override void OnFailure(Pawn pawn, Building_ObstacleBase pitfallTile)
        {
            OnFailure(pawn, pitfallTile as Building_PitfallTile);
        }

        public override void OnSuccess(Pawn pawn, Building_ObstacleBase pitfallTile)
        {
            OnSuccess(pawn, pitfallTile as Building_PitfallTile);
        }
      
        public override bool TryTickProgress(Pawn pawn, Building_ObstacleBase obstacle)
        {
            currentWorkAmount++;
            if (currentWorkAmount >= def.workTicks)
            {
                CompleteSolution(pawn, RollCheck(pawn, 5, obstacle, out RollCheckOutcome outcome));
                return true;
            }

            return true;
        }

        // Context-specific messages for crafting
        public override string GetSuccessMessage(Pawn pawn)
        {
            return $"{pawn.LabelShort} successfully crafted a fake eye, allowing safe passage across the pitfall.";
        }

        public override string GetFailureMessage(Pawn pawn)
        {
            return $"{pawn.LabelShort} failed to craft a suitable fake eye. The trap mechanism wasn't fooled!";
        }


    }
}
