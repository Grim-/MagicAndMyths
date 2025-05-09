﻿using RimWorld;

namespace MagicAndMyths
{
    public class EnchantEffectDef_PawnStatOffset : EnchantEffectDef_PawnStat
    {
        public float statOffset = 0f;

        public EnchantEffectDef_PawnStatOffset()
        {
            workerClass = typeof(EnchantEffect_PawnStatOffset);
        }

        public override string EffectDescription
        {
            get
            {
                string statChangeString = statOffset != 0 ? $"+{statOffset}" : "";
                return $"(Pawn)Increases {statToAffect.LabelCap} by {statChangeString}";
            }
        }


    }

    public class EnchantEffect_PawnStatOffset : EnchantWorker
    {
        EnchantEffectDef_PawnStatOffset StatDef => (EnchantEffectDef_PawnStatOffset)def;

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statOffset;
            }
            return 0f;
        }
        public override string GetExplanationString()
        {
            string sign = StatDef.statOffset >= 0 ? "+" : "";
            return $"{sign}{StatDef.statOffset:0.##}";
        }
    }
}