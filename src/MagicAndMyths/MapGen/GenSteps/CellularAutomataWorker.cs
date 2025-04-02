using Verse;

namespace MagicAndMyths
{
    public abstract class CellularAutomataWorker
    {
        // Updated signature to include the current state
        public abstract void Apply(Map map, BoolGrid dungeonGrid, BoolGrid currentState);

        // Count wall neighbors (neighbors that are NOT in the grid = walls)
        protected int CountWallNeighbors(IntVec3 cell, Map map, BoolGrid grid)
        {
            int count = 0;
            foreach (IntVec3 neighbor in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (neighbor.InBounds(map) && !grid[neighbor])
                {
                    count++;
                }
            }
            return count;
        }

        // Check if a cell is likely a path/corridor
        protected bool IsPathCell(IntVec3 cell, Map map, BoolGrid grid)
        {
            if (!grid[cell]) return false; // Only floor cells can be paths

            int cardinalFloorNeighbors = 0;
            foreach (IntVec3 dir in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                IntVec3 neighbor = cell + dir;
                if (neighbor.InBounds(map) && grid[neighbor])
                {
                    cardinalFloorNeighbors++;
                }
            }

            // If this has 2 or fewer cardinal floor neighbors, it's likely a corridor
            return cardinalFloorNeighbors <= 2;
        }
    }
}
