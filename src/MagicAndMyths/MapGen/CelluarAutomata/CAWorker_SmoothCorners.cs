using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_SmoothCorners : CellularAutomataWorker
    {
        private float randomChance = 0.7f;

        public CAWorker_SmoothCorners()
        {
        }

        public override void Apply(Map map, Dungeon Dungeon, BoolGrid dungeonGrid, BoolGrid currentState)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.x <= 2 || cell.z <= 2 || cell.x >= map.Size.x - 3 || cell.z >= map.Size.z - 3)
                {
                    continue;
                }

                if (!currentState[cell]) // Wall cell
                {
                    if (IsCornerWall(cell, map, currentState) && Rand.Chance(randomChance))
                    {
                        dungeonGrid[cell] = true;
                    }
                }
            }
        }

        private bool IsCornerWall(IntVec3 cell, Map map, BoolGrid grid)
        {
            if (grid[cell]) return false;

            int diagonalFloorCount = 0;
            int cardinalWallCount = 0;

            IntVec3[] cardinals = {
                new IntVec3(0, 0, 1),  // North
                new IntVec3(1, 0, 0),  // East
                new IntVec3(0, 0, -1), // South
                new IntVec3(-1, 0, 0)  // West
            };

            IntVec3[] diagonals = {
                new IntVec3(1, 0, 1),   // Northeast
                new IntVec3(1, 0, -1),  // Southeast
                new IntVec3(-1, 0, -1), // Southwest
                new IntVec3(-1, 0, 1)   // Northwest
            };

            foreach (var dir in cardinals)
            {
                IntVec3 neighbor = cell + dir;
                if (neighbor.InBounds(map) && !grid[neighbor])
                {
                    cardinalWallCount++;
                }
            }

            foreach (var dir in diagonals)
            {
                IntVec3 neighbor = cell + dir;
                if (neighbor.InBounds(map) && grid[neighbor])
                {
                    diagonalFloorCount++;
                }
            }

            return cardinalWallCount >= 2 && diagonalFloorCount >= 2;
        }
    }
}
