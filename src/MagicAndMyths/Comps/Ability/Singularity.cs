using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    [StaticConstructorOnStartup]
    public class Singularity : ThingWithComps, IThingHolder
    {
        private ThingOwner<Thing> innerContainer;
        private Dictionary<Thing, Vector3> relativePositions = new Dictionary<Thing, Vector3>();
        public float gravityRadius = 10f;
        public float pullStrength = 0.05f;


        private Effecter effecter = null;
        public Singularity()
        {
            this.innerContainer = new ThingOwner<Thing>(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad)
            {
                ReconnectRelativePositions();
            }

            if (effecter == null)
            {
                effecter = DefDatabase<EffecterDef>.GetNamed("MagicAndMyths_EffectSingularityAura").SpawnMaintained(this.Position, this.Map);
            }
        }

        private void ReconnectRelativePositions()
        {
            foreach (Thing thing in innerContainer)
            {
                if (!relativePositions.ContainsKey(thing))
                {
                    relativePositions[thing] = new Vector3(
                        Rand.Range(-0.5f, 0.5f),
                        0f,
                        Rand.Range(-0.5f, 0.5f));
                }
            }
        }

        public override void Tick()
        {
            if (Find.TickManager.TicksGame % 10 == 0)
            {
                ApplyGravitationalPull();
            }

            if (effecter != null)
            {
                effecter.EffectTick(new TargetInfo(this), new TargetInfo(this));
                effecter.ticksLeft = 100;
            }
        }

        private void ApplyGravitationalPull()
        {
            IEnumerable<Thing> thingsInRange = GenRadial.RadialDistinctThingsAround(Position, Map, gravityRadius, true)
                .Where(t => t != this);

            foreach (Thing thing in thingsInRange)
            {
                if (innerContainer.Contains(thing))
                    continue;

                Vector3 pullDirection = (DrawPos - thing.DrawPos).normalized;
                float distance = Vector3.Distance(DrawPos, thing.DrawPos);

                float currentPullStrength = pullStrength * (1f - (distance / gravityRadius));

                if (thing.Spawned)
                {
                    if (thing != this)
                    {
                        DamageInfo damage = thing.def.mineable ? new DamageInfo(DamageDefOf.Mining, 344 * 2, 1) : new DamageInfo(DamageDefOf.Blunt, 15, 1);
                        thing.TakeDamage(damage);
                    }


                    if (CanPullThing(thing))
                    {
                        if (thing is Pawn pawn)
                        {
                            TryPullPawn(pawn, currentPullStrength);
                        }
                        else if (thing.def.category == ThingCategory.Item && thing.def.EverHaulable)
                        {
                            TryPullItem(thing, pullDirection, currentPullStrength);
                        }

                    }

                }
            }
        }

        private void TryPullPawn(Pawn pawn, float pullStrength)
        {
            ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, pawn, this.Position, pawn.Map, null, null, null, pawn.DrawPos, false);
            thingFlyer.OnRespawn += ThingFlyer_OnRespawn;
            ThingFlyer.LaunchFlyer(thingFlyer, pawn, this.Position, pawn.Map);
        }

        private void TryPullItem(Thing thing, Vector3 pullDirection, float pullStrength)
        {
            ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, thing, this.Position, thing.Map, null, null, null, thing.DrawPos, false);
            thingFlyer.OnRespawn += ThingFlyer_OnRespawn;
            ThingFlyer.LaunchFlyer(thingFlyer, thing, this.Position, thing.Map);
        }

        private void ThingFlyer_OnRespawn(IntVec3 arg1, Thing arg2, Pawn arg3)
        {
            AbsorbThing(arg2);
        }

        private bool CanPullThing(Thing thing)
        {
            if (thing is Building && !(thing is Building_Door))
                return false;

            if (thing.def.category == ThingCategory.Item && thing.def.EverHaulable)
                return true;

            if (thing is Pawn pawn)
            {
                return !pawn.Dead;
            }

            return false;
        }

        private void AbsorbThing(Thing thing)
        {
            if (thing == null || !thing.Spawned)
                return;

            if (thing.Spawned)
                thing.DeSpawn();

            if (innerContainer.TryAdd(thing))
            {
                relativePositions[thing] = Rand.InsideUnitCircleVec3 * 2.2f;
            }
        }

        //protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        //{
        //    base.DrawAt(drawLoc, flip);


        //}

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);

            foreach (Thing thing in innerContainer)
            {
                if (relativePositions.TryGetValue(thing, out Vector3 relPos))
                {
                    Vector3 drawPos = DrawPos + relPos;

                    if (thing is Pawn pawn)
                    {
                        pawn.Drawer.renderer.DynamicDrawPhaseAt(phase, drawPos);
                    }
                    else
                    {
                        thing.DynamicDrawPhaseAt(phase, drawPos, flip);
                    }
                }
                else
                {
                    relativePositions[thing] = Vector3.zero;
                }
            }
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public void ReleaseThings()
        {
            List<Thing> thingsToRelease = new List<Thing>(innerContainer);
            foreach (Thing thing in thingsToRelease)
            {
                if (relativePositions.TryGetValue(thing, out Vector3 relPos))
                {
                    Vector3 releasePos = DrawPos + relPos;
                    IntVec3 releaseCell = IntVec3.FromVector3(releasePos);

                    if (!releaseCell.InBounds(Map))
                        releaseCell = Position;

                    if (innerContainer.TryDrop(thing, releaseCell, Map, ThingPlaceMode.Near, out Thing droppedThing))
                    {
                        relativePositions.Remove(thing);
                    }
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode != DestroyMode.Vanish)
            {
                ReleaseThings();
            }
            base.Destroy(mode);
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref this.innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref gravityRadius, "gravityRadius", 10f);
            Scribe_Values.Look(ref pullStrength, "pullStrength", 0.05f);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                List<Thing> things = relativePositions.Keys.ToList();
                List<Vector3> positions = relativePositions.Values.ToList();

                Scribe_Collections.Look(ref things, "thingRefs", LookMode.Reference);
                Scribe_Collections.Look(ref positions, "relativePositions", LookMode.Value);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<Thing> things = null;
                List<Vector3> positions = null;

                Scribe_Collections.Look(ref things, "thingRefs", LookMode.Reference);
                Scribe_Collections.Look(ref positions, "relativePositions", LookMode.Value);

                if (things != null && positions != null && things.Count == positions.Count)
                {
                    relativePositions = new Dictionary<Thing, Vector3>();
                    for (int i = 0; i < things.Count; i++)
                    {
                        if (things[i] != null)
                            relativePositions[things[i]] = positions[i];
                    }
                }
                else
                {
                    relativePositions = new Dictionary<Thing, Vector3>();
                }
            }
        }

        public override string GetInspectString()
        {
            return $"Singularity ({innerContainer.Count} things trapped)";
        }
    }

}
