using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatDaylight : EnchantEffectDef_PawnStatLerp
    {
        public EnchantEffectDef_PawnStatDaylight()
        {
            workerClass = typeof(EnchantEffect_PawnStatDaylight);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Increases a Pawns {statToAffect.LabelCap} stat by {minValue} (night) to {maxValue} (day)";
            }
        }
    }

    public class EnchantEffect_PawnStatDaylight : EnchantEffect_PawnStatLerp
    {
        new protected EnchantEffectDef_PawnStatDaylight Def => (EnchantEffectDef_PawnStatDaylight)def;

        public override float GetStatOffset(StatDef stat)
        {
            return Mathf.Lerp(Def.minValue, Def.maxValue, GetLerpValue());
        }

        protected override float GetLerpValue()
        {
            if (MateriaComp == null)
                return 0.5f;

            return MateriaComp.EquippedPawn.Map.skyManager.CurSkyGlow;
        }

        public override string GetExplanationString()
        {
            float progress = MateriaComp?.EquippedPawn?.Map?.skyManager?.CurSkyGlow ?? 0.5f;
            return $"{(Def.minValue >= 0 ? "+" : "")}{Def.minValue:0.##} to {(Def.maxValue >= 0 ? "+" : "")}{Def.maxValue:0.##} (Current: {(GetStatOffset(Def.statToAffect) >= 0 ? "+" : "")}{GetStatOffset(Def.statToAffect):0.##}, Day: {progress:P0})";
        }
    }
}