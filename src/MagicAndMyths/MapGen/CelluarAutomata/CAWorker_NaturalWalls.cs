using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_NaturalWalls : CellularAutomataWorker
    {
        private int birthThreshold = 6;
        private int deathThreshold = 2;
        private float randomChance = 0.3f;


        public CAWorker_NaturalWalls()
        {

        }

        public override void Apply(Map map, Dungeon Dungeon, BoolGrid dungeonGrid, BoolGrid currentState)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.x <= 3 || cell.z <= 3 || cell.x >= map.Size.x - 4 || cell.z >= map.Size.z - 4)
                {
                    continue;
                }

                if (currentState[cell])
                {
                    if (!IsNearRoomEdge(cell, map, currentState))
                    {
                        continue;
                    }

                    if (IsPathCell(cell, map, currentState) || IsAdjacentToPath(cell, map, currentState))
                    {
                        continue;
                    }
                }

                int wallNeighbors = CountWallNeighbors(cell, map, currentState);

                if (currentState[cell])
                {
                    if (wallNeighbors >= birthThreshold && Rand.Chance(randomChance))
                    {
                        dungeonGrid[cell] = false;
                    }
                }
                else
                {
                    if (wallNeighbors <= deathThreshold && Rand.Chance(randomChance * 0.7f))
                    {
                        if (CountFloorNeighbors(cell, map, currentState) >= 1)
                        {
                            dungeonGrid[cell] = true;
                        }
                    }
                }
            }
        }

        private bool IsNearRoomEdge(IntVec3 cell, Map map, BoolGrid grid)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dz = -2; dz <= 2; dz++)
                {
                    IntVec3 checkCell = new IntVec3(cell.x + dx, cell.y, cell.z + dz);
                    if (checkCell.InBounds(map) && !grid[checkCell])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAdjacentToPath(IntVec3 cell, Map map, BoolGrid grid)
        {
            foreach (IntVec3 dir in GenAdjFast.AdjacentCells8Way(cell).ToArray())
            {
                if (dir.InBounds(map) && grid[dir] && IsPathCell(dir, map, grid))
                {
                    return true;
                }
            }
            return false;
        }

        private int CountFloorNeighbors(IntVec3 cell, Map map, BoolGrid grid)
        {
            int count = 0;
            foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell).ToArray())
            {
                if (neighbor.InBounds(map) && grid[neighbor])
                {
                    count++;
                }
            }
            return count;
        }
    }
}
