using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatOffsetLerp : EnchantEffectDef_PawnStatOffset
    {
        public float minFactor = 0.5f;
        public float maxFactor = 2f;

        public EnchantEffectDef_PawnStatOffsetLerp()
        {
            workerClass = typeof(EnchantEffect_PawnStatOffsetLerp);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Increases a Pawns {statToAffect.LabelCap} stat by {minFactor} to {maxFactor}";
            }
        }
    }


    public class EnchantEffect_PawnStatOffsetLerp : EnchantEffect_PawnStatOffset
    {
        protected EnchantEffectDef_PawnStatOffsetLerp Def => (EnchantEffectDef_PawnStatOffsetLerp)def;

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == Def.statToAffect)
            {
                float lerpValue = GetLerpValue();
                float value = Mathf.Lerp(Def.minFactor, Def.maxFactor, lerpValue);
                return value;
            }

            return 0;
        }

        protected virtual float GetLerpValue()
        {
            return 0.5f;
        }

        public override string GetExplanationString()
        {
            return $"{(Def.minFactor >= 0 ? "+" : "")}{Def.minFactor:0.##} to {(Def.maxFactor >= 0 ? "+" : "")}{Def.maxFactor:0.##} (Current: {(GetStatOffset(Def.statToAffect) >= 0 ? "+" : "")}{GetStatOffset(Def.statToAffect):0.##})";
        }
    }
}