using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class OrbitalLaser : Thing
    {
        private Vector3 originPosition;
        private Vector3 targetPosition;
        private int currentTick = 0;
        private int totalDurationTicks = 180;
        private bool hasImpacted = false;
        private bool isFired = false;

        private int growingPhaseTicks = 120;
        private int shrinkingPhaseTicks = 60;
        private int impactTick;


        private float maxWidth = 4f;
        private float initialWidth = 0.5f;
        private float currentWidth = 0.5f;


        private int explosionRadius = 8;
        private int damageAmount = 150;
        private DamageDef damageDef = DamageDefOf.Bomb;

        private MoteDualAttached laserMote;

        public ThingDef OverrideMoteDef = null;
        private static readonly ThingDef DefaultLaserMote = DefDatabase<ThingDef>.GetNamed("MagicAndMyths_MoteMoonBeam");

        protected ThingDef ActualLaserMoteDef => OverrideMoteDef != null ? OverrideMoteDef : DefaultLaserMote;

        private Vector3 skyOffset = new Vector3(0f, 0f, 100f);
        private Vector3 groundOffset = Vector3.zero;


        private SimpleCurve WidthOverLife = new SimpleCurve(new List<CurvePoint> 
        { 
            new CurvePoint(0, 0),
            new CurvePoint(0.05f, 0.5f),
            new CurvePoint(1, 4f)
        });

        private float progress => (float)currentTick / growingPhaseTicks;

        private float adjustedProgress
        {
            get
            {
                float adjustedProgress;
                if (progress < 0.8f)
                {
                    adjustedProgress = progress * 0.5f;
                }
                else
                {
                    adjustedProgress = 0.4f + ((progress - 0.8f) * 3f);
                }

                return adjustedProgress;
            }
        }

        private List<IntVec3> _impactCells;
        private List<IntVec3> impactCells
        {
            get
            {
                if (_impactCells == null || _impactCells.Count == 0)
                {
                    _impactCells = GenRadial.RadialCellsAround(Position, explosionRadius, true).ToList();
                }

                return _impactCells;
            }
        }

        public void Fire(IntVec3 target, int ticksToImpact = 180)
        {
            targetPosition = target.ToVector3Shifted();
            currentTick = 0;
            this.totalDurationTicks = ticksToImpact;
            this.impactTick = (int)(growingPhaseTicks * 0.9f); 
            hasImpacted = false;
            currentWidth = initialWidth;
            isFired = true;
            CreateLaserBeam();
        }

        private void CreateLaserBeam()
        {
            if (ActualLaserMoteDef == null)
            {
                Log.Error("OrbitalLaser: No mote Thingdef found");
                return;
            }

            TargetInfo sourceTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);
            TargetInfo destTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);
            
            laserMote = MoteMaker.MakeInteractionOverlay(
                DefaultLaserMote,
                sourceTarget,
                destTarget);

            if (laserMote != null)
            {
                laserMote.UpdateTargets(
                    sourceTarget,
                    destTarget,
                    skyOffset,
                    groundOffset
                );
                laserMote.solidTimeOverride = totalDurationTicks;
            }
        }

        public override void Tick()
        {
            if (!isFired)
                return;

            currentTick++;

            if (currentTick <= growingPhaseTicks)
            {
                currentWidth = Mathf.Lerp(initialWidth, maxWidth, adjustedProgress);

                if (currentTick >= impactTick && !hasImpacted)
                {
                    Impact();
                }
            }
            else if (currentTick <= totalDurationTicks)
            {
                float shrinkProgress = (float)(currentTick - growingPhaseTicks) / shrinkingPhaseTicks;
                currentWidth = Mathf.Lerp(maxWidth, 0f, shrinkProgress * shrinkProgress);
            }
            else
            {
                CleanupAndDespawn();
            }

            UpdateMote();

            AddVisualEffects();
        }

        private void UpdateMote()
        {
            if (laserMote == null || laserMote.Destroyed)
                return;

            laserMote.linearScale = new Vector3(currentWidth, 1f, (laserMote.link1.LastDrawPos - laserMote.link1.Target.CenterVector3).MagnitudeHorizontal());
            laserMote.Maintain();

            TargetInfo sourceTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);
            TargetInfo destTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);

            Vector3 skyOffset = new Vector3(0f, 0f, 100f);
            Vector3 groundOffset = Vector3.zero;

            laserMote.UpdateTargets(
                sourceTarget,
                destTarget,
                skyOffset,
                groundOffset
            );
        }

        private void AddVisualEffects()
        {
            if (currentTick % 30 == 0)
            {
                Vector3 effectPos = targetPosition;
                FleckMaker.ThrowMicroSparks(effectPos, Map);
            }

            if (currentTick <= growingPhaseTicks)
            {
                if (currentTick % 5 == 0)
                {
                    Vector3 effectPos = targetPosition + new Vector3(0, 0, Rand.Range(10f, 50f));
                    FleckMaker.ThrowLightningGlow(effectPos, Map, Rand.Range(0.5f, currentWidth * 0.4f));
                }
            }

            if (currentTick >= impactTick - 10 && currentTick <= impactTick + 30)
            {
                if (currentTick % 3 == 0)
                {
                    float radius = (currentWidth * 0.7f) * Rand.Range(0.5f, 1.1f);
                    float angle = Rand.Range(0f, 360f);
                    Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 0f, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
                    FleckMaker.ThrowHeatGlow((targetPosition + offset).ToIntVec3(), Map, Rand.Range(1f, 2f));
                }
            }
        }

        private void Impact()
        {
            hasImpacted = true;

            StageVisualEffect.CreateStageEffect(impactCells, Map, Random.Range(3, 5), (IntVec3 cell) =>
            {
                EffecterDefOf.ImpactSmallDustCloud.Spawn(cell, Map);

                List<Thing> things = cell.GetThingList(Map).ToList();

                foreach (var t in things)
                {
                    if (t is Pawn || t is Building building)
                    {
                        DamageInfo damage = t.def.mineable ? new DamageInfo(DamageDefOf.Mining, damageAmount * 2, 1) : new DamageInfo(damageDef, damageAmount, 1);
                        t.TakeDamage(damage);
                    }
                }

                if (cell.GetTerrain(Map) == TerrainDefOf.WaterDeep || cell.GetTerrain(Map) == TerrainDefOf.WaterShallow)
                {
                    Map.terrainGrid.SetTerrain(cell, TerrainDefOf.Soil);
                }
                else
                {
                    Map.terrainGrid.SetTerrain(cell, TerrainDefOf.Sand);
                }

                if (Rand.Bool)
                {
                    FireUtility.TryStartFireIn(cell, Map, Rand.Value, this);
                }
            });
        }

        private void CleanupAndDespawn()
        {
            if (laserMote != null && !laserMote.Destroyed)
            {
                laserMote.Destroy();
            }

            this.DeSpawn();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (laserMote != null && !laserMote.Destroyed)
            {
                laserMote.Destroy();
            }

            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref targetPosition, "targetPosition");
            Scribe_Values.Look(ref currentTick, "currentTick");
            Scribe_Values.Look(ref totalDurationTicks, "totalDurationTicks");
            Scribe_Values.Look(ref hasImpacted, "hasImpacted");
            Scribe_Values.Look(ref isFired, "isFired");
            Scribe_Values.Look(ref currentWidth, "currentWidth");
            Scribe_Values.Look(ref impactTick, "impactTick");
        }
    }
}
