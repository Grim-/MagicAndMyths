using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class ApplyTattooRecipeDef : RecipeDef
    {
        public MagicTattooDef tattooDef;

        public ApplyTattooRecipeDef()
        {
            workerClass = typeof(Recipe_ApplyTattoo);
        }
    }
}
