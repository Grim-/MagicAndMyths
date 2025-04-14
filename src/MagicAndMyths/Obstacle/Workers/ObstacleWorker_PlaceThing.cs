using Verse;

namespace MagicAndMyths
{
    public class ObstacleWorker_PlaceThing : ObstacleWorker
    {
        private ObstacleDef_PlaceThing Def => (ObstacleDef_PlaceThing)def;


        public override bool TryPlaceObstacles(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            if (Def == null || Def.thingToPlace == null)
            {
                return false;
            }

            int countOnMap = map.listerThings.AllThings.Count(x => x.def == Def.thingToPlace);

            if (Def.maxAllowed > 0 && countOnMap >= Def.maxAllowed)
            {
                return false;
            }

            Thing thing = ThingMaker.MakeThing(Def.thingToPlace, Def.thingStuff);
            
            if (GenPlace.TryPlaceThing(thing, Room.roomCellRect.CenterCell, map, ThingPlaceMode.Direct))
            {
                //thing = GenSpawn.Spawn(thing, Room.roomCellRect.CenterCell, map);
                return true;
            }

            return false;
        }
    }


    public class ObstacleDef_PlaceThing : ObstacleDef
    {
        public ThingDef thingToPlace;
        public ThingDef thingStuff;
        public int maxAllowed = 1;
    }
}
