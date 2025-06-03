using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_ThrowFilthInRadius : CompProperties_AbilityEffect
    {
        public int radius = 3;
        public FloatRange chance = new FloatRange(1, 1);
        public ThingDef filthDef;

        public CompProperties_ThrowFilthInRadius()
        {
            compClass = typeof(CompAbilityEffect_ThrowFilthInRadius);
        }
    }


    public class CompAbilityEffect_ThrowFilthInRadius : CompAbilityEffect
    {
        CompProperties_ThrowFilthInRadius Props => (CompProperties_ThrowFilthInRadius)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            StageVisualEffect.CreateRadialStageEffect(this.parent.pawn.Position, Props.radius, this.parent.pawn.Map, 3, (IntVec3 cell, Map targetMap, int currentSEction) =>
            {
                if (Rand.Value <= Props.chance.RandomInRange)
                {                   
                    FilthMaker.TryMakeFilth(cell, targetMap, Props.filthDef);
                }
            });
        }
    }
}
