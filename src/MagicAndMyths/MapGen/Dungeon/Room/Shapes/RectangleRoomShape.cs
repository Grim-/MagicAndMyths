using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class RectangleRoomShape : RoomShapeBase
    {

        public override List<IntVec3> GenerateRoomCells(DungeonGenerationContext context, CellRect bounds, float sizeMultiplier)
        {
            List<IntVec3> cells = new List<IntVec3>();
            IntVec3 center = bounds.CenterCell;

            int scaledWidth = (int)(bounds.Width);
            int scaledHeight = (int)(bounds.Height);

            scaledWidth = Math.Max(9, bounds.Width);
            scaledHeight = Math.Max(9, bounds.Height);

            int minX = center.x - scaledWidth / 2;
            int maxX = center.x + scaledWidth / 2;
            int minZ = center.z - scaledHeight / 2;
            int maxZ = center.z + scaledHeight / 2;

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);
                    if (bounds.Contains(cell))
                    {
                        cells.Add(cell);
                    }
                }
            }


            //Log.Message($"Generating Rectangle \r\nRoom Bounds : {bounds.Width} x {bounds.Height}\r\n Actual {scaledWidth} x {scaledHeight}");
            return cells;
        }
    }
}