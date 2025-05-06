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
}