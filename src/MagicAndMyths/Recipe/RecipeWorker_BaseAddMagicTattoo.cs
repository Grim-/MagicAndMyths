using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class RecipeWorker_BaseAddMagicTattoo : Recipe_Surgery
    {
        MagicTattooRecipeDef Def => (MagicTattooRecipeDef)this.recipe;

        protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);

            TattooDef faceTattoo = Def.tattooDef;
            pawn.style.FaceTattoo = faceTattoo;
        }
    }
}
