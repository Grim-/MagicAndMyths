using Verse;

namespace MagicAndMyths
{
    public class ComboReactionWorker_DealDamage : ComboReactionWorker
    {
        public override void DoReaction(Pawn Pawn)
        {
            base.DoReaction(Pawn);

            if (this.Def.reactionProperties != null)
            {
                if (Def.reactionProperties.isAOE)
                {
                    HediffUtil.ApplyDamageInRadius(Def.reactionProperties.damageDef, 
                        Def.reactionProperties.damageRange.RandomInRange, 
                        Def.reactionProperties.armourpenRange.RandomInRange,
                        Pawn.Position, 
                        Pawn.Map, 
                        Def.reactionProperties.radius,
                        Pawn.Faction,
                        true, 
                        Pawn,
                        Def.reactionProperties.canTargetHostile,
                        Def.reactionProperties.canTargetFriendly,
                        Def.reactionProperties.canTargetNeutral);
                }
                else
                {
                    Pawn.TakeDamage(new DamageInfo(Def.reactionProperties.damageDef, Def.reactionProperties.damageRange.RandomInRange, Def.reactionProperties.armourpenRange.RandomInRange));
                }           
            }
        }
    }
}