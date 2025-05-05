using RimWorld;

namespace MagicAndMyths
{
    public abstract class EnchantEffectDef_PawnStat : EnchantEffectDef
    {
        public StatDef statToAffect;
        public abstract string GetExplanationString();
    }

}