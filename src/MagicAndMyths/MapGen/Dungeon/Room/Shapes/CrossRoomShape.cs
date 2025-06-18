using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CrossRoomShape : RoomShapeBase
    {
        public int armWidth = 2;

        public override List<IntVec3> GenerateRoomCells(DungeonGenerationContext context, CellRect bounds, float sizeMultiplier)
        {
            List<IntVec3> cells = new List<IntVec3>();

            IntVec3 center = bounds.CenterCell;

            int hArmLength = (int)(bounds.Width / 2 * sizeMultiplier);
            for (int x = center.x - hArmLength; x <= center.x + hArmLength; x++)
            {
                for (int z = center.z - armWidth / 2; z <= center.z + armWidth / 2; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);
                    if (bounds.Contains(cell))
                        cells.Add(cell);
                }
            }

            int vArmLength = (int)(bounds.Height / 2 * sizeMultiplier);
            for (int z = center.z - vArmLength; z <= center.z + vArmLength; z++)
            {
                for (int x = center.x - armWidth / 2; x <= center.x + armWidth / 2; x++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);
                    if (bounds.Contains(cell) && !cells.Contains(cell))
                        cells.Add(cell);
                }
            }

            return cells;
        }
    }
}
