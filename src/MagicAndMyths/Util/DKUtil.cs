using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace MagicAndMyths
{
    public static class DKUtil
    {
        public static bool IsUndead(this Pawn pawn)
        {
            return pawn.health.hediffSet.HasHediff(MagicAndMythDefOf.DeathKnight_Undead);
        }

        public static Hediff_Transformation AddTransformationHediff(this Pawn pawn)
        {
            if (!pawn.health.hediffSet.HasHediff(MagicAndMythDefOf.MagicAndMyths_Transformation))
            {
                return (Hediff_Transformation)pawn.health.AddHediff(MagicAndMythDefOf.MagicAndMyths_Transformation, null);
            }

            return null;
        }
        public static void StartDKQuest(this Pawn pawn)
        {
            Slate newSlate = new Slate();
            newSlate.Set<string>("colonistQuestSubject", pawn.ThingID);
            newSlate.Set<string>("colonistQuestSubjectName", pawn.Label);
            QuestUtility.GenerateQuestAndMakeAvailable(MagicAndMythDefOf.Quest_DeathKnightStartingPath, newSlate);
        }
    }
}
