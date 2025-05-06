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
        protected override float GetLerpValue()
        {
            if (EquippingPawn?.Map == null)
                return 0.5f;

            return EquippingPawn.Map.skyManager.CurSkyGlow;
        }
    }
}