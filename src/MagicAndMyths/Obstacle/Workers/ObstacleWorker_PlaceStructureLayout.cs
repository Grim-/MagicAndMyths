using Verse;

namespace MagicAndMyths
{
    public class ObstacleWorker_PlaceStructureLayout : ObstacleWorker
    {
        public override bool TryPlaceObstacles(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            StructureLayoutDef structureLayoutDef = MagicAndMythDefOf.TurretObstacleStructure;
            IntVec3 position = Room.Center;

            //too small to fit
            if (Room.roomCellRect.Width < structureLayoutDef.MaxBuildSize.x || Room.roomCellRect.Height < structureLayoutDef.MaxBuildSize.z)
            {
                return false;
            }

            // Get the CellRect needed for the structure
            CellRect neededRect = structureLayoutDef.GetCellRect(position);

            StructureBuilder.BuildStructure(structureLayoutDef, position, structureLayoutDef.LastStageIndex, map);

            return true;
        }
    }
}
