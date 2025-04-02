using Verse;

namespace MagicAndMyths
{
    public abstract class CellularAutomataWorker
    {
        public abstract void Apply(Map map, BoolGrid dungeonGrid, BoolGrid currentState);

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

        protected bool IsPathCell(IntVec3 cell, Map map, BoolGrid grid)
        {
            if (!grid[cell]) 
                return false;


            int cardinalFloorNeighbors = 0;
            foreach (IntVec3 dir in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                IntVec3 neighbor = cell + dir;
                if (neighbor.InBounds(map) && grid[neighbor])
                {
                    cardinalFloorNeighbors++;
                }
            }

            return cardinalFloorNeighbors <= 2;
        }
    }
}
