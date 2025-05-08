using Verse;

namespace MagicAndMyths
{
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
        protected EnchantEffectDef_PawnStatFactorDaylight Def => (EnchantEffectDef_PawnStatFactorDaylight)def;

        protected override float GetLerpValue()
        {
            if (MateriaComp == null)
                return 0.5f;

            return MateriaComp.EquippedPawn.Map.skyManager.CurSkyGlow;
        }

        public override string GetExplanationString()
        {
            float progress = GetLerpValue();
            return $"x{Def.minFactor:0.##} to x{Def.maxFactor:0.##} (Current: x{GetStatFactor(Def.statToAffect):0.##}, Day: {progress:P0})";
        }
    }


}