using RimWorld;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_PortalBarrage : CompProperties_AbilityEffect
    {
        public ThingDef portalDef;     
        public ThingDef projectileDef; 
        public int portalCount = 5;
        public float portalDistance = 2f;

        public CompProperties_PortalBarrage()
        {
            compClass = typeof(CompAbilityEffect_PortalBarrage);
        }
    }

    public class CompAbilityEffect_PortalBarrage : CompAbilityEffect
    {
        public new CompProperties_PortalBarrage Props => (CompProperties_PortalBarrage)props;

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
                portal.Init(caster, Props.projectileDef, 90);
                portal.target = target;
                portal.ticksToFire = Rand.Range(15, 60);
            }
        }
    }

}
