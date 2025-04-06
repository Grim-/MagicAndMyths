using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public static class CellularAutomataManager
    {
        public static void ApplyRules(Map map, Dictionary<BspNode, DungeonRoom> nodeToRoomMap, BoolGrid dungeonGrid, List<CelluarAutomataDef> workers, int iterations = 3)
        {
            BoolGrid originalGrid = new BoolGrid(map);
            foreach (IntVec3 cell in map.AllCells)
            {
                originalGrid[cell] = dungeonGrid[cell];
            }

            for (int i = 0; i < iterations; i++)
            {
                foreach (var worker in workers)
                {
                    BoolGrid currentState = new BoolGrid(map);
                    foreach (IntVec3 cell in map.AllCells)
                    {
                        currentState[cell] = dungeonGrid[cell];
                    }
                    worker.Apply(map,nodeToRoomMap, dungeonGrid, currentState);
                }
            }


            //just return the boolgrid let generator do this
            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell])
                {
                    Thing wall = cell.GetFirstBuilding(map);
                    if (wall != null && wall.def == MagicAndMythDefOf.DungeonWall)
                    {
                        wall.Destroy();
                    }
                }
                else
                {
                    if (cell.GetFirstBuilding(map) == null)
                    {
                        GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, cell, map);
                    }
                }
            }
        }
    }
}
