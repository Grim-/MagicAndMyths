using RimWorld;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class ProjectileCompProperties_ImpactLightningStrike : ProjectileCompProperties
    {
        public float strikeRadius = 3f;
        public int strikeDamage = 50;
        public DamageDef strikeDamageDef;

        public ProjectileCompProperties_ImpactLightningStrike()
        {
            compClass = typeof(ProjectileComp_ImpactLightningStrike);
        }
    }

    public class ProjectileComp_ImpactLightningStrike : ProjectileComp
    {
        public ProjectileCompProperties_ImpactLightningStrike Props => (ProjectileCompProperties_ImpactLightningStrike)props;

        public override void PreImpact(Thing hitThing, bool blockedByShield)
        {
            if (blockedByShield)
                return;

            Map map = parent.Map;
            IntVec3 loc = parent.Position;
            LightningStrike.GenerateLightningStrike(map, loc, Props.strikeRadius, out IEnumerable<IntVec3> affectedCells, Props.strikeDamage, 1, Props.strikeDamageDef ?? DamageDefOf.ElectricalBurn);
        }
    }
}
