using RimWorld;
using System.Collections.Generic;

namespace MagicAndMyths
{
    public class EnchantEffectDef_EquipmentStat : EnchantEffectDef
    {
        public StatDef statToAffect;
        public float statFactor = 1f;
        public float statOffset = 0f;

        public List<StatModifier> StatOffsets
        {
            get
            {
                return new List<StatModifier>
            {
                new StatModifier { stat = statToAffect, value = statOffset }
            };
            }
        }

        public List<StatModifier> StatFactors
        {
            get
            {
                return new List<StatModifier>
            {
                new StatModifier { stat = statToAffect, value = statFactor }
            };
            }
        }

        public override string EffectDescription => $"(Equipment)Increases {statToAffect.LabelCap} by +{statOffset} or {statFactor}%";
    }

    public class EnchantEffect_EquipmentStat : EnchantWorker
    {
        EnchantEffectDef_EquipmentStat StatDef => (EnchantEffectDef_EquipmentStat)def;

        public override float GetStatFactor(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statFactor;
            }
            return 1f;
        }

        public override float GetStatOffset(StatDef stat)
        {
            if (stat == StatDef.statToAffect)
            {
                return StatDef.statOffset;
            }
            return 0f;
        }
    }
}