using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    // Manages grid-based operations
    public class DungeonGridManager : IExposable
    {
        public BoolGrid dungeonGrid;
        public BoolGrid ProtectionGrid;
        public Map map;

        public DungeonGridManager()
        {
        }

        public DungeonGridManager(Map map)
        {
            this.map = map;
            dungeonGrid = new BoolGrid(map);
            ProtectionGrid = new BoolGrid(map);
        }

        public void MarkCellAsFloor(IntVec3 cell)
        {
            if (cell.InBounds(map))
            {
                dungeonGrid[cell] = true;
            }
        }

        public void MarkCellAsWall(IntVec3 cell)
        {
            if (cell.InBounds(map))
            {
                dungeonGrid[cell] = false;
            }
        }

        public bool IsCellFloor(IntVec3 cell)
        {
            return cell.InBounds(map) && dungeonGrid[cell];
        }

        public void MarkCellProtected(IntVec3 cell, bool isProtected)
        {
            if (cell.InBounds(map))
            {
                ProtectionGrid[cell] = isProtected;
            }
        }

        public void MarkCellsProtected(IEnumerable<IntVec3> cells, bool isProtected)
        {
            foreach (var cell in cells)
            {
                if (cell.InBounds(map))
                {
                    ProtectionGrid[cell] = isProtected;
                }
            }
        }

        public bool IsCellProtected(IntVec3 cell)
        {
            return cell.InBounds(map) && ProtectionGrid[cell];
        }

        public bool IsIsolatedWall(IntVec3 cell)
        {
            foreach (IntVec3 adj in GenAdjFast.AdjacentCells8Way(cell))
            {
                if (!adj.InBounds(map) || !IsCellFloor(adj))
                {
                    return false;
                }
            }
            return true;
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref dungeonGrid, "dungeonGrid");
            Scribe_Deep.Look(ref ProtectionGrid, "protectionGrid");
        }
    }
}