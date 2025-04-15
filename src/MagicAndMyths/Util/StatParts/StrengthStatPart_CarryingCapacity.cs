using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class StrengthStatPart_CarryingCapacity : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                float carryCapacity = pawn.GetStatValue(StatDefOf.CarryingCapacity) * DCUtility.Strength_CarryCapacityModifier / StatDefOf.CarryingCapacity.defaultBaseValue;
                val *= Mathf.Lerp(DCUtility.Strength_CarryCapacityRange.min, DCUtility.Strength_CarryCapacityRange.max, carryCapacity);
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.HasThing && req.Thing is Pawn pawn)
            {
                float carryCapacity = pawn.GetStatValue(StatDefOf.CarryingCapacity) * DCUtility.Strength_CarryCapacityModifier / StatDefOf.CarryingCapacity.defaultBaseValue;
                float factor = Mathf.Lerp(DCUtility.Strength_CarryCapacityRange.min, DCUtility.Strength_CarryCapacityRange.max, carryCapacity);
                return "Manipulation: x" + factor.ToStringPercent();
            }
            return null;
        }
    }
}