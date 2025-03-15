using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class Recipe_ApplyTattoo : Recipe_AddHediff
    {
        ApplyTattooRecipeDef Def => (ApplyTattooRecipeDef)this.recipe;

        public override bool CompletableEver(Pawn surgeryTarget)
        {
            return true;
        }
        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            return true;
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return thing is Pawn pawn && pawn.style != null;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

            if (pawn.style != null)
            {
                if (Def.tattooDef.tattooType == TattooType.Body)
                {
                    pawn.style.BodyTattoo = Def.tattooDef;
                }
                else
                {
                    pawn.style.FaceTattoo = Def.tattooDef;
                }
               
                Log.Message($"APplying {Def.addsHediff}");
                pawn.style.Notify_StyleItemChanged();
            }
        }
    }
}
