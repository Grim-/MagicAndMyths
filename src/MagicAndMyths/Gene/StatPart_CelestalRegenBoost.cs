using RimWorld;

namespace MagicAndMyths
{
    public class StatPart_CelestalRegenBoost : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            return string.Empty;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Pawn.Spawned && req.Pawn.Map != null)
            {
                val *= 1.5f * req.Pawn.Map.skyManager.CurSkyGlow;
            }
        }
    }
}