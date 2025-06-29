using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class ConditionalStat_NeedsNight : ConditionalStatAffecter
    {
        public override string Label => throw new System.NotImplementedException();

        public override bool Applies(StatRequest req)
        {
            return req.Pawn != null && req.Pawn.Map.skyManager.CurSkyGlow < 0.2f;
        }
    }

}