using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace MagicAndMyths
{
    public class ThingFlyer : Thing, IThingHolder
	{
		private ThingOwner<Thing> innerContainer;
		protected Vector3 startVec;
		private IntVec3 destCell;
		private float flightDistance;
		protected int ticksFlightTime = 120;
		protected int ticksFlying;
		protected EffecterDef flightEffecterDef;
		protected SoundDef soundLanding;
		private LocalTargetInfo target;
		private Effecter flightEffecter;
		private int positionLastComputedTick = -1;
		private Vector3 groundPos;
		private Vector3 effectivePos;
		private float effectiveHeight;

		protected Pawn throwingPawn = null;
		public event Action<IntVec3, Pawn> OnLanded;
		public event Action<IntVec3, Thing, Pawn> OnImpactedThing;
		protected Thing FlyingThing
		{
			get
			{
				if (this.innerContainer.InnerListForReading.Count <= 0)
				{
					return null;
				}
				return this.innerContainer.InnerListForReading[0];
			}
		}

		public Vector3 DestinationPos
		{
			get
			{
				Thing flyingThing = this.FlyingThing;
				if (flyingThing == null)
				{
					return this.destCell.ToVector3Shifted();
				}
				return GenThing.TrueCenter(this.destCell, flyingThing.Rotation, flyingThing.def.size, flyingThing.def.Altitude);
			}
		}
		public override Vector3 DrawPos
		{
			get
			{
				this.RecomputePosition();
				return this.effectivePos;
			}
		}

		public ThingFlyer()
		{
			this.innerContainer = new ThingOwner<Thing>(this);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return this.innerContainer;
		}

		private void RecomputePosition()
		{
			if (this.positionLastComputedTick == this.ticksFlying)
			{
				return;
			}
			this.positionLastComputedTick = this.ticksFlying;
			float t = (float)this.ticksFlying / (float)this.ticksFlightTime;
			float t2 = this.def.pawnFlyer.Worker.AdjustedProgress(t);
			this.effectiveHeight = this.def.pawnFlyer.Worker.GetHeight(t2);
			this.groundPos = Vector3.Lerp(this.startVec, this.DestinationPos, t2);
			Vector3 b = Altitudes.AltIncVect * this.effectiveHeight;
			Vector3 b2 = Vector3.forward * (this.def.pawnFlyer.heightFactor * this.effectiveHeight);
			this.effectivePos = this.groundPos + b + b2;
			base.Position = this.groundPos.ToIntVec3();
		}

		public void SetThrowingPawn(Pawn pawn)
        {
			this.throwingPawn = pawn;
        }

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				float num = Mathf.Max(this.flightDistance, 1f) / this.def.pawnFlyer.flightSpeed;
				num = Mathf.Max(num, this.def.pawnFlyer.flightDurationMin);
				this.ticksFlightTime = num.SecondsToTicks();
				this.ticksFlying = 0;
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			Effecter effecter = this.flightEffecter;
			if (effecter != null)
			{
				effecter.Cleanup();
			}
			base.Destroy(mode);
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
		}

		protected virtual void RespawnThing()
		{
			Thing flyingThing = this.FlyingThing;
			if (flyingThing == null)
			{
				return;
			}

			Map map = this.Map;
			if (map == null)
			{
				return;
			}

			this.innerContainer.TryDrop(flyingThing, this.destCell, map, ThingPlaceMode.Direct, out Thing thing, null, null, false);

			this.Position = this.destCell;

			if (this.throwingPawn != null)
			{
				OnLanded?.Invoke(this.Position, this.throwingPawn);
			}

			List<Thing> thingList = map.thingGrid.ThingsListAtFast(this.Position);
			if (thingList != null)
			{
				foreach (Thing th in thingList)
				{
					if (th != null && th != flyingThing && th != this)
					{
						if (this.throwingPawn != null)
						{
							OnImpactedThing?.Invoke(this.Position, th, this.throwingPawn);
						}
						break;
					}
				}
			}
		}


		public override void Tick()
		{
			if (this.flightEffecter == null && this.flightEffecterDef != null)
			{
				this.flightEffecter = this.flightEffecterDef.Spawn();
				this.flightEffecter.Trigger(this, TargetInfo.Invalid, -1);
			}
			else
			{
				Effecter effecter = this.flightEffecter;
				if (effecter != null)
				{
					effecter.EffectTick(this, TargetInfo.Invalid);
				}
			}
			if (this.ticksFlying >= this.ticksFlightTime)
			{
				this.RespawnThing();
				this.Destroy(DestroyMode.Vanish);
			}
			else
			{
				if (this.ticksFlying % 5 == 0)
				{
					this.CheckDestination();
				}
				this.innerContainer.ThingOwnerTick(true);
			}
			this.ticksFlying++;
		}

		private void CheckDestination()
		{
			if (!JumpUtility.ValidJumpTarget(base.Map, this.destCell))
			{
				int num = GenRadial.NumCellsInRadius(3.9f);
				for (int i = 0; i < num; i++)
				{
					IntVec3 cell = this.destCell + GenRadial.RadialPattern[i];
					if (JumpUtility.ValidJumpTarget(base.Map, cell))
					{
						this.destCell = cell;
						return;
					}
				}
			}
		}

		public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
		{
			this.RecomputePosition();
			Thing flyingThing = this.FlyingThing;
			if (flyingThing != null)
			{
				flyingThing.DynamicDrawPhaseAt(phase, this.effectivePos, false);
			}
			base.DynamicDrawPhaseAt(phase, drawLoc, flip);
		}
		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			this.DrawShadow(this.groundPos, this.effectiveHeight);
		}
		private void DrawShadow(Vector3 drawLoc, float height)
		{
			Material shadowMaterial = this.def.pawnFlyer.ShadowMaterial;
			if (shadowMaterial == null)
			{
				return;
			}
			float num = Mathf.Lerp(1f, 0.6f, height);
			Vector3 s = new Vector3(num, 1f, num);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(drawLoc, Quaternion.identity, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
		}

		public static ThingFlyer MakeFlyer(ThingDef flyingDef, Thing thing, IntVec3 destCell, Map map, EffecterDef flightEffecterDef, SoundDef landingSound, Pawn throwerPawn, Vector3? overrideStartVec = null, LocalTargetInfo target = default(LocalTargetInfo))
		{
			ThingFlyer thingFlyer = (ThingFlyer)ThingMaker.MakeThing(flyingDef, null);
			thingFlyer.startVec = (overrideStartVec ?? thing.TrueCenter());
			thingFlyer.Rotation = thing.Rotation;
			thingFlyer.flightDistance = thing.Position.DistanceTo(destCell);
			thingFlyer.destCell = destCell;
			thingFlyer.flightEffecterDef = flightEffecterDef;
			thingFlyer.soundLanding = landingSound;
			thingFlyer.target = target;
			thingFlyer.SetThrowingPawn(throwerPawn);

			if (thing.Spawned)
			{
				thing.DeSpawn(DestroyMode.WillReplace);
			}

			if (!thingFlyer.innerContainer.TryAdd(thing, true))
			{
				Log.Error("Could not add " + thing.ToStringSafe<Thing>() + " to a flyer.");
				thing.Destroy(DestroyMode.Vanish);
			}

            Comp_Throwable throwableComp = thing.TryGetComp<Comp_Throwable>();
            if (throwableComp != null)
            {
                throwableComp.OnThrown(throwerPawn.Position, map, throwerPawn);
            }

            thingFlyer.OnLanded += (IntVec3 position, Pawn throwingPawn) =>
            {
                if (throwableComp != null)
                {
                    throwableComp.OnLanded(position, map, throwerPawn);
                }
                else
                {
                    ThrowUtility.ApplyDefaultThrowBehavior(throwerPawn, thing, position, map);
                }
            };

            thingFlyer.OnImpactedThing += (IntVec3 position, Thing impactedThing, Pawn throwingPawn) =>
            {
                if (throwableComp != null)
                {
                    throwableComp.OnImpactedThing(position, map, throwingPawn, impactedThing);
                }
                else
                {
                    ThrowUtility.ApplyDefaultThrowImpactThingBehavior(throwingPawn, thing, position, map, impactedThing);
                }
            };

            if (throwerPawn != null)
            {
				thingFlyer.SetThrowingPawn(throwerPawn);
            }

            return thingFlyer;
		}
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Vector3>(ref this.startVec, "startVec", default(Vector3), false);
			Scribe_Values.Look<IntVec3>(ref this.destCell, "destCell", default(IntVec3), false);
			Scribe_Values.Look<float>(ref this.flightDistance, "flightDistance", 0f, false);
			Scribe_Values.Look<int>(ref this.ticksFlightTime, "ticksFlightTime", 0, false);
			Scribe_Values.Look<int>(ref this.ticksFlying, "ticksFlying", 0, false);
			Scribe_References.Look(ref throwingPawn, "throwingPawn");
			Scribe_Defs.Look<EffecterDef>(ref this.flightEffecterDef, "flightEffecterDef");
			Scribe_Defs.Look<SoundDef>(ref this.soundLanding, "soundLanding");
			Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});
			Scribe_TargetInfo.Look(ref this.target, "target");
		}
	}
}
