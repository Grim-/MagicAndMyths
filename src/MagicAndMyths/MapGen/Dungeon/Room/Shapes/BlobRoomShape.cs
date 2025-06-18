using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class BlobRoomShape : RoomShapeBase
    {
        public float irregularity = 0.3f;
        public int seed = 12345;
        public int noiseOctaves = 3;
        public float noiseScale = 0.1f;

        public override List<IntVec3> GenerateRoomCells(DungeonGenerationContext context, CellRect bounds, float sizeMultiplier)
        {
            List<IntVec3> cells = new List<IntVec3>();
            IntVec3 center = bounds.CenterCell;
            float baseRadiusX = bounds.Width / 2f * sizeMultiplier;
            float baseRadiusZ = bounds.Height / 2f * sizeMultiplier;

            System.Random rand = new System.Random(seed + center.x + center.z);

            for (int x = bounds.minX; x <= bounds.maxX; x++)
            {
                for (int z = bounds.minZ; z <= bounds.maxZ; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);
                    float dx = x - center.x;
                    float dz = z - center.z;

                    if (dx == 0 && dz == 0)
                    {
                        cells.Add(cell);
                        continue;
                    }

                    float angle = Mathf.Atan2(dz, dx);
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);

                    float noise = 0f;
                    float amplitude = irregularity;
                    float frequency = 1f;

                    for (int octave = 0; octave < noiseOctaves; octave++)
                    {
                        float sampleX = Mathf.Cos(angle * frequency) * distance * noiseScale + seed;
                        float sampleZ = Mathf.Sin(angle * frequency) * distance * noiseScale + seed;
                        noise += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
                        amplitude *= 0.5f;
                        frequency *= 2f;
                    }

                    float radiusVariation = 1f + (noise - 0.5f) * 2f;
                    float effectiveRadiusX = baseRadiusX * radiusVariation;
                    float effectiveRadiusZ = baseRadiusZ * radiusVariation;

                    float normalizedDistance = (dx * dx) / (effectiveRadiusX * effectiveRadiusX) +
                                             (dz * dz) / (effectiveRadiusZ * effectiveRadiusZ);

                    if (normalizedDistance <= 1.0f)
                    {
                        cells.Add(cell);
                    }
                }
            }

            cells = RemoveIsolatedCells(cells);
            cells = EnsureMinimumSize(cells, center);

            return cells;
        }

        private List<IntVec3> RemoveIsolatedCells(List<IntVec3> cells)
        {
            HashSet<IntVec3> cellSet = new HashSet<IntVec3>(cells);
            List<IntVec3> connectedCells = new List<IntVec3>();

            foreach (IntVec3 cell in cells)
            {
                int neighborCount = 0;
                foreach (IntVec3 neighbor in GenAdj.AdjacentCells)
                {
                    if (cellSet.Contains(cell + neighbor))
                    {
                        neighborCount++;
                    }
                }

                if (neighborCount >= 2)
                {
                    connectedCells.Add(cell);
                }
            }

            return connectedCells;
        }

        private List<IntVec3> EnsureMinimumSize(List<IntVec3> cells, IntVec3 center)
        {
            HashSet<IntVec3> cellSet = new HashSet<IntVec3>(cells);

            if (!cellSet.Contains(center))
            {
                cellSet.Add(center);
            }

            foreach (IntVec3 adj in GenAdj.AdjacentCells)
            {
                cellSet.Add(center + adj);
            }

            return new List<IntVec3>(cellSet);
        }
    }
}
