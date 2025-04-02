using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class GenStepDef_BspDungeon : GenStepDef
    {
        public int randomCorridoorAmount = 1;
        public int maxDepth = 8;
        public int minRoomSize = 8;
        public int minRoomPadding = 3;
        public float roomSizeFactor = 1f;

        public int minRooms = 3;
        public int maxRooms = 3;

        public float minSizeMultiplier = 1.0f;
        public float aspectRatioThreshold = 1.1f;
        public float edgeMarginDivisor = 2f;

        public bool addRandomCorridoors = true;
    }

    public class DungeonRoom
    {
        public RoomType type = RoomType.Normal;
        public float distanceFromStart = 0f;
    }
    public class GenStep_BspDungeon : GenStep
    {

        GenStepDef_BspDungeon Def => (GenStepDef_BspDungeon)def;
        public override int SeedPart => 654321;

        public int randomCorridoorAmount = 1;


        public override void Generate(Map map, GenStepParams parms)
        {
            foreach (IntVec3 cell in map.AllCells)
            {
                GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, cell, map);
                map.terrainGrid.SetUnderTerrain(cell, TerrainDefOf.MetalTile);
            }

            BoolGrid dungeonGrid = new BoolGrid(map);

            BspUtility.BspNode rootNode = BspUtility.GenerateBspTreeWithRoomCount(
                new CellRect(2, 2, map.Size.x - 4, map.Size.z - 4),
                minRooms: Def.minRooms,
                maxRooms: Def.maxRooms,
                minRoomSize: Def.minRoomSize,
                maxSplitAttempts: 200,
                minSizeMultiplier: Def.minSizeMultiplier,
                aspectRatioThreshold: Def.aspectRatioThreshold,
                edgeMarginDivisor: Def.edgeMarginDivisor);

            List<BspUtility.BspNode> leafNodes = new List<BspUtility.BspNode>();
            BspUtility.GetLeafNodes(rootNode, leafNodes);

            BspUtility.GenerateRooms(leafNodes, minPadding: Def.minRoomPadding, roomSizeFactor: Def.roomSizeFactor);

            foreach (var node in leafNodes)
            {
                node.tag = new DungeonRoom();
            }

            BspUtility.CreateMinimumSpanningTree(leafNodes);

            if (Def.addRandomCorridoors)
            {
                int extraConnections = Rand.Range(1, Math.Max(Def.randomCorridoorAmount, leafNodes.Count / 5));
                BspUtility.AddRandomConnections(leafNodes, extraConnections);
            }

            ApplyRooms(map, leafNodes, dungeonGrid);
            List<Corridoor> corridorSegments = new List<Corridoor>();

            HashSet<string> processedConnections = new HashSet<string>();
            foreach (var node in leafNodes)
            {
                foreach (var connectedNode in node.connectedNodes)
                {
                    string connectionId = GetConnectionId(node, connectedNode);

                    if (!processedConnections.Contains(connectionId) &&
                        node.room != null && connectedNode.room != null)
                    {
                        corridorSegments.AddRange(BspUtility.GenerateCorridorPoints(node, connectedNode));
                        processedConnections.Add(connectionId);
                    }
                }
            }

            BspUtility.ApplyCorridorsToGrid(corridorSegments, map, dungeonGrid);

            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell])
                {
                    map.thingGrid.ThingsAt(cell)
                        .ToList()
                        .ForEach(t => t.Destroy());

                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
                }
            }

            DesignateRoomTypes(map, leafNodes);
            SpawnDoors(map, leafNodes);
            ObstacleGenerator.GenerateObstacles(map, rootNode, leafNodes);

            CellularAutomataManager.ApplyRules(map, dungeonGrid, new List<CellularAutomataWorker>()
            {
                new CAWorker_NaturalWalls()
            });

            foreach (IntVec3 cell in map.AllCells)
            {
                if (dungeonGrid[cell])
                {
                    // This is a floor cell - remove any walls
                    map.thingGrid.ThingsAt(cell)
                        .ToList()
                        .ForEach(t => t.Destroy());

                    map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
                }
                else
                {
                    // This is a wall cell - ensure a wall exists
                    Thing existingThing = cell.GetFirstBuilding(map);
                    if (existingThing == null || existingThing.def != MagicAndMythDefOf.DungeonWall)
                    {
                        // Clean up any existing thing
                        if (existingThing != null)
                            existingThing.Destroy();

                        // Spawn a wall
                        GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, cell, map);
                    }
                }
            }
        }

        private string GetConnectionId(BspUtility.BspNode node1, BspUtility.BspNode node2)
        {
            // Order nodes by their memory addresses to ensure consistent IDs
            ulong id1 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node1);
            ulong id2 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node2);
            return id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";
        }


        private void ApplyRooms(Map map, List<BspUtility.BspNode> leafNodes, BoolGrid dungeonGrid)
        {
            foreach (var node in leafNodes)
            {
                foreach (IntVec3 cell in node.room)
                {
                    if (cell.InBounds(map))
                    {
                        dungeonGrid[cell] = true;
                    }
                }
            }
        }
 
        private void DesignateRoomTypes(Map map, List<BspUtility.BspNode> rooms)
        {
            if (rooms.Count < 4)
                return;

            foreach (var node in rooms)
            {
                if (node.tag == null)
                    node.tag = new DungeonRoom();
            }

            var furthestPair = BspUtility.FindFurthestPair(rooms);
            if (furthestPair.Item1 != null && furthestPair.Item2 != null)
            {
                var startNode = furthestPair.Item1;
                var endNode = furthestPair.Item2;

                MagicAndMythDefOf.StartRoom.DoWorker(map, startNode.room);
                MagicAndMythDefOf.EndRoom.DoWorker(map, endNode.room);

                foreach (var node in rooms)
                {
                    if (node != startNode && node != endNode)
                    {
                        float distance = Vector3.Distance(
                            node.room.CenterCell.ToVector3(),
                            startNode.room.CenterCell.ToVector3());
                        ((DungeonRoom)node.tag).distanceFromStart = distance;
                    }
                }

                List<BspUtility.BspNode> sortedRooms = rooms
                    .Where(r => r != startNode && r != endNode)
                    .OrderBy(r => ((DungeonRoom)r.tag).distanceFromStart)
                    .ToList();

                List<RoomTypeDef> randomRooms = DefDatabase<RoomTypeDef>.AllDefs
                    .Where(x => x.roomType == RoomType.Normal)
                    .ToList();

                foreach (var item in sortedRooms)
                {
                    randomRooms.RandomElement().DoWorker(map, item.room);
                }
            }
        }

        private void SpawnDoors(Map map, List<BspUtility.BspNode> rooms)
        {
            foreach (var room in rooms)
            {
                foreach (var item in room.roomWalls.EdgeCells)
                {
                    if (item.GetFirstBuilding(map) == null)
                    {
                        bool hasOppositeWalls = false;
                        IntVec3 left = new IntVec3(item.x - 1, item.y, item.z);
                        IntVec3 right = new IntVec3(item.x + 1, item.y, item.z);
                        if (IsWall(map, left) && IsWall(map, right))
                        {
                            hasOppositeWalls = true;
                        }

                        if (!hasOppositeWalls)
                        {
                            IntVec3 top = new IntVec3(item.x, item.y, item.z - 1);
                            IntVec3 bottom = new IntVec3(item.x, item.y, item.z + 1);
                            if (IsWall(map, top) && IsWall(map, bottom))
                            {
                                hasOppositeWalls = true;
                            }
                        }

                        if (hasOppositeWalls)
                        {
                            GenSpawn.Spawn(ThingDefOf.Door, item, map);
                        }
                    }
                }
            }
        }

        private bool IsWall(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map)) return false;
            Building building = cell.GetFirstBuilding(map);
            return building != null && building.def == MagicAndMythDefOf.DungeonWall;
        }
    }
}
