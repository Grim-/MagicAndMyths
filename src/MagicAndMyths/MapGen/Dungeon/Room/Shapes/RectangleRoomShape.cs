using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class RectangleRoomShape : RoomShapeBase
    {
        public float randomVariation = 0.2f;
        public int seed = 54321;

        public override List<IntVec3> GenerateRoomCells(DungeonGenerationContext context, CellRect bounds, float sizeMultiplier)
        {
            List<IntVec3> cells = new List<IntVec3>();
            IntVec3 center = bounds.CenterCell;

            Random rand = new Random(seed + center.x + center.z);

            float widthVariation = 1.0f + (float)(rand.NextDouble() * 2.0 - 1.0) * randomVariation;
            float heightVariation = 1.0f + (float)(rand.NextDouble() * 2.0 - 1.0) * randomVariation;

            int scaledWidth = (int)(bounds.Width * sizeMultiplier * widthVariation);
            int scaledHeight = (int)(bounds.Height * sizeMultiplier * heightVariation);

            scaledWidth = Math.Max(1, Math.Min(scaledWidth, bounds.Width));
            scaledHeight = Math.Max(1, Math.Min(scaledHeight, bounds.Height));

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

            return cells;
        }
    }
}