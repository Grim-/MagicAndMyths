using Verse;

namespace MagicAndMyths
{
    public class StunReactionProperties : ComboReactionProperties
    {
        public IntRange stunTicks = new IntRange(300, 300);
    }

    public class ComboReactionWorker_Stun : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null && this.Def.reactionProperties is StunReactionProperties reactionProperties)
            {
                if (Pawn.stances?.stunner != null)
                {
                    Pawn.stances.stunner.StunFor(reactionProperties.stunTicks.RandomInRange, Pawn);
                }
            }
        }
    }
}