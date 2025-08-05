using EMF;
using Verse;

namespace MagicAndMyths
{
    public class ObstacleDef_PlaceStructureLayout : ObstacleDef
    {
        public StructureLayoutDef structureToPlace;

        public ObstacleDef_PlaceStructureLayout()
        {
            this.workerClass = typeof(ObstacleWorker_PlaceStructureLayout);
        }
    }

    public class ObstacleWorker_PlaceStructureLayout : ObstacleWorker
    {
        ObstacleDef_PlaceStructureLayout Def => (ObstacleDef_PlaceStructureLayout)def;

        public override bool TryPlaceObstacles(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            if (Def.structureToPlace == null)
            {
                return false;
            }

            StructureLayoutDef structureLayoutDef = Def.structureToPlace;
            IntVec3 position = Room.Center;
            CellRect neededRect = structureLayoutDef.GetCellRect(position);

            if (neededRect.FullyContainedWithin(Room.RoomCellRect))
            {
                StructureBuilder.BuildStructure(structureLayoutDef, position, structureLayoutDef.LastStageIndex, map);
                return true;
            }

            return false;
        }
    }
}
