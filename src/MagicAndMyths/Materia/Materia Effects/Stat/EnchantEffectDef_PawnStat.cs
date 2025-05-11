using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public enum StatModifierType
    {
        Offset,
        Factor
    }

    public class EnchantEffectDef_PawnStat : EnchantEffectDef
    {
        public StatDef statToAffect;
        public StatModifierType modifierType = StatModifierType.Offset;
        public float modifierValue = 0f;

        public EnchantEffectDef_PawnStat()
        {
            workerClass = typeof(EnchantEffect_PawnStat);
        }

        public override string EffectDescription
        {
            get
            {
                string statChangeString = "";
                if (modifierType == StatModifierType.Offset)
                {
                    statChangeString = modifierValue != 0 ? $"+{modifierValue}" : "";
                    return $"(Pawn)Increases {statToAffect.LabelCap} by {statChangeString}";
                }
                else
                {
                    statChangeString = modifierValue != 1 ? $"x{modifierValue}" : "";
                    return $"Increases a Pawns {statToAffect.LabelCap} stat by {statChangeString}";
                }
            }
        }
    }

    public class EnchantEffect_PawnStat : EnchantWorker
    {
        public EnchantEffectDef_PawnStat StatDef => (EnchantEffectDef_PawnStat)def;

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDef.statToAffect && StatDef.modifierType == StatModifierType.Offset)
            {
                return StatDef.modifierValue;
            }
            return 0f;
        }

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDef.statToAffect && StatDef.modifierType == StatModifierType.Factor)
            {
                return StatDef.modifierValue;
            }
            return 1f;
        }

        public override string GetExplanationString()
        {
            if (StatDef.modifierType == StatModifierType.Offset)
            {
                string sign = StatDef.modifierValue >= 0 ? "+" : "";
                return $"{sign}{StatDef.modifierValue:0.##}";
            }
            else
            {
                return $"x{StatDef.modifierValue:0.##}";
            }
        }
    }


}