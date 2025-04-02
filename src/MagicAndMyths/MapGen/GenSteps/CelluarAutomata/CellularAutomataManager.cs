using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public static class CellularAutomataManager
    {
        public static void ApplyRules(Map map, BoolGrid dungeonGrid, List<CelluarAutomataDef> workers, int iterations = 3)
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
                    worker.Apply(map, dungeonGrid, currentState);
                }
            }

            //EnsurePathsIntact(map, dungeonGrid, originalGrid);

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

        private static void EnsurePathsIntact(Map map, BoolGrid currentGrid, BoolGrid originalGrid)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                // If this was a floor in the original and is now a wall
                if (originalGrid[cell] && !currentGrid[cell])
                {
                    int cardinalFloorNeighbors = 0;
                    foreach (IntVec3 dir in GenAdjFast.AdjacentCellsCardinal(cell))
                    {
                        IntVec3 neighbor = cell + dir;
                        if (neighbor.InBounds(map) && originalGrid[neighbor])
                        {
                            cardinalFloorNeighbors++;
                        }
                    }

                    // If this had exactly 2 cardinal floor neighbors in the original, it was likely a corridor
                    if (cardinalFloorNeighbors == 2)
                    {
                        currentGrid[cell] = true;
                    }
                }
            }
        }
    }
}
