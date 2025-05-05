using RimWorld;
using UnityEngine;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatFactor : EnchantEffectDef_PawnStat
    {
        public float statFactor = 1f;
        public EnchantEffectDef_PawnStatFactor()
        {
            workerClass = typeof(EnchantEffect_PawnStatFactor);
        }
        public override string EffectDescription
        {
            get
            {
                string statChangeString = statFactor != 1 ? $"x{statFactor}" : "";
                return $"Increases a Pawns {statToAffect.LabelCap} stat by {statChangeString}";
            }
        }

        public override string GetExplanationString()
        {
            return $"x{statFactor}";
        }
    }

    public class EnchantEffect_PawnStatFactor : EnchantWorker
    {
        EnchantEffectDef_PawnStatFactor StatDef => (EnchantEffectDef_PawnStatFactor)def;

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statFactor;
            }
            return 1f;
        }
    }

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

    public class EnchantEffectDef_PawnStatFactorDaylight : EnchantEffectDef_PawnStatFactorLerp
    {
        public EnchantEffectDef_PawnStatFactorDaylight()
        {
            workerClass = typeof(EnchantEffect_PawnStatFactorDaylight);
        }

        public override string EffectDescription
        {
            get
            {
                return $"Increases a Pawns {statToAffect.LabelCap} stat by x{minFactor} (night) to x{maxFactor} (day)";
            }
        }
    }

    public class EnchantEffect_PawnStatFactorDaylight : EnchantEffect_PawnStatFactorLerp
    {
        protected override float GetLerpValue()
        {
            if (EquippingPawn?.Map == null)
                return 0.5f;

            return EquippingPawn.Map.skyManager.CurSkyGlow;
        }
    }
}