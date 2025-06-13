using System;
using Verse;

namespace MagicAndMyths
{
    public class DungeonQueryManager
    {
        public bool IsNearRoomEdge(IntVec3 cell, Map map, BoolGrid grid)
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

        public bool IsWallNearFloor(IntVec3 cell, Map map, BoolGrid grid)
        {
            foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell))
            {
                if (neighbor.InBounds(map) && grid[neighbor])
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsCornerWall(IntVec3 cell, Map map, BoolGrid grid)
        {
            if (grid[cell]) return false;

            int diagonalFloorCount = 0;
            int cardinalWallCount = 0;

            foreach (var dir in GenAdj.CardinalDirections)
            {
                IntVec3 neighbor = cell + dir;
                if (neighbor.InBounds(map) && !grid[neighbor])
                {
                    cardinalWallCount++;
                }
            }

            foreach (var dir in GenAdj.DiagonalDirections)
            {
                IntVec3 neighbor = cell + dir;
                if (neighbor.InBounds(map) && grid[neighbor])
                {
                    diagonalFloorCount++;
                }
            }

            return cardinalWallCount >= 2 && diagonalFloorCount >= 2;
        }

        public int CountCardinalWallNeighbors(IntVec3 cell, Map map, BoolGrid currentState)
        {
            int count = 0;

            foreach (IntVec3 neighbor in GenAdj.CardinalDirectionsAround)
            {
                IntVec3 neighborCell = cell + neighbor;
                if (neighborCell.InBounds(map))
                {
                    if (!currentState[neighborCell])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public IntVec3 FindNearestFloor(IntVec3 cell, Map map, BoolGrid grid)
        {
            for (int radius = 1; radius <= 3; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        if (Math.Abs(dx) + Math.Abs(dz) <= radius)
                        {
                            IntVec3 checkCell = new IntVec3(cell.x + dx, cell.y, cell.z + dz);
                            if (checkCell.InBounds(map) && grid[checkCell])
                            {
                                return checkCell;
                            }
                        }
                    }
                }
            }
            return IntVec3.Invalid;
        }

        public bool IsAdjacentToPath(IntVec3 cell, Map map, BoolGrid grid, Func<IntVec3, bool> isPathCell)
        {
            foreach (IntVec3 dir in GenAdjFast.AdjacentCells8Way(cell).ToArray())
            {
                if (dir.InBounds(map) && grid[dir] && isPathCell(dir))
                {
                    return true;
                }
            }
            return false;
        }

        public int CountFloorNeighbors(IntVec3 cell, Map map, BoolGrid grid)
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