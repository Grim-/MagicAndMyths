using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class MagicTattooRecipeDef : RecipeDef
    {
        public TattooDef tattooDef;

        public MagicTattooRecipeDef()
        {
            workerClass = typeof(RecipeWorker_BaseAddMagicTattoo);
        }
    }
}
