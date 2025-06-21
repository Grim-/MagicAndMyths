using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class StraightCorridorPath : CorridorPathBase
    {
        public override List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map)
        {
            int width = 3; // Default width, should come from context
            return GenerateWideCorridor(start, end, map, width);
        }

        private List<IntVec3> GenerateWideCorridor(IntVec3 start, IntVec3 end, Map map, int width)
        {
            var cells = new HashSet<IntVec3>();

            // Get direction vector and normalize to unit steps
            IntVec3 direction = end - start;
            float distance = direction.LengthHorizontal;

            if (distance < 1f)
            {
                return new List<IntVec3> { start };
            }

            // Calculate step vector for line traversal
            Vector3 stepVector = new Vector3(direction.x / distance, 0, direction.z / distance);

            // Walk along the line and dig out width at each step
            for (float t = 0; t <= distance; t += 0.5f)
            {
                Vector3 currentPos = start.ToVector3() + (stepVector * t);
                IntVec3 centerCell = new IntVec3(
                    Mathf.RoundToInt(currentPos.x),
                    0,
                    Mathf.RoundToInt(currentPos.z)
                );

                // Add cells for width around this center point
                DigAtPosition(centerCell, direction, width, cells, map);
            }

            return new List<IntVec3>(cells);
        }

        private void DigAtPosition(IntVec3 center, IntVec3 direction, int width, HashSet<IntVec3> cells, Map map)
        {
            // Get perpendicular direction for width
            IntVec3 perpendicularDir = GetPerpendicularDirection(direction);

            int halfWidth = width / 2;

            // Add cells along the perpendicular direction
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                IntVec3 cell = center + (perpendicularDir * i);

                if (cell.InBounds(map))
                {
                    cells.Add(cell);
                }
            }
        }

        private IntVec3 GetPerpendicularDirection(IntVec3 direction)
        {
            // For horizontal corridors, expand vertically
            // For vertical corridors, expand horizontally
            if (Math.Abs(direction.x) >= Math.Abs(direction.z))
            {
                return IntVec3.South; // Use Z direction for width
            }
            else
            {
                return IntVec3.East; // Use X direction for width
            }
        }
    }
}
