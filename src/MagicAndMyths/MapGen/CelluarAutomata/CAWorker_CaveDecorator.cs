using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_CaveDecorator : CellularAutomataWorker
    {
        private float randomChance = 0.2f;

        public CAWorker_CaveDecorator()
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

                if (!currentState[cell] && IsWallNearFloor(cell, map, currentState))
                {
                    if (Rand.Chance(randomChance))
                    {
                        IntVec3 nearestFloor = FindNearestFloor(cell, map, currentState);
                        if (nearestFloor.IsValid)
                        {
                            IntVec3 direction = new IntVec3(
                                Math.Sign(nearestFloor.x - cell.x),
                                0,
                                Math.Sign(nearestFloor.z - cell.z));

                            IntVec3 nextCell = cell + direction;
                            if (nextCell.InBounds(map) && !currentState[nextCell])
                            {
                                dungeonGrid[nextCell] = true;
                            }
                        }
                    }
                }
            }
        }

        private bool IsWallNearFloor(IntVec3 cell, Map map, BoolGrid grid)
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

        private IntVec3 FindNearestFloor(IntVec3 cell, Map map, BoolGrid grid)
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
    }
}
