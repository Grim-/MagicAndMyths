using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CAWorker_FillSmallGaps : CellularAutomataWorker
    {
        public CAWorker_FillSmallGaps()
        {

        }

        public override void Apply(Map map, Dictionary<BspNode, DungeonRoom> nodeToRoomMap, BoolGrid dungeonGrid, BoolGrid currentState)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.x <= 2 || cell.z <= 2 || cell.x >= map.Size.x - 3 || cell.z >= map.Size.z - 3)
                {
                    continue;
                }

                if (currentState[cell]) // Floor cell
                {
                    bool isSmallGap = IsSmallFloorGap(cell, map, currentState);
                    if (isSmallGap && !IsPathCell(cell, map, currentState))
                    {
                        dungeonGrid[cell] = false;
                    }
                }
            }
        }

        private bool IsSmallFloorGap(IntVec3 cell, Map map, BoolGrid grid)
        {
            if (!grid[cell]) return false;

            int wallCount = 0;
            foreach (IntVec3 neighbor in GenAdjFast.AdjacentCells8Way(cell))
            {
                if (neighbor.InBounds(map) && !grid[neighbor])
                {
                    wallCount++;
                }
            }

            return wallCount >= 5;
        }
    }
}
