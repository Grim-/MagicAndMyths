using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class GenStep_Dungeon : GenStep
    {
        public override int SeedPart => 654321;

        public override void Generate(Map map, GenStepParams parms)
        {
            // 1. Fill map with walls
            foreach (IntVec3 cell in map.AllCells)
            {
                GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, cell, map);
                map.terrainGrid.SetUnderTerrain(cell, TerrainDefOf.Voidmetal);
            }

            // 2. Create initial random open spaces
            BoolGrid dungeonGrid = new BoolGrid(map);
            foreach (IntVec3 cell in map.AllCells)
            {
                if (Rand.Chance(0.4f))
                    dungeonGrid[cell] = true;
            }

            // 3. Run cellular automaton
            for (int i = 0; i < 1000; i++)
            {
                BoolGrid newGrid = new BoolGrid(map);
                foreach (IntVec3 cell in map.AllCells)
                {
                    int neighborCount = 0;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            IntVec3 neighbor = new IntVec3(cell.x + dx, 0, cell.z + dz);
                            if (neighbor.InBounds(map) && dungeonGrid[neighbor])
                                neighborCount++;
                        }
                    }
                    if (dungeonGrid[cell])
                        newGrid[cell] = neighborCount >= 4; // Stay open if 4+ neighbors
                    else
                        newGrid[cell] = neighborCount >= 5; // Become open if 5+ neighbors
                }
                dungeonGrid = newGrid;
            }

            // 4. Make sure the entrance area is clear
            CellRect entranceRect = CellRect.CenteredOn(map.Center, 5, 5);
            foreach (IntVec3 cell in entranceRect)
            {
                if (cell.InBounds(map))
                {
                    dungeonGrid[cell] = true;
                }
            }

            // 5. Identify separate regions
            List<List<IntVec3>> regions = FindRegions(map, dungeonGrid);

            // 7. Track door positions while connecting regions
            List<IntVec3> doorPositions = new List<IntVec3>();

            // Connect regions and track door positions
            ConnectRegions(map, dungeonGrid, regions, doorPositions);

            // 8. Apply changes to the map
            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell])
                {
                    map.thingGrid.ThingsAt(cell)
                        .ToList()
                        .ForEach(t => t.Destroy());
                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.Voidmetal);
                }
            }

            // 9. Place doors at marked positions
            foreach (IntVec3 doorPos in doorPositions)
            {
                GenSpawn.Spawn(ThingDefOf.Door, doorPos, map);
            }
        }

        private List<List<IntVec3>> FindRegions(Map map, BoolGrid dungeonGrid)
        {
            List<List<IntVec3>> regions = new List<List<IntVec3>>();
            BoolGrid visitedGrid = new BoolGrid(map);

            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell] && !visitedGrid[cell])
                {
                    // Found a new region, flood fill to find all connected cells
                    List<IntVec3> region = new List<IntVec3>();
                    Queue<IntVec3> queue = new Queue<IntVec3>();

                    visitedGrid[cell] = true;
                    queue.Enqueue(cell);
                    region.Add(cell);

                    while (queue.Count > 0)
                    {
                        IntVec3 current = queue.Dequeue();

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dz = -1; dz <= 1; dz++)
                            {
                                // Only cardinal directions (no diagonals)
                                if (Math.Abs(dx) + Math.Abs(dz) != 1) continue;

                                IntVec3 neighbor = new IntVec3(current.x + dx, 0, current.z + dz);
                                if (neighbor.InBounds(map) && dungeonGrid[neighbor] && !visitedGrid[neighbor])
                                {
                                    visitedGrid[neighbor] = true;
                                    queue.Enqueue(neighbor);
                                    region.Add(neighbor);
                                }
                            }
                        }
                    }

                    // Only add regions with a minimum size
                    if (region.Count >= 20)
                    {
                        regions.Add(region);
                    }
                }
            }

            return regions;
        }

        private void ConnectRegions(Map map, BoolGrid dungeonGrid, List<List<IntVec3>> regions, List<IntVec3> doorPositions)
        {
            if (regions.Count <= 1) return; // Nothing to connect

            // Create a minimum spanning tree to connect all regions
            List<Tuple<int, int, int>> edges = new List<Tuple<int, int, int>>();

            // Find the closest pair of cells between each region pair
            for (int i = 0; i < regions.Count; i++)
            {
                for (int j = i + 1; j < regions.Count; j++)
                {
                    IntVec3 bestCell1 = regions[i][0];
                    IntVec3 bestCell2 = regions[j][0];
                    int bestDistance = int.MaxValue;

                    // Sample only a subset of cells for large regions to improve performance
                    List<IntVec3> sampledRegion1 = SampleRegion(regions[i], 100);
                    List<IntVec3> sampledRegion2 = SampleRegion(regions[j], 100);

                    foreach (IntVec3 cell1 in sampledRegion1)
                    {
                        foreach (IntVec3 cell2 in sampledRegion2)
                        {
                            int distance = Math.Abs(cell1.x - cell2.x) + Math.Abs(cell1.z - cell2.z);
                            if (distance < bestDistance)
                            {
                                bestDistance = distance;
                                bestCell1 = cell1;
                                bestCell2 = cell2;
                            }
                        }
                    }

                    edges.Add(new Tuple<int, int, int>(i, j, bestDistance));
                }
            }

            // Sort edges by distance
            edges.Sort((a, b) => a.Item3.CompareTo(b.Item3));

            // Create a union-find data structure for Kruskal's algorithm
            int[] parent = new int[regions.Count];
            for (int i = 0; i < regions.Count; i++)
            {
                parent[i] = i;
            }

            // Function to find the root of a set
            Func<int, int> find = null;
            find = x => parent[x] == x ? x : parent[x] = find(parent[x]);

            // Build the minimum spanning tree
            List<Tuple<int, int>> mstEdges = new List<Tuple<int, int>>();
            foreach (var edge in edges)
            {
                int root1 = find(edge.Item1);
                int root2 = find(edge.Item2);

                if (root1 != root2)
                {
                    mstEdges.Add(new Tuple<int, int>(edge.Item1, edge.Item2));
                    parent[root1] = root2;
                }
            }

            // Connect regions with tunnels
            foreach (var edge in mstEdges)
            {
                IntVec3 start = FindBestConnector(regions[edge.Item1], map, dungeonGrid);
                IntVec3 end = FindBestConnector(regions[edge.Item2], map, dungeonGrid);

                // Create a tunnel between start and end, collecting door positions
                List<IntVec3> tunnelDoors = new List<IntVec3>();
                CreateTunnel(start, end, map, dungeonGrid, tunnelDoors);
                doorPositions.AddRange(tunnelDoors);
            }
        }

        private List<IntVec3> SampleRegion(List<IntVec3> region, int maxSamples)
        {
            if (region.Count <= maxSamples) return region;

            List<IntVec3> result = new List<IntVec3>();
            int step = region.Count / maxSamples;

            for (int i = 0; i < region.Count; i += step)
            {
                result.Add(region[i]);
            }

            return result;
        }

        private IntVec3 FindBestConnector(List<IntVec3> region, Map map, BoolGrid dungeonGrid)
        {
            // Find a cell that's good for connecting (has some wall neighbors)
            foreach (IntVec3 cell in region)
            {
                int wallNeighbors = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (Math.Abs(dx) + Math.Abs(dz) != 1) continue;

                        IntVec3 neighbor = new IntVec3(cell.x + dx, 0, cell.z + dz);
                        if (neighbor.InBounds(map) && !dungeonGrid[neighbor])
                        {
                            wallNeighbors++;
                        }
                    }
                }

                if (wallNeighbors > 0)
                {
                    return cell;
                }
            }

            // Fallback to using a random cell
            return region[Rand.Range(0, region.Count)];
        }

        private void CreateTunnel(IntVec3 start, IntVec3 end, Map map, BoolGrid dungeonGrid, List<IntVec3> doorPositions)
        {
            // Use a simple L-shaped tunnel
            IntVec3 corner = new IntVec3(start.x, 0, end.z);

            // Determine if we should use a different corner configuration
            if (Rand.Value < 0.5f)
            {
                corner = new IntVec3(end.x, 0, start.z);
            }

            // Find positions for the doors (one cell away from start and end)
            IntVec3 doorStart = GetDoorPosition(start, corner, map);
            IntVec3 doorEnd = GetDoorPosition(end, corner, map);

            // Create the first straight segment
            DrawLine(start, corner, map, dungeonGrid);

            // Create the second straight segment
            DrawLine(corner, end, map, dungeonGrid);

            // Add door positions to the list
            doorPositions.Add(doorStart);
            doorPositions.Add(doorEnd);
        }

        private IntVec3 GetDoorPosition(IntVec3 roomCell, IntVec3 corner, Map map)
        {
            // Get direction from room to corridor
            int dx = Math.Sign(corner.x - roomCell.x);
            int dz = Math.Sign(corner.z - roomCell.z);

            // Door position is one step out of the room
            return new IntVec3(roomCell.x + dx, 0, roomCell.z + dz);
        }

        private void DrawLine(IntVec3 start, IntVec3 end, Map map, BoolGrid dungeonGrid)
        {
            int dx = Math.Sign(end.x - start.x);
            int dz = Math.Sign(end.z - start.z);

            IntVec3 current = start;

            while (current.x != end.x || current.z != end.z)
            {
                // Carve horizontal component
                if (current.x != end.x)
                {
                    current = new IntVec3(current.x + dx, 0, current.z);
                }
                // Carve vertical component
                else if (current.z != end.z)
                {
                    current = new IntVec3(current.x, 0, current.z + dz);
                }

                // Single-width corridor
                if (current.InBounds(map))
                {
                    dungeonGrid[current] = true;
                }
            }
        }
    }

}
