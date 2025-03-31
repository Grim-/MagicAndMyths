using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace MagicAndMyths
{
    public class DungeonGen_CaveSystem : DungeonGen
    {
        private const float CAVE_DENSITY = 0.7f; // How much of the map should be caves
        private readonly ThingDef wallDef = ThingDefOf.Wall;
        private readonly TerrainDef floorDef = TerrainDefOf.Concrete;
        private ModuleBase directionNoise;

        public override int Priority => 10;

        public DungeonGen_CaveSystem(Map map) : base(map)
        {
            this.directionNoise = new Perlin(0.002050000010058284, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
        }

        public override void Generate()
        {
            // First fill the entire map with walls
            FillMapWithWalls();

            // Then generate cave system
            GenerateCaves();
        }

        private void FillMapWithWalls()
        {
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int z = 0; z < map.Size.z; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);

                    // Clear the cell first
                    map.terrainGrid.SetTerrain(cell, floorDef);
                    foreach (Thing t in cell.GetThingList(map).ToList())
                    {
                        t.DeSpawn(DestroyMode.Vanish);
                    }

                    // Place wall
                    Thing wall = ThingMaker.MakeThing(wallDef, ThingDefOf.Steel);
                    GenSpawn.Spawn(wall, cell, map);
                }
            }
        }

        private void GenerateCaves()
        {
            BoolGrid visited = new BoolGrid(map);
            MapGenFloatGrid caves = MapGenerator.Caves;
            List<IntVec3> cavePoints = new List<IntVec3>();

            // Select random starting points for caves
            int numStartPoints = (int)(map.Size.x * map.Size.z * 0.001f);
            for (int i = 0; i < numStartPoints; i++)
            {
                IntVec3 start = new IntVec3(
                    Rand.Range(10, map.Size.x - 10),
                    0,
                    Rand.Range(10, map.Size.z - 10)
                );

                if (!visited[start])
                {
                    DigCaveTunnel(start, Rand.Range(0f, 360f), Rand.Range(3f, 6f), visited, caves);
                }
            }

            ApplyCavesToMap(caves);
        }

        private void DigCaveTunnel(IntVec3 start, float initialDir, float width, BoolGrid visited, MapGenFloatGrid caves)
        {
            Vector3 currentPos = start.ToVector3Shifted();
            float currentDir = initialDir;
            float distanceTraveled = 0f;

            int maxLength = Math.Min(map.Size.x, map.Size.z) * 2;
            int currentLength = 0;

            while (currentLength < maxLength)
            {
                IntVec3 currentCell = currentPos.ToIntVec3();

                // Check bounds
                if (!currentCell.InBounds(map))
                    break;

                // Mark cell as cave
                if (!visited[currentCell])
                {
                    visited[currentCell] = true;
                    caves[currentCell] = width;

                    // Add some random variation to tunnel width
                    int radius = Mathf.CeilToInt(width / 2f);
                    foreach (IntVec3 neighbor in GenRadial.RadialCellsAround(currentCell, radius, true))
                    {
                        if (neighbor.InBounds(map) && !visited[neighbor])
                        {
                            visited[neighbor] = true;
                            caves[neighbor] = width * 0.8f;
                        }
                    }
                }

                // Random direction changes
                float noise = (float)directionNoise.GetValue(
                    distanceTraveled * 60.0,
                    (double)start.x * 200.0,
                    (double)start.z * 200.0
                );
                currentDir += noise * 8f;

                // Move forward
                currentPos += Vector3Utility.FromAngleFlat(currentDir) * 0.5f;
                distanceTraveled += 0.5f;
                currentLength++;

                // Random branching
                if (currentLength > 15 && Rand.Chance(0.1f))
                {
                    DigCaveTunnel(
                        currentCell,
                        currentDir + Rand.Range(-90f, 90f),
                        width * Rand.Range(0.6f, 0.8f),
                        visited,
                        caves
                    );
                }

                // Gradually reduce width
                width -= 0.034f;
                if (width < 1.4f)
                    break;
            }
        }

        private void ApplyCavesToMap(MapGenFloatGrid caves)
        {
            // Convert cave grid to actual map changes
            for (int x = 0; x < map.Size.x; x++)
            {
                for (int z = 0; z < map.Size.z; z++)
                {
                    IntVec3 cell = new IntVec3(x, 0, z);
                    if (caves[cell] > 0f)
                    {
                        // Remove walls where caves should be
                        foreach (Thing t in cell.GetThingList(map).ToList())
                        {
                            if (t.def == wallDef)
                            {
                                t.Destroy();
                            }
                        }

                        // Set floor
                        map.terrainGrid.SetTerrain(cell, floorDef);
                    }
                }
            }
        }
    }
}

