using Verse;

namespace MagicAndMyths
{
    public class HediffCompProperties_TorturedSoul : HediffCompProperties
    {
        public HediffCompProperties_TorturedSoul()
        {
            compClass = typeof(HediffComp_TorturedSoul);
        }
    }


    public class HediffComp_TorturedSoul : HediffComp
    {
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn.ideo != null)
            {
                //change ideology to dominante player ideo
            }
        }
    }
}