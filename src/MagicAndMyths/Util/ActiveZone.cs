using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ActiveZone : ThingWithComps
    {
        protected List<IntVec3> ZoneCells = new List<IntVec3>();

        protected List<ActiveZoneComp> ZoneComps => this.GetComps<ActiveZoneComp>().ToList();

        public int ZoneLifeTime = 1000;
        protected int ZoneLifetimeTicks = 0;
        protected DamageDef DamageDef;
        protected FloatRange Damage;
        public int ticksBetweenDamage = 100;
        public int maxTargets = -1;
        public EffecterDef targetDamageEffecterDef = null;
        public void SetZoneCells(List<IntVec3> cells)
        {
            ZoneCells = cells.ToList();
        }


        public void SetDamage(DamageDef damageDef, FloatRange damage)
        {
            DamageDef = damageDef;
            Damage = damage;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }

            foreach (var item in ZoneComps)
            {
                item.OnZoneSpawned(this, ref ZoneCells);
            }
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);

            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }

            foreach (var item in ZoneComps)
            {
                item.OnZoneDespawned(this, ref ZoneCells);
            }
        }

        public override void Tick()
        {
            base.Tick();


            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }


            ZoneLifetimeTicks++;

            if (ZoneLifetimeTicks >= ZoneLifeTime)
            {
                if (!this.Destroyed)
                {
                    this.Destroy();
                    return;
                }
            }


            if (DamageDef != null)
            {
                if (this.IsHashIntervalTick(ticksBetweenDamage))
                {
                    int currentTargetCount = 0;

                    List<Thing> things = GetCurrentThingsInZone(ref ZoneCells);
                    foreach (var item in things)
                    {
                        if (maxTargets > 0 && currentTargetCount > maxTargets)
                        {
                            break;
                        }

                        //EffecterDefOf.ImpactDustCloud.Spawn(item, item.Map);

                        //if (targetDamageEffecterDef != null)
                        //{
                        //    targetDamageEffecterDef.Spawn(item, item.Map);
                        //}

                        item.TakeDamage(new DamageInfo(DamageDef, Damage.RandomInRange));
                        currentTargetCount++;
                    }


                    ZoneCells.ForEach(x =>
                    {
                        EffecterDefOf.ImpactSmallDustCloud.Spawn(x, this.Map, 0.4f);
                    });
                }
            }

            foreach (var item in ZoneComps)
            {
                item.OnZoneTick(this, ref ZoneCells);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            //base.DrawAt(drawLoc, flip);


            if (ZoneCells == null || ZoneCells.Empty())
            {
                return;
            }

            GenDraw.DrawFieldEdges(ZoneCells);
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            //base.DynamicDrawPhaseAt(phase, drawLoc, flip);

        }
        public List<Thing> GetCurrentThingsInZone(ref List<IntVec3> cells)
        {
            return TargetUtil.GetThingsInCells(cells, this.Map);
        }
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref ZoneCells, "ZoneCells", LookMode.Reference);
            Scribe_Values.Look(ref ZoneLifetimeTicks, "ZoneLifetimeTicks");
            Scribe_Defs.Look(ref DamageDef, "DamageDef");
            Scribe_Values.Look(ref Damage, "Damage");
        }
    }
}
