namespace MagicAndMyths
{
    public class EnchantEffectDef_OnHitBase : EnchantEffectDef
    {
        public OnHitMode hitMode = OnHitMode.Melee;

        public string attackType => hitMode == OnHitMode.Range ? "ranged" : "melee";
    }


}