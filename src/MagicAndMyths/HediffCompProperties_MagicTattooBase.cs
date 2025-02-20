using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_MagicTattooBase : HediffCompProperties
    {
        public TattooDef tattooDef;

        public HediffCompProperties_MagicTattooBase()
        {
            compClass = typeof(HediffComp_MagicTattooBase);
        }
    }
}
