using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_AbilityZoneEffect : CompProperties_AbilityEffect
    {
        public ThingDef zoneDef;
        public int zoneLifetime = 1000;

        public CompProperties_AbilityZoneEffect()
        {
            compClass = typeof(AbilityZoneEffect);
        }
    }

    public abstract class AbilityZoneEffect : CompAbilityEffect
    {
        public CompProperties_AbilityZoneEffect Props => (CompProperties_AbilityZoneEffect)props;

        public virtual ActiveZone SpawnZone(IntVec3 SpawnPosition, List<IntVec3> ZoneCells, Map map)
        {
            if (Props.zoneDef == null)
            {
                return null;
            }

            if (ZoneCells.NullOrEmpty())
            {
                return null;
            }

            ActiveZone zone = (ActiveZone)ThingMaker.MakeThing(Props.zoneDef);
            zone.ZoneLifeTime = Props.zoneLifetime;
            zone.SetZoneCells(ZoneCells);

            GenSpawn.Spawn(zone, SpawnPosition, map);

            return zone;
        }
    }


}
