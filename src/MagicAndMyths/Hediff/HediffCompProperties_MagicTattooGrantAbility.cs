using RimWorld;

namespace MagicAndMyths
{
    public class HediffCompProperties_MagicTattooGrantAbility : HediffCompProperties_MagicTattooBase
    {
        public AbilityDef ability;

        public HediffCompProperties_MagicTattooGrantAbility()
        {
            compClass = typeof(HediffComp_MagicTattooGrantAbility);
        }
    }
}
