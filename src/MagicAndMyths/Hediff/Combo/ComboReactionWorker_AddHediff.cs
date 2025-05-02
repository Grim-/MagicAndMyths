using Verse;

namespace MagicAndMyths
{
    public class AddHediffReactionProperties : ComboReactionProperties
    {
        public HediffDef hediff;
    }


    public class ComboReactionWorker_AddHediff : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is AddHediffReactionProperties hediffProperties)
            {

                if (hediffProperties.isAOE)
                {
                    AOEUtil.ApplyHediffInRadius(hediffProperties.hediff,
                        Pawn.Position,
                        Pawn.Map,
                        hediffProperties.radius,
                        Pawn.Faction, 
                        true,
                        hediffProperties.canTargetHostile,
                        hediffProperties.canTargetFriendly,
                        hediffProperties.canTargetNeutral);
                }
                else
                {
                    if (Pawn.health != null)
                    {
                        Pawn.health.GetOrAddHediff(hediffProperties.hediff);
                    }
                }

            }
        }

        public override string GetExplanation(Pawn Pawn)
        {
            string explanation = string.Empty;

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is AddHediffReactionProperties hediffProperties)
            {

                string aoe = hediffProperties.isAOE ? $"In a {hediffProperties.radius} radius." : "";
                explanation += $"Applies {hediffProperties.hediff.LabelCap} {aoe}";
            }

            return explanation;
        }
    }
}