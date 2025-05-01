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
                    HediffUtil.ApplyHediffInRadius(hediffProperties.hediff,
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
    }
}