using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ProjectileCompProperties_AOEDamageAlongPath : ProjectileCompProperties
    {
        public DamageDef damageDef;
        public float damageAmount = 10f;
        public float radius = 2.5f;
        public int tickInterval = 2;
        public bool affectFriendlies = false;
        public bool growRadius = false;
        public float radiusGrowthRate = 0.1f;
        public float maxRadius = 10f;

        public ProjectileCompProperties_AOEDamageAlongPath()
        {
            compClass = typeof(ProjectileComp_AOEDamageAlongPath);
        }
    }

    public class ProjectileComp_AOEDamageAlongPath : ProjectileComp
    {
        private int tickCounter = 0;
        private int initialFlightTime = 0;

        public ProjectileCompProperties_AOEDamageAlongPath Props => (ProjectileCompProperties_AOEDamageAlongPath)props;

        public override void PostLaunch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            initialFlightTime = 0;
        }

        public override void PostFlightTick()
        {
            tickCounter++;
            if (tickCounter >= Props.tickInterval)
            {
                tickCounter = 0;
                DealAOEDamage();
            }
        }

        private float GetCurrentRadius()
        {
            if (!Props.growRadius || initialFlightTime <= 0)
            {
                return Props.radius;
            }

            float flightProgress = 1f - ((float)ParentAsProjectile.TicksUntilImpact / initialFlightTime);
            float radiusIncrease = flightProgress * Props.radiusGrowthRate * initialFlightTime;
            float currentRadius = Props.radius + radiusIncrease;

            return UnityEngine.Mathf.Min(currentRadius, Props.maxRadius);
        }

        private void DealAOEDamage()
        {
            Map map = parent.Map;
            if (map == null)
                return;

            IntVec3 center = parent.Position;
            float currentRadius = GetCurrentRadius();
            EffecterDefOf.MeatExplosionSmall.Spawn(center, map);

            TargetUtil.ApplyDamageInRadius(Props.damageDef, Props.damageAmount, 1, center, map, currentRadius, parent.Faction);
        }
    }
}
