using EMF;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityStunInRadius : CompProperties_AbilityEffect
    {
        public int radius = 3;
        public IntRange stunTicks = new IntRange(100, 200);
        public FriendlyFireSettings fireSettings = FriendlyFireSettings.HostileOnly();

        public CompProperties_AbilityStunInRadius()
        {
            compClass = typeof(CompAbilityEffect_AbilityStunInRadius);
        }
    }


    public class CompAbilityEffect_AbilityStunInRadius : CompAbilityEffect
    {
        CompProperties_AbilityStunInRadius Props => (CompProperties_AbilityStunInRadius)props;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Map map = this.parent.pawn.Map;

            StageVisualEffect.CreateRadialStageEffect(this.parent.pawn.Position, Props.radius, this.parent.pawn.Map, 3, (IntVec3 cell, Map targetMap, int currentSEction) =>
            {
                EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, targetMap);
                Pawn pawn = cell.GetFirstPawn(targetMap);
                if (pawn != null && pawn.CanTargetThing(this.parent.pawn.Faction, Props.fireSettings))
                {
                    pawn.stances?.stunner?.StunFor(Props.stunTicks.RandomInRange, this.parent.pawn);
                }
            });
        }
    }




}
