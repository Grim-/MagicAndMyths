using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public abstract class CorridorPathBase
    {
        public bool smoothCorners = true;



        public virtual bool FitnessTest(IntVec3 start, IntVec3 end, Map map)
        {
            return true;
        }


        public abstract List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map);

        public virtual List<IntVec3> GeneratePathWithWidth(IntVec3 start, IntVec3 end, Map map, int width = 1)
        {
            var spinePath = GeneratePath(start, end, map);
            if (width <= 1) 
                return spinePath;

            var widePath = ExpandPathToWidth(spinePath, width);
            return smoothCorners ? SmoothCorners(widePath) : widePath;
        }

        protected void AddPointsAlongLine(List<IntVec3> path, IntVec3 start, IntVec3 end)
        {
            int dx = end.x - start.x;
            int dz = end.z - start.z;
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dz));
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0 : (float)i / steps;
                int x = start.x + (int)Math.Round(dx * t);
                int z = start.z + (int)Math.Round(dz * t);
                path.Add(new IntVec3(x, 0, z));
            }
        }

        protected List<IntVec3> ExpandPathToWidth(List<IntVec3> spinePath, int width)
        {
            if (width <= 1) 
                return spinePath;

            HashSet<IntVec3> expandedCells = new HashSet<IntVec3>(spinePath);

            foreach (var cell in spinePath)
            {
                // Get local direction at this point
                IntVec3 localDirection = GetLocalDirection(cell, spinePath);
                IntVec3 perpendicular = new IntVec3(-localDirection.z, 0, localDirection.x);

                if (perpendicular.LengthHorizontalSquared > 0)
                {
                    // Normalize perpendicular vector
                    float length = perpendicular.LengthHorizontal;
                    IntVec3 normalizedPerp = new IntVec3(
                        (int)Math.Round(perpendicular.x / length),
                        0,
                        (int)Math.Round(perpendicular.z / length)
                    );

                    // For width=2, add 1 cell to one side only
                    // For width=3, add 1 cell to each side  
                    // For width=4, add 1 to one side, 2 to other, etc.
                    int additionalCells = width - 1;
                    int leftSide = additionalCells / 2;
                    int rightSide = additionalCells - leftSide;

                    // Add cells to the left
                    for (int i = 1; i <= leftSide; i++)
                    {
                        expandedCells.Add(new IntVec3(
                            cell.x - i * normalizedPerp.x,
                            cell.y,
                            cell.z - i * normalizedPerp.z
                        ));
                    }

                    // Add cells to the right
                    for (int i = 1; i <= rightSide; i++)
                    {
                        expandedCells.Add(new IntVec3(
                            cell.x + i * normalizedPerp.x,
                            cell.y,
                            cell.z + i * normalizedPerp.z
                        ));
                    }
                }
            }

            return expandedCells.ToList();
        }

        private IntVec3 GetLocalDirection(IntVec3 currentCell, List<IntVec3> path)
        {
            int index = path.IndexOf(currentCell);

            if (index == 0 && path.Count > 1)
            {
                // First cell - use direction to next
                return path[1] - currentCell;
            }
            else if (index == path.Count - 1 && path.Count > 1)
            {
                // Last cell - use direction from previous
                return currentCell - path[index - 1];
            }
            else if (index > 0 && index < path.Count - 1)
            {
                // Middle cell - use average direction
                IntVec3 dirToPrev = currentCell - path[index - 1];
                IntVec3 dirToNext = path[index + 1] - currentCell;
                return new IntVec3(
                    (dirToPrev.x + dirToNext.x) / 2,
                    0,
                    (dirToPrev.z + dirToNext.z) / 2
                );
            }

            // Fallback
            return IntVec3.North;
        }

        protected List<IntVec3> SmoothCorners(List<IntVec3> path)
        {
            var smoothed = new HashSet<IntVec3>(path);

            foreach (var cell in path.ToList())
            {
                if (IsCornerCell(cell, path))
                {
                    foreach (var neighbor in GenAdjFast.AdjacentCellsCardinal(cell))
                    {
                        smoothed.Add(neighbor);
                    }
                }
            }

            return smoothed.ToList();
        }

        private bool IsCornerCell(IntVec3 cell, List<IntVec3> path)
        {
            var pathSet = new HashSet<IntVec3>(path);
            int connectionCount = 0;

            foreach (var neighbor in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (pathSet.Contains(neighbor)) connectionCount++;
            }

            return connectionCount >= 2;
        }
    }
}
