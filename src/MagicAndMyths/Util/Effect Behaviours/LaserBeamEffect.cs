using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class LaserBeamEffect : Ticker
    {
        #region Fields
        private Map map;
        private Thing origin;
        private Thing target;
        private int lifeTimeTicks;

        private TrackedMote beamEffect;
        private static readonly ThingDef BeamMoteDef = DefDatabase<ThingDef>.GetNamed("Mote_GraserBeamBase");

        public event Action<LaserBeamEffect, Thing, Thing, Map> OnBeamCreated;
        public event Action<LaserBeamEffect, Thing, Thing, Map> OnBeamDestroyed;
        #endregion

        #region Initialization
        public LaserBeamEffect(Map map, Thing origin, Thing target, int lifeTimeTicks)
            : base(lifeTimeTicks, null, null, false, -1)
        {
            this.map = map;
            this.origin = origin;
            this.target = target;
            this.lifeTimeTicks = lifeTimeTicks;
        }

        public void StartBeam()
        {
            if (!ValidateBeam())
                return;

            CreateBeam();
            Start();
        }

        private bool ValidateBeam()
        {
            return origin != null &&
                   target != null &&
                   origin.Spawned &&
                   target.Spawned &&
                   origin.Map == target.Map;
        }

        private void CreateBeam()
        {
            if (BeamMoteDef == null)
            {
                Log.Error("LaserBeamEffect: Beam mote def not found");
                return;
            }

            MoteDualAttached mote = MoteMaker.MakeInteractionOverlay(
                BeamMoteDef,
                origin,
                new TargetInfo(target.Position, map, false));

            if (mote != null)
            {
                beamEffect = new TrackedMote(mote, origin, target, lifeTimeTicks);
                FleckMaker.ThrowLightningGlow(origin.DrawPos, map, 0.8f);
                OnBeamCreated?.Invoke(this, origin, target, map);
            }
        }

        #endregion

        #region Main Loop
        public override void Tick()
        {
            base.Tick();

            if (IsFinished)
                return;

            if (!ValidateBeam() || beamEffect == null || !ValidateEffect())
            {
                Stop();
                return;
            }

            UpdateBeam();
        }

        private bool ValidateEffect()
        {
            return beamEffect.RemainingTicks > 0 &&
                   beamEffect.Mote != null &&
                   !beamEffect.Mote.Destroyed;
        }

        private void UpdateBeam()
        {
            beamEffect.Mote.Maintain();
            beamEffect.Mote.UpdateTargets(
                beamEffect.SourceThing,
                beamEffect.TargetThing,
                Vector3.zero,
                Vector3.zero
            );
        }
        #endregion

        #region Cleanup
        public override void Stop(bool reset = false)
        {
            if (beamEffect != null && beamEffect.Mote != null && !beamEffect.Mote.Destroyed)
            {
                beamEffect.Mote.Destroy();
                FleckMaker.ThrowLightningGlow(target.DrawPos, map, 0.5f);
                OnBeamDestroyed?.Invoke(this, origin, target, map);
            }

            beamEffect = null;
            base.Stop(reset);
        }
        #endregion

        #region Save/Load
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref origin, "origin");
            Scribe_References.Look(ref target, "target");
        }
        #endregion
    }

}
