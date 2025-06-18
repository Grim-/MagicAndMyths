using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CircularRoomShape : RoomShapeBase
    {
        public override List<IntVec3> GenerateRoomCells(DungeonGenerationContext context, CellRect bounds, float sizeMultiplier)
        {
            IntVec3 center = bounds.CenterCell;
            int radius = (int)((bounds.Width / 2 - 1) * Random.Range(0.6f, 1f));

            var allCells = GenRadial.RadialCellsAround(center, radius, true).ToList();

            // Remove the outermost cells in cardinal directions
            var filteredCells = allCells.Where(cell => !IsOutermostCardinalCell(cell, center, allCells)).ToList();

            return filteredCells;
        }

        private bool IsOutermostCardinalCell(IntVec3 cell, IntVec3 center, List<IntVec3> allCells)
        {
            // Check if this cell is at the extreme north, south, east, or west position
            bool isNorthernmost = cell.z == allCells.Max(c => c.z) && cell.x == center.x;
            bool isSouthernmost = cell.z == allCells.Min(c => c.z) && cell.x == center.x;
            bool isEasternmost = cell.x == allCells.Max(c => c.x) && cell.z == center.z;
            bool isWesternmost = cell.x == allCells.Min(c => c.x) && cell.z == center.z;

            return isNorthernmost || isSouthernmost || isEasternmost || isWesternmost;
        }
    }
}
