using System.Collections.Generic;

namespace MagicAndMyths
{
    public class EnchantEffectDef_WeaponEnchant : EnchantEffectDef
    {
        public List<AdditionalGraphicData> graphics;

        public EnchantEffectDef_WeaponEnchant()
        {
            workerClass = typeof(EnchantEffect_WeaponEnchant);
        }
    }

    public class EnchantEffect_WeaponEnchant : EnchantWorker
    {
        EnchantEffectDef_WeaponEnchant Def => (EnchantEffectDef_WeaponEnchant)def;
        public override void PostDraw()
        {
            base.PostDraw();


           //CompDrawAdditionalGraphics.DrawAdditionalGraphics(Def.graphics, this.EquippingPawn);
        }
    }
}