using Verse;

namespace MagicAndMyths
{
    public static class DKUtil
    {
        public static bool IsUndead(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(MagicAndMythDefOf.DeathKnight_Undead);
        }
    }


}
