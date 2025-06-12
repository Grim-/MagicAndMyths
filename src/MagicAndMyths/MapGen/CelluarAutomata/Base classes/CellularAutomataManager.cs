using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public static class CellularAutomataManager
    {
        public static void ApplyRules(Map map, Dungeon Dungeon, List<CelluarAutomataData> workers, int iterations = 1)
        {
            BoolGrid dungeonGrid = Dungeon.GridManager.dungeonGrid;
            BoolGrid originalGrid = new BoolGrid(map);
            foreach (IntVec3 cell in map.AllCells)
            {
                originalGrid[cell] = dungeonGrid[cell];
            }

            foreach (var worker in workers)
            {
                BoolGrid currentState = new BoolGrid(map);
                foreach (IntVec3 cell in map.AllCells)
                {
                    currentState[cell] = dungeonGrid[cell];
                }

                for (int x = 0; x < worker.iterations; x++)
                {
                    worker.automataDef.Apply(map, Dungeon, dungeonGrid, currentState);
                }

            }


            ////just return the boolgrid let generator do this
            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell])
                {
                    Thing wall = cell.GetFirstBuilding(map);
                    if (wall != null && wall.def == Dungeon.Def.WallDef)
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
