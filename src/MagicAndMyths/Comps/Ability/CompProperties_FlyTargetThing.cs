using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class CompProperties_FlyTargetThing : CompProperties_AbilityEffect
    {
        public CompProperties_FlyTargetThing()
        {
            compClass = typeof(CompAbilityEffect_FlyTargetThing);
        }
    }

    public class CompAbilityEffect_FlyTargetThing : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.pawn?.Map == null)
                return;
            Map map = parent.pawn.Map;


            if (target.Thing == null)
            {
                return;
            }


            IntVec3 spawnPosition = target.Thing.Position;


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
                canTargetPlants = false,
                canTargetPawns = true,
                canTargetBuildings = true
            }, (LocalTargetInfo targetLocation) =>
            {

                if (targetLocation.Cell.IsValid)
                {
                    ThingFlyer thingFlyer = ThingFlyer.MakeFlyer(MagicAndMythDefOf.MagicAndMyths_ThingFlyer, target.Thing, targetLocation.Cell, map, null, null, this.parent.pawn);
                    ThingFlyer.LaunchFlyer(thingFlyer, target.Thing, spawnPosition, targetLocation.Cell, map);
                }
            });
        }
    }
}
