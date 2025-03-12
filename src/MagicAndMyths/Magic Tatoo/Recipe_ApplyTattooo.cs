using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class Recipe_ApplyTattoo : Recipe_AddHediff
    {
        ApplyTattooRecipeDef Def => (ApplyTattooRecipeDef)this.recipe;

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);


            if (pawn.style != null)
            {
                pawn.style.BodyTattoo = Def.tattooDef;
                Log.Message($"APplying {Def.addsHediff}");
                //pawn.health.GetOrAddHediff(Def.tattooDef.hediff);
                pawn.style.Notify_StyleItemChanged();
            }
        }

        public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
        {
            return true;
        }

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            return new List<BodyPartRecord>()
            {
                { null }
            };
        }
    }
}
