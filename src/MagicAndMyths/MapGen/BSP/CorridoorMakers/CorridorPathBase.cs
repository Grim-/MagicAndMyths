using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public abstract class CorridorPathBase
    {
        public int width = 3;
        public bool smoothCorners = true;

        public abstract List<IntVec3> GeneratePath(IntVec3 start, IntVec3 end, Map map);

        public virtual List<IntVec3> GeneratePathWithWidth(IntVec3 start, IntVec3 end, Map map)
        {
            var spinePath = GeneratePath(start, end, map);
            if (width <= 1) return spinePath;

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
            if (width <= 1) return spinePath;

            HashSet<IntVec3> expandedCells = new HashSet<IntVec3>();
            int radius = width / 2;

            foreach (var cell in spinePath)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        if (width % 2 == 0 && (x > radius || z > radius)) continue;
                        expandedCells.Add(new IntVec3(cell.x + x, cell.y, cell.z + z));
                    }
                }
            }

            return expandedCells.ToList();
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
