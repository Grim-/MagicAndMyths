using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatOffsetDaylight : EnchantEffectDef_PawnStatOffsetLerp
    {
        public EnchantEffectDef_PawnStatOffsetDaylight()
        {
            workerClass = typeof(EnchantEffect_PawnStatOffsetDaylight);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Increases a Pawns {statToAffect.LabelCap} stat by {minFactor} (night) to {maxFactor} (day)";
            }
        }
    }

    public class EnchantEffect_PawnStatOffsetDaylight : EnchantEffect_PawnStatOffsetLerp
    {
        new protected EnchantEffectDef_PawnStatOffsetDaylight Def => (EnchantEffectDef_PawnStatOffsetDaylight)def;

        public override float GetStatOffset(StatDef stat)
        {
            return Mathf.Lerp(Def.minFactor, Def.maxFactor, GetLerpValue());
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
            return $"{(Def.minFactor >= 0 ? "+" : "")}{Def.minFactor:0.##} to {(Def.maxFactor >= 0 ? "+" : "")}{Def.maxFactor:0.##} (Current: {(GetStatOffset(Def.statToAffect) >= 0 ? "+" : "")}{GetStatOffset(Def.statToAffect):0.##}, Day: {progress:P0})";
        }
    }
}