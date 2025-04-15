using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MagicAndMyths
{
    public class CompProperties_PawnActions : CompProperties
    {
        public float throwMaxRange = 12f;
        public float throwMaxMass = 5f;
        public SoundDef throwSound;

        public CompProperties_PawnActions()
        {
            compClass = typeof(Comp_PawnActions);
        }
    }

    [StaticConstructorOnStartup]
    public class Comp_PawnActions : Comp_PawnActionBase
    {
        private static Texture2D defaultIcon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
        private Pawn Pawn => parent as Pawn;
        public CompProperties_PawnActions Props => (CompProperties_PawnActions)props;


        protected int LastThrowAttemptTick = -1;
        protected int ThrowCooldownTicks = 1250;


        protected bool IsThrowOffCooldown
        {
            get
            {
                if (LastThrowAttemptTick <= 0)
                {
                    return true;
                }

                return Current.Game.tickManager.TicksGame >= LastThrowAttemptTick + ThrowCooldownTicks;
            }
        }

        public override bool CanPerformAction(Pawn pawn)
        {
            return pawn.health.hediffSet.GetNotMissingParts().Any(limb => limb.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbCore)) && base.CanPerformAction(pawn);
        }
        private bool CanThrowThing(Thing thing)
        {
            if (thing == null || Pawn == null)
                return false;


            if (!CanPerformAction(Pawn))
            {
                return false;
            }

            if (thing is Plant)
            {
                return false;
            }

            if (thing is Mote)
            {
                return false;
            }

            return thing.Spawned && IsAbleToThrow(thing);
        }

        private bool IsAbleToThrow(Thing thing)
        {
            float pawnMaxThrowMass = ThrowUtility.CalculateMaxThrowMassForPawn(Pawn);
            float thingMass = thing.GetStatValue(StatDefOf.Mass);

            return pawnMaxThrowMass >= thingMass;
        }


        private void BeginThrowTarget()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetItems = true,
                canTargetSelf = false,
                canTargetAnimals = true,
                canTargetBuildings = true,
                canTargetHumans = true,
                canTargetPlants = false,
                canTargetLocations = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => CanThrowThing(x.Thing)
            },
            (LocalTargetInfo target) =>
            {
                if (target.Thing != null)
                {
                    StartPickUpAndThrow(target.Thing);
                    LastThrowAttemptTick = Current.Game.tickManager.TicksGame;
                }
            },
            null, null);
        }
 
        private List<Thing> GetThrowableEquipment()
        {
            List<Thing> thingsToThrow = new List<Thing>();

            if (Pawn?.inventory?.innerContainer != null)
            {
                thingsToThrow.AddRange(Pawn.inventory.innerContainer.Where(x=> IsAbleToThrow(x)).ToList());
            }

            if (Pawn?.equipment?.AllEquipmentListForReading != null)
            {
                thingsToThrow.AddRange(Pawn.equipment.AllEquipmentListForReading.Where(x=> IsAbleToThrow(x)).ToList());
            }
             
            return thingsToThrow;
        }

        private void BeginThrowFromInventory(Thing itemToThrow)
        {
            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetLocations = true,
                canTargetHumans = true,
                canTargetAnimals = true,
                canTargetCorpses = true,
                canTargetFires = true,
                canTargetItems = true,
                canTargetMechs = true,
                canTargetMutants = true,
                canTargetBloodfeeders = true,
                canTargetSelf = false,
                canTargetPlants = true,
                canTargetPawns = true,
                canTargetBuildings = true,
            },
            delegate (LocalTargetInfo target)
            {
                ExecuteThrowFromInventory(itemToThrow, target);
            });
        }
        private void ExecuteThrowFromInventory(Thing itemToThrow, LocalTargetInfo target)
        {
            if (Pawn.inventory.innerContainer.Contains(itemToThrow))
            {
                Pawn.inventory.innerContainer.Remove(itemToThrow);
            }
            else if(Pawn.equipment.AllEquipmentListForReading.Any(x=> x.thingIDNumber == itemToThrow.thingIDNumber))
            {
                Pawn.equipment.Remove((ThingWithComps)itemToThrow);
            }

            ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(
                MagicAndMythDefOf.MagicAndMyths_ThingFlyer,
                thing: itemToThrow,
                destCell: target.Cell,
                map: this.Pawn.Map,
                flightEffecterDef : null,
                landingSound : null,
                throwerPawn: Pawn,
                overrideStartVec: Pawn.DrawPos);

            GenSpawn.Spawn(thingFlyer, Pawn.Position, Pawn.Map);

            LastThrowAttemptTick = Current.Game.tickManager.TicksGame;
        }
        private void StartPickUpAndThrow(Thing thingToThrow)
        {
            if (thingToThrow == null || !thingToThrow.Spawned) 
                return;

            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetLocations = true,
                canTargetHumans = true,
                canTargetAnimals = true,
                canTargetCorpses = true,
                canTargetFires = true,
                canTargetItems = true, 
                canTargetMechs = true,
                canTargetMutants = true,
                canTargetBloodfeeders = true,
                canTargetSelf = false,
                canTargetPlants = true,
                canTargetPawns = true,
                canTargetBuildings = true
            },
            (LocalTargetInfo target) =>
            {
                Job job = JobMaker.MakeJob(MagicAndMythDefOf.MagicAndMyths_PickupAndThrow, thingToThrow, target);
                job.count = Math.Min(thingToThrow.def.stackLimit, thingToThrow.stackCount);
                Pawn.jobs.TryTakeOrderedJob(job);
            },
            null);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Pawn == null || !Pawn.Spawned || !Pawn.IsColonistPlayerControlled)
                yield break;

            yield return new Command_ActionWithCooldown
            {
                defaultLabel = "Throw Item",
                defaultDesc = "Throw an item from your inventory at a target location.",
                icon = defaultIcon,
                action = delegate
                {
                    List<Thing> throwableItems = GetThrowableEquipment();
                    if (throwableItems.Count == 0)
                    {
                        Messages.Message("No throwable items in inventory.", MessageTypeDefOf.RejectInput);
                        return;
                    }

                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Thing item in throwableItems)
                    {
                        options.Add(new FloatMenuOption(item.LabelCap, () => BeginThrowFromInventory(item),
                            item.def.uiIcon, item.def.uiIconColor));
                    }

                    Find.WindowStack.Add(new FloatMenu(options));
                },
                Disabled = !CanPerformAction(Pawn),
                disabledReason = !CanPerformAction(Pawn) ? "This pawn can't throw" : "",
                //cooldownPercentGetter = () => IsThrowOffCooldown
            };

            yield return new Command_Action
            {
                defaultLabel = "Throw Target",
                defaultDesc = "Pick up and throw an object from the environment.",
                icon = defaultIcon,
                action = BeginThrowTarget,
                Disabled = !CanPerformAction(Pawn),
                disabledReason = !CanPerformAction(Pawn) ? "This pawn can't throw" : ""
            };
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref LastThrowAttemptTick, "LastThrowAttemptTick", -1);
        }

    }
}