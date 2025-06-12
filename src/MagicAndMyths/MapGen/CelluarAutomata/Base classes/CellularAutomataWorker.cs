using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public abstract class CellularAutomataWorker
    {
        public abstract void Apply(Map map, Dungeon Dungeon, BoolGrid dungeonGrid, BoolGrid currentState);


        protected bool CanAffectCell(Map map, Dungeon dungeon, IntVec3 cell)
        {
            if (cell.x <= 3 || cell.z <= 3 || cell.x >= map.Size.x - 4 || cell.z >= map.Size.z - 4)
            {
                return false;
            }

            if (dungeon.GridManager.ProtectionGrid[cell])
            {
                return false;
            }

            return true;
        }

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
