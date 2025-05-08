using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatFactorLerp : EnchantEffectDef_PawnStatFactor
    {
        public float minFactor = 0.5f;
        public float maxFactor = 2f;

        public EnchantEffectDef_PawnStatFactorLerp()
        {
            workerClass = typeof(EnchantEffect_PawnStatFactorLerp);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Increases a Pawns {statToAffect.LabelCap} stat by x{minFactor} to x{maxFactor}";
            }
        }
    }


    public class EnchantEffect_PawnStatFactorLerp : EnchantEffect_PawnStatFactor
    {
        protected EnchantEffectDef_PawnStatFactorLerp Def => (EnchantEffectDef_PawnStatFactorLerp)def;

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == Def.statToAffect)
            {
                float lerpValue = GetLerpValue();
                return Mathf.Lerp(Def.minFactor, Def.maxFactor, lerpValue);
            }
            return 1f;
        }

        protected virtual float GetLerpValue()
        {
            return 0.5f;
        }


        public override string GetExplanationString()
        {
            return $"x{Def.minFactor:0.##} to x{Def.maxFactor:0.##} (Current: x{GetStatFactor(Def.statToAffect):0.##})";
        }
    }
}