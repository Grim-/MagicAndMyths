using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_SpawnProjectileProxy : CompProperties_AbilityEffect
    {
        public ThingDef portalDef;     
        public ThingDef projectileDef; 
        public int portalCount = 5;
        public float portalDistance = 2f;
        public int portalLifeTime = 90;
        public IntRange timesToShoot = new IntRange(1, 1);
        public IntRange shotsPerBurst = new IntRange(1, 1);
        public IntRange ticksBetweenShots = new IntRange(15, 60);


        public CompProperties_SpawnProjectileProxy()
        {
            compClass = typeof(CompAbilityEffect_SpawnProjectileProxy);
        }
    }

    public class CompAbilityEffect_SpawnProjectileProxy : CompAbilityEffect
    {
        public new CompProperties_SpawnProjectileProxy Props => (CompProperties_SpawnProjectileProxy)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo destination)
        {
            base.Apply(target, destination);

            Pawn caster = parent.pawn;
            if (caster == null || !target.IsValid || caster.Map == null)
                return;

            Map map = caster.Map;

            for (int i = 0; i < Props.portalCount; i++)
            {
                float angle = (360f / Props.portalCount) * i;
                float distance = Props.portalDistance;

                float x = Mathf.Cos(Mathf.Deg2Rad * angle) * distance;
                float z = Mathf.Sin(Mathf.Deg2Rad * angle) * distance;

                IntVec3 portalPos = caster.Position + new IntVec3(Mathf.RoundToInt(x), 0, Mathf.RoundToInt(z));

                if (!portalPos.InBounds(map) || !portalPos.Walkable(map))
                    continue;

                ProjectileProxy portal = (ProjectileProxy)ThingMaker.MakeThing(Props.portalDef);
                GenSpawn.Spawn(portal, portalPos, map);
                portal.Init(caster, Props.projectileDef);
                portal.visualOverrideDef = ThingDefOf.Skull;
                portal.amountOfShots = Props.timesToShoot.RandomInRange;
                portal.amountPerBurst = Props.shotsPerBurst.RandomInRange;
                portal.ticksBetweenShots = Props.ticksBetweenShots.RandomInRange;

                portal.target = target;
            }
        }
    }

}
