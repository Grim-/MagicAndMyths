using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class OrbitalLaser : Thing
    {
        // Core properties
        private Vector3 originPosition;
        private Vector3 targetPosition;
        private int currentTick = 0;
        private int totalDurationTicks = 180;
        private bool hasImpacted = false;
        private bool isFired = false;

        // Animation timing
        private int growingPhaseTicks = 120;
        private int shrinkingPhaseTicks = 60;
        private int impactTick;

        // Visual effect parameters
        private float maxWidth = 15f;
        private float initialWidth = 0.5f;
        private float currentWidth = 0.5f;

        // Damage properties
        private int explosionRadius = 8;
        private int damageAmount = 150;
        private DamageDef damageDef = DamageDefOf.Bomb;

        // The actual mote effect that represents the laser beam
        private MoteDualAttached laserMote;
        private static readonly ThingDef LaserMoteDef = DefDatabase<ThingDef>.GetNamed("mote_LightningStrike");

        // Visual elements
        private Vector2 startSize = new Vector2(0.5f, 0.5f);
        private Vector2 finalSize = new Vector2(15f, 15f);

        public override Vector2 DrawSize
        {
            get
            {
                return new Vector2(maxWidth * 2, maxWidth * 2);
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

            // Create the mote effect
            CreateLaserBeam();
        }

        private void CreateLaserBeam()
        {
            if (LaserMoteDef == null)
            {
                Log.Error("OrbitalLaser: Mote_GraserBeamBase def not found");
                return;
            }

            // Create TargetInfo objects for ground positions
            TargetInfo sourceTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);
            TargetInfo destTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);

            Vector3 skyOffset = new Vector3(0f, 0f, 100f); 
            Vector3 groundOffset = Vector3.zero;

            // Create the dual attached mote
            laserMote = MoteMaker.MakeInteractionOverlay(
                LaserMoteDef,
                sourceTarget,
                destTarget);

            if (laserMote != null)
            {
                // Set initial width
                laserMote.Scale = currentWidth;

                // IMMEDIATELY update the targets with proper offsets
                laserMote.UpdateTargets(
                    sourceTarget,
                    destTarget,
                    skyOffset,  // This adds the height to the source
                    groundOffset
                );

                // Make sure the mote stays visible
                laserMote.solidTimeOverride = totalDurationTicks;
            }
        }

        public override void Tick()
        {
            if (!isFired)
                return;

            currentTick++;

            // Growing phase
            if (currentTick <= growingPhaseTicks)
            {
                float progress = (float)currentTick / growingPhaseTicks;

                // Akira laser starts thin and slowly grows
                // Then suddenly expands near the impact moment
                float adjustedProgress;
                if (progress < 0.8f)
                {
                    // Slow growth for first 80%
                    adjustedProgress = progress * 0.5f;
                }
                else
                {
                    // Fast expansion for last 20%
                    adjustedProgress = 0.4f + ((progress - 0.8f) * 3f);
                }

                currentWidth = Mathf.Lerp(initialWidth, maxWidth, adjustedProgress);

                // Check for impact
                if (currentTick >= impactTick && !hasImpacted)
                {
                    Impact();
                }
            }
            // Shrinking phase
            else if (currentTick <= totalDurationTicks)
            {
                float shrinkProgress = (float)(currentTick - growingPhaseTicks) / shrinkingPhaseTicks;
                currentWidth = Mathf.Lerp(maxWidth, 0f, shrinkProgress * shrinkProgress); // Ease out quad
            }
            // End
            else
            {
                CleanupAndDespawn();
            }

            // Update mote properties
            UpdateMote();

            // Add visual effects based on current phase
            AddVisualEffects();
        }
        private void UpdateMote()
        {
            if (laserMote == null || laserMote.Destroyed)
                return;

            // Update mote scale based on current width
            laserMote.Scale = currentWidth;

            // Maintain the mote so it doesn't fade
            laserMote.Maintain();

            // Create TargetInfo objects for the ground positions
            TargetInfo sourceTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);
            TargetInfo destTarget = new TargetInfo(targetPosition.ToIntVec3(), Map, false);

            Vector3 skyOffset = new Vector3(0f, 0f, 100f);
            Vector3 groundOffset = Vector3.zero;

            // Update the targets with offsets
            laserMote.UpdateTargets(
                sourceTarget,
                destTarget,
                skyOffset,  // This adds the height to the source
                groundOffset
            );
        }

        private void AddVisualEffects()
        {
            // Growing phase effects
            if (currentTick <= growingPhaseTicks)
            {
                // Occasional smaller flashes along beam
                if (currentTick % 5 == 0)
                {
                    Vector3 effectPos = targetPosition + new Vector3(0, Rand.Range(10f, 50f), 0);
                    FleckMaker.ThrowLightningGlow(effectPos, Map, Rand.Range(0.5f, currentWidth * 0.4f));
                }
            }

            // Near impact effects
            if (currentTick >= impactTick - 10 && currentTick <= impactTick + 30)
            {
                // Energy build-up on ground
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

            GenExplosion.DoExplosion(
                Position,
                Map,
                explosionRadius,
                damageDef,
                this,
                damageAmount);

            // Visual effects for impact
            FleckMaker.Static(Position, Map, FleckDefOf.ExplosionFlash, 12f);

            // Akira-style expanding ring effect
            for (int i = 0; i < 18; i++)
            {
                float angle = (float)i / 18f * 360f;
                float distance = Rand.Range(3f, 8f);
                Vector3 ringPos = targetPosition + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );

                FleckMaker.ThrowHeatGlow(ringPos.ToIntVec3(), Map, Rand.Range(2f, 4f));
            }
        }

        private void CleanupAndDespawn()
        {
            // Clean up the mote
            if (laserMote != null && !laserMote.Destroyed)
            {
                laserMote.Destroy();
            }

            this.DeSpawn();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            // Make sure we clean up the mote when despawned
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
