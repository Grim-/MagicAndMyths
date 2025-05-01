using Verse;

namespace MagicAndMyths
{
    public class ComboReactionWorker_IncreaseParentSeverity : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null)
            {
                this.Comp.parent.Severity += this.Def.reactionProperties.severityRange.RandomInRange;
            }
        }
    }
}