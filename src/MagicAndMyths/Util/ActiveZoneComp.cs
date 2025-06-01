using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public abstract class ActiveZoneComp : ThingComp
    {
        protected ActiveZone _ParentZone;
        public ActiveZone ParentZone
        {
            get
            {
                if (_ParentZone == null && this.parent is ActiveZone)
                {
                    _ParentZone = this.parent as ActiveZone;
                }

                return _ParentZone;
            }
        }

        public virtual void OnZoneSpawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {

        }

        public virtual void OnZoneDespawned(ActiveZone ParentZone, ref List<IntVec3> cells)
        {

        }

        public virtual void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {

        }

        protected List<Thing> GetCurrentThingsInZone(ref List<IntVec3> cells)
        {
            return TargetUtil.GetThingsInCells(cells, this.parent.Map);
        }
    }

    public class CompProperties_DamageActiveZoneComp : CompProperties
    {
        public int ticksBetweenDamage = 100;
        public int maxTargets = -1;

        public FloatRange damage = new FloatRange(1, 1);
        public DamageDef damageDef;

        public EffecterDef targetDamageEffecterDef = null;

        public CompProperties_DamageActiveZoneComp()
        {
            compClass = typeof(Damage_ActiveZoneComp);
        }
    }

    public class Damage_ActiveZoneComp : ActiveZoneComp
    {
        CompProperties_DamageActiveZoneComp Props => (CompProperties_DamageActiveZoneComp)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);

            if (ParentZone.IsHashIntervalTick(Props.ticksBetweenDamage))
            {
                int currentTargetCount = 0;

                List<Thing> things = ParentZone.GetCurrentThingsInZone(ref cells);
                foreach (var item in things)
                {
                    if (Props.maxTargets > 0 && currentTargetCount > Props.maxTargets)
                    {
                        break;
                    }

                    if (item.def.useHitPoints)
                    {
                        if (Props.targetDamageEffecterDef != null)
                        {
                            Props.targetDamageEffecterDef.Spawn(item, item.Map);
                        }

                        item.TakeDamage(new DamageInfo(Props.damageDef, Props.damage.RandomInRange));
                        currentTargetCount++;

                    }
                }
            }

        }
    }

    public class CompProperties_EffecterActiveZoneComp : CompProperties
    {
        public int ticksBetweenSpawns = 60;
        public float chancePerCell = 0.1f;
        public float totalCoverage = 1f;

        public EffecterDef effecterDef;

        public CompProperties_EffecterActiveZoneComp()
        {
            compClass = typeof(Effecter_ActiveZoneComp);
        }
    }

    public class Effecter_ActiveZoneComp : ActiveZoneComp
    {
        CompProperties_EffecterActiveZoneComp Props => (CompProperties_EffecterActiveZoneComp)props;

        public override void OnZoneTick(ActiveZone ParentZone, ref List<IntVec3> cells)
        {
            base.OnZoneTick(ParentZone, ref cells);

            if (ParentZone.IsHashIntervalTick(Props.ticksBetweenSpawns))
            {
                if (Props.effecterDef == null || cells.NullOrEmpty())
                {
                    return;
                }

                int targetCellCount = Mathf.RoundToInt(cells.Count * Props.totalCoverage);
                int spawnedCount = 0;

                List<IntVec3> shuffledCells = cells.InRandomOrder().ToList();

                foreach (var cell in shuffledCells)
                {
                    if (spawnedCount >= targetCellCount)
                    {
                        break;
                    }

                    if (Rand.Chance(Props.chancePerCell))
                    {
                        Props.effecterDef.Spawn(cell, ParentZone.Map);
                        spawnedCount++;
                    }
                }
            }
        }
    }
}
