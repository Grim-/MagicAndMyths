using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatLerp : EnchantEffectDef_PawnStat
    {
        public float minValue = 0.5f;
        public float maxValue = 2f;

        public EnchantEffectDef_PawnStatLerp()
        {
            workerClass = typeof(EnchantEffect_PawnStatLerp);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Increases a Pawns {statToAffect.LabelCap} stat by {minValue} to {maxValue}";
            }
        }
    }

    public class EnchantEffect_PawnStatLerp : EnchantEffect_PawnStat
    {
        protected EnchantEffectDef_PawnStatLerp LerpDef => (EnchantEffectDef_PawnStatLerp)def;

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == LerpDef.statToAffect && LerpDef.modifierType == StatModifierType.Offset)
            {
                float lerpValue = GetLerpValue();
                float value = Mathf.Lerp(LerpDef.minValue, LerpDef.maxValue, lerpValue);
                return value;
            }
            return 0f;
        }

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == LerpDef.statToAffect && LerpDef.modifierType == StatModifierType.Factor)
            {
                float lerpValue = GetLerpValue();
                float value = Mathf.Lerp(LerpDef.minValue, LerpDef.maxValue, lerpValue);
                return value;
            }
            return 1f;
        }

        protected virtual float GetLerpValue()
        {
            return 0.5f;
        }

        public override string GetExplanationString()
        {
            if (LerpDef.modifierType == StatModifierType.Offset)
            {
                float currentValue = GetStatOffset(LerpDef.statToAffect);
                return $"{(LerpDef.minValue >= 0 ? "+" : "")}{LerpDef.minValue:0.##} to {(LerpDef.maxValue >= 0 ? "+" : "")}{LerpDef.maxValue:0.##} (Current: {(currentValue >= 0 ? "+" : "")}{currentValue:0.##})";
            }
            else
            {
                float currentValue = GetStatFactor(LerpDef.statToAffect);
                return $"x{LerpDef.minValue:0.##} to x{LerpDef.maxValue:0.##} (Current: x{currentValue:0.##})";
            }
        }
    }
}