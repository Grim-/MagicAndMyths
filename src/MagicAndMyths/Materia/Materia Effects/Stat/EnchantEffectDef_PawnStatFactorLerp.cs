using RimWorld;
using UnityEngine;

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
        protected EnchantEffectDef_PawnStatFactorLerp LerpDef => (EnchantEffectDef_PawnStatFactorLerp)def;

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == LerpDef.statToAffect)
            {
                float lerpValue = GetLerpValue();
                return Mathf.Lerp(LerpDef.minFactor, LerpDef.maxFactor, lerpValue);
            }
            return 1f;
        }

        protected virtual float GetLerpValue()
        {
            return 0.5f;
        }
    }

}