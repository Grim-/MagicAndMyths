using LudeonTK;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public static class MagicUtil
    {

        [DebugAction("Magic And Myths", "Test Spawn Orbital Laser", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnOrbitalLaser()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    OrbitalLaser meteor = (OrbitalLaser)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("MagicAndMyths_OrbitalLaser"));
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Fire(target.Cell);
                }
            }
            );


        }
        [DebugAction("Magic And Myths", "Test Transmute Lightning", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FireTransmutationLightning()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    LightningStrike.GenerateLightningStrike(Find.CurrentMap, target.Cell, 5, out IEnumerable<IntVec3> affectedCells);
                    TerrainDef goldTile = DefDatabase<TerrainDef>.AllDefs.RandomElement();


                    List<ThingDef> naturalRockDefs = DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.building.isNaturalRock).ToList();

                    foreach (var item in affectedCells)
                    {
                        Find.CurrentMap.terrainGrid.SetTerrain(item, goldTile);



                        foreach (var thing in item.GetThingList(Find.CurrentMap))
                        {
                            if (thing.def.building != null && thing.def.building.isNaturalRock)
                            {
                                IntVec3 position = thing.Position;

                                thing.Destroy();

                                Thing replacementRock = ThingMaker.MakeThing(naturalRockDefs.RandomElement());
                                GenSpawn.Spawn(replacementRock, position, Find.CurrentMap);


                            }

                        }
                    }

                }
            }
            );


        }
        [DebugAction("Magic And Myth", "Test Spawn Meteor", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnMeteor()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters()
            {
                canTargetLocations = true
            },
            (LocalTargetInfo target) =>
            {
                if (target.Cell.IsValid && target.Cell.InBounds(Find.CurrentMap))
                {
                    Meteor meteor = (Meteor)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("MagicAndMyths_Meteor"));
                    GenSpawn.Spawn(meteor, target.Cell, Find.CurrentMap);

                    meteor.Launch(target.Cell);
                }
            }
            );


        }
    }
}
