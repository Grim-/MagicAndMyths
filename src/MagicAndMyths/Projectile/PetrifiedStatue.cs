using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class PetrifiedStatue : ThingWithComps, IThingHolder
	{
		private ThingOwner<Thing> innerContainer;
		private Faction restoreFaction = null;

		public event Action<IntVec3, Thing> OnUnpetrify;

		protected Pawn PetrifiedThing
		{
			get
			{
				if (this.innerContainer.InnerListForReading.Count <= 0)
				{
					return null;
				}
				return (Pawn)this.innerContainer.InnerListForReading[0];
			}
		}

        public override string GetInspectString()
        {
			return PetrifiedThing != null ? $"Eerie Statue of a {PetrifiedThing.def.LabelCap}." : base.GetInspectString();
        }

        public PetrifiedStatue()
		{
			this.innerContainer = new ThingOwner<Thing>(this);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
            {
				if (PetrifiedThing == null || !(PetrifiedThing is Pawn))
				{
					GenerateRandomPetrifiedPawn();
				}
			}
		}

		private void GenerateRandomPetrifiedPawn()
		{
			PawnKindDef kindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.RandomElement();

			if (DefDatabase<PawnKindDef>.AllDefsListForReading.Any(k => k.RaceProps.Humanlike))
			{
				kindDef = DefDatabase<PawnKindDef>.AllDefsListForReading
					.Where(k => k.RaceProps.Humanlike)
					.RandomElement();
			}


			Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
				kind: kindDef,
				faction: null,
				context: PawnGenerationContext.NonPlayer));

			this.innerContainer.TryAdd(pawn, true);

			if (pawn.RaceProps.Humanlike)
			{
				pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
			}

			if (pawn.RaceProps.Humanlike)
			{
				PawnApparelGenerator.GenerateStartingApparelFor(pawn, new PawnGenerationRequest(
					kindDef,
					null,
					PawnGenerationContext.NonPlayer));
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

		public override void Tick()
		{
			base.Tick();
			//this.innerContainer.ThingOwnerTick(true);
		}

		public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
		{
            Thing petrifiedThing = this.PetrifiedThing;
            if (petrifiedThing != null)
            {
                Rot4 originalRotation = petrifiedThing.Rotation;
                petrifiedThing.Rotation = Rot4.South;
                petrifiedThing.DynamicDrawPhaseAt(phase, drawLoc, false);
                petrifiedThing.Rotation = originalRotation;
			}
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.PetrifiedThing != null)
            {
				Pawn pawn = UnpetrifyThing(false);
				DealUnpetrificationDamage(pawn);
			}
            base.Destroy(mode);
        }


		public void DealUnpetrificationDamage(Pawn pawn)
        {

			if (Rand.Bool)
			{
				HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
			}
			else
			{
				HealthUtility.DamageLimbsUntilIncapableOfManipulation(pawn, false);
			}
		}

        public static PetrifiedStatue PetrifyPawn(ThingDef statueDef, Pawn pawn, IntVec3 position, Map map)
		{
            if (pawn.Spawned)
			{
				pawn.DeSpawn();			
			}

			pawn.story.skinColorOverride = Color.grey;

			PetrifiedStatue statue = (PetrifiedStatue)ThingMaker.MakeThing(statueDef, null);
			statue.restoreFaction = pawn.Faction;
			pawn.SetFaction(null);
			if (!statue.innerContainer.TryAdd(pawn, true))
			{
				Log.Error("Failed to add pawn to petrified statue container: " + pawn);
				return null;
			}

			GenSpawn.Spawn(statue, position, map);
			return statue;
		}

		public Pawn UnpetrifyThing(bool destroy = true)
		{
			Thing petrifiedThing = this.PetrifiedThing;
			if (petrifiedThing == null)
			{
				return null;
			}

			if (petrifiedThing is Pawn petrifiedPawn2)
			{
				petrifiedPawn2.story.skinColorOverride = null;
			}

			if (restoreFaction != null)
			{
				petrifiedThing.SetFaction(restoreFaction);
			}

			Map map = this.Map;
			if (map == null)
			{
				return null;
			}

			IntVec3 position = this.Position;
			OnUnpetrify?.Invoke(position, petrifiedThing);


            if (this.innerContainer.TryDrop(petrifiedThing, position, map, ThingPlaceMode.Near, out Thing thing, null, null, false))
            {
                if (destroy)
                {
					this.Destroy(DestroyMode.Vanish);
				}

				return thing as Pawn;
			}

			return null;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
			{
				this
			});

			Scribe_References.Look(ref restoreFaction, "restoreFaction");
		}
	}
}
