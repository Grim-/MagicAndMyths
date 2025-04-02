using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public static class CellularAutomataManager
    {
        public static void ApplyRules(Map map, BoolGrid dungeonGrid, List<CellularAutomataWorker> workers, int iterations = 3)
        {
            // Store the truly original grid once, for path integrity checking at the end
            BoolGrid originalGrid = new BoolGrid(map);
            foreach (IntVec3 cell in map.AllCells)
            {
                originalGrid[cell] = dungeonGrid[cell];
            }

            // Run the specified number of iterations
            for (int i = 0; i < iterations; i++)
            {
                // In each iteration, apply each worker
                foreach (var worker in workers)
                {
                    // Each worker needs a snapshot of the current state to read from
                    BoolGrid currentState = new BoolGrid(map);
                    foreach (IntVec3 cell in map.AllCells)
                    {
                        currentState[cell] = dungeonGrid[cell];
                    }

                    // The worker reads from currentState and writes to dungeonGrid
                    worker.Apply(map, dungeonGrid, currentState);
                }
            }

            // After all iterations, ensure critical paths remain intact
            EnsurePathsIntact(map, dungeonGrid, originalGrid);

            // Apply the final grid to the map
            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell])
                {
                    // This is a floor cell - ensure any walls are removed
                    Thing wall = cell.GetFirstBuilding(map);
                    if (wall != null && wall.def == MagicAndMythDefOf.DungeonWall)
                    {
                        wall.Destroy();
                    }
                }
                else
                {
                    // This is a wall cell - ensure a wall exists
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
