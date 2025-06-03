using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityStartFiresInRadius : CompProperties_AbilityEffect
    {
        public int radius = 3;
        public FloatRange chance = new FloatRange(1, 1);
        public IntRange fireSize= new IntRange(10, 10);

        public CompProperties_AbilityStartFiresInRadius()
        {
            compClass = typeof(CompAbilityEffect_AbilityStartFiresInRadius);
        }
    }


    public class CompAbilityEffect_AbilityStartFiresInRadius : CompAbilityEffect
    {
        CompProperties_AbilityStartFiresInRadius Props => (CompProperties_AbilityStartFiresInRadius)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            StageVisualEffect.CreateRadialStageEffect(this.parent.pawn.Position, Props.radius, this.parent.pawn.Map, 3, (IntVec3 cell, Map targetMap, int currentSEction) =>
            {
                if (Rand.Value <= Props.chance.RandomInRange)
                {
                    FireUtility.TryStartFireIn(cell, targetMap, Props.fireSize.RandomInRange, this.parent.pawn);
                }
            });
        }
    }
}
