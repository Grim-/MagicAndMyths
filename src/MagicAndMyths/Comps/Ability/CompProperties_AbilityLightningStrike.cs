using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityLightningStrike : CompProperties_AbilityEffect
    {
        public float strikeRadius = 3f;
        public int strikeDamage = 50;
        public DamageDef strikeDamageDef;

        public CompProperties_AbilityLightningStrike()
        {
            compClass = typeof(CompAbilityEffect_LightningStrike);
        }
    }


    public class CompAbilityEffect_LightningStrike : CompAbilityEffect
    {
        new CompProperties_AbilityLightningStrike Props => (CompProperties_AbilityLightningStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (parent.pawn?.Map == null)
                return;

            LightningStrike.GenerateLightningStrike(parent.pawn.Map, target.Cell, Props.strikeRadius, out IEnumerable<IntVec3> affectedCells, Props.strikeDamage, 1, Props.strikeDamageDef);
        }
    }
}
