using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public struct PlannedRoom
    {
        public RoomTypeDef RoomType;
    }

    public class DungeonGenerator
    {
        private Dungeon dungeon;
        public Dungeon GeneratedDungeon
        {
            get => dungeon;
        }
        private GenStepDef_BspDungeon def;


        private Map map;

        public DungeonGenerator(Map map, GenStepDef_BspDungeon def)
        {
            this.dungeon = new Dungeon(map);
            this.def = def;
            this.map = map;
        }

        public void Generate()
        {
            // Phase 1: Initialize the dungeon
            FillMapWithWalls();

            // Phase 2: Generate BSP structure
            GenerateBspStructure();

            ProcessPlannedRooms(def.availableRoomTypes);

            // Phase 3: Create rooms from BSP nodes
            CreateRoomsFromBspNodes();

            // Phase 4: Apply early processing
            ApplyEarlyAutomata();

            // Phase 5: Assign room types
            Log.Message("Assigning room types");
            AssignRoomTypes();

            // Phase 6: Connections
            Log.Message("Creating room connections");
            CreateMinimumSpanningTree();
            List<RoomConnection> connections = GenerateRoomConnections(map);
            EnsureAllRoomsConnected();

            // Phase 7: Create critical path 
            DesignateCriticalPath();

            ApplyConnectionsToGrid(connections);
            ApplyRoomsToGrid();

            ClearWalls();

            if (def.postGenAutomata != null)
            {
                Log.Message("Applying Post-Generation Cellular Automata");
                CellularAutomataManager.ApplyRules(dungeon.Map, dungeon.nodeToRoomMap, dungeon.DungeonGrid, def.postGenAutomata);
            }

            foreach (var room in dungeon.GetAllRooms())
            {
                if (room.def != null)
                {
                    room.def.DoWorker(dungeon.Map, room);
                }
            }

            ObstacleGenerator.GenerateObstacles(dungeon.Map, dungeon);
        }

        private void FillMapWithWalls()
        {
            Log.Message("Filling map with walls...");

            foreach (IntVec3 cell in dungeon.Map.AllCells)
            {
                GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, cell, dungeon.Map);
                dungeon.Map.terrainGrid.SetUnderTerrain(cell, TerrainDefOf.MetalTile);
            }
        }

        private void GenerateBspStructure()
        {
            Log.Message("Generating BSP tree structure");
            CellRect mapArea = new CellRect(2, 2, dungeon.Map.Size.x - 4, dungeon.Map.Size.z - 4);
            BspNode rootNode = BspUtility.GenerateBspTreeWithRoomCount(
                mapArea,
                minRooms: def.minRooms,
                maxRooms: def.maxRooms,
                minRoomSize: def.minRoomSize,
                maxSplitAttempts: 200,
                minSizeMultiplier: def.minSizeMultiplier,
                aspectRatioThreshold: def.aspectRatioThreshold,
                edgeMarginDivisor: def.edgeMarginDivisor);

            List<BspNode> leafNodes = new List<BspNode>();
            BspUtility.GetLeafNodes(rootNode, leafNodes);

            dungeon.SetBspStructure(rootNode, leafNodes);

            BspUtility.GenerateRoomGeometry(dungeon.LeafNodes,
                minPadding: def.minRoomPadding,
                roomSizeFactor: def.roomSizeFactor);
        }

        //this is shit
        private void ProcessPlannedRooms(List<RoomTypeDef> plannedRooms)
        {
            if (plannedRooms == null || plannedRooms.Count == 0)
                return;

            // Sort nodes by size to better match requirements
            dungeon.LeafNodes.Sort((a, b) =>
                (b.rect.Width * b.rect.Height).CompareTo(a.rect.Width * a.rect.Height));

            // Track which nodes have been assigned
            HashSet<BspNode> assignedNodes = new HashSet<BspNode>();

            //ry to find exact matches for planned rooms
            foreach (var plannedRoom in plannedRooms)
            {
                BspNode bestNode = null;
                float bestFitScore = float.MaxValue;

                //the node that best fits this planned room
                foreach (var node in dungeon.LeafNodes)
                {
                    if (assignedNodes.Contains(node))
                        continue;

                    // Check if node is large enough
                    if (node.rect.Width < plannedRoom.minSize.x + (def.minRoomPadding * 2) ||
                        node.rect.Height < plannedRoom.minSize.z + (def.minRoomPadding * 2))
                        continue;

                    // Calculate how well this node fits (lower is better)
                    float fitScore = (node.rect.Width * node.rect.Height) -
                                    (plannedRoom.minSize.x * plannedRoom.minSize.z);

                    if (fitScore < bestFitScore)
                    {
                        bestFitScore = fitScore;
                        bestNode = node;
                    }
                }

                // If we found a suitable node, assign the room
                if (bestNode != null)
                {
                    // Get the center of the node
                    int centerX = bestNode.rect.minX + bestNode.rect.Width / 2;
                    int centerZ = bestNode.rect.minZ + bestNode.rect.Height / 2;

                    // Calculate room position (centered in node)
                    int x = centerX - plannedRoom.minSize.x / 2;
                    int z = centerZ - plannedRoom.minSize.z / 2;

                    // Ensure room stays within node boundaries with padding
                    x = Math.Max(bestNode.rect.minX + def.minRoomPadding,
                            Math.Min(x, bestNode.rect.maxX - plannedRoom.minSize.x - def.minRoomPadding));
                    z = Math.Max(bestNode.rect.minZ + def.minRoomPadding,
                            Math.Min(z, bestNode.rect.maxZ - plannedRoom.minSize.z - def.minRoomPadding));

                    // Set the room rectangle
                    bestNode.roomRect = new CellRect(x, z, plannedRoom.minSize.x, plannedRoom.minSize.z);

                    // Create and assign the room
                    DungeonRoom room = DungeonRoom.FromBspNode(bestNode);
                    room.def = plannedRoom;
                    dungeon.AddRoom(bestNode, room);

                    // Mark as assigned
                    assignedNodes.Add(bestNode);
                }
                else
                {
                    // No suitable node found - need to split an existing node
                    Log.Message($"No suitable node for planned room {plannedRoom}. Attempting to create one...");

                    // Find the largest unassigned node
                    BspNode largestNode = dungeon.LeafNodes
                        .Where(n => !assignedNodes.Contains(n))
                        .OrderByDescending(n => n.rect.Width * n.rect.Height)
                        .FirstOrDefault();

                    if (largestNode != null)
                    {
                        // Force the node's room rect to our planned size
                        int x = largestNode.rect.minX + Math.Max(def.minRoomPadding,
                                    (largestNode.rect.Width - plannedRoom.minSize.x) / 2);
                        int z = largestNode.rect.minZ + Math.Max(def.minRoomPadding,
                                    (largestNode.rect.Height - plannedRoom.minSize.z) / 2);

                        largestNode.roomRect = new CellRect(x, z, plannedRoom.minSize.x, plannedRoom.minSize.z);

                        // Create and assign the room
                        DungeonRoom room = DungeonRoom.FromBspNode(largestNode);
                        room.def = plannedRoom;
                        dungeon.AddRoom(largestNode, room);

                        assignedNodes.Add(largestNode);
                    }
                    else
                    {
                        Log.Error($"Failed to create planned room {plannedRoom} - no nodes available!");
                    }
                }
            }
        }

        private void CreateRoomsFromBspNodes()
        {
            Log.Message("Creating dungeon rooms from BSP nodes");

            foreach (var node in dungeon.LeafNodes)
            {
                if (dungeon.HasMapping(node))
                    continue;

                DungeonRoom room = DungeonRoom.FromBspNode(node);
                dungeon.AddRoom(node, room);
            }
        }

        private void ApplyEarlyAutomata()
        {
            Log.Message("Applying Early Cellular Automata");

            if (def.earlyAutomata != null)
            {
                CellularAutomataManager.ApplyRules(dungeon.Map, dungeon.nodeToRoomMap, dungeon.DungeonGrid, def.earlyAutomata);
            }
        }

        private void AssignRoomTypes()
        {
            foreach (var node in dungeon.LeafNodes)
            {
                DungeonRoom room = dungeon.GetRoom(node);

                if (room.def != null)
                    continue;

                if (dungeon.StartNode != null)
                {
                    float distance = Vector3.Distance(
                        room.Center.ToVector3(),
                        dungeon.GetRoom(dungeon.StartNode).Center.ToVector3());
                    room.distanceFromStart = distance;
                }

                // Assign a random normal room type
                RoomTypeDef normalDef = def.availableRoomTypes
                    .Where(x => x.roomType == RoomType.Normal)
                    .RandomElement();
                room.def = normalDef;
            }
        }

        private void DesignateCriticalPath()
        {
            Log.Message("Designating Critical Path Using Graph Distance");

            //Pick a random room
            DungeonRoom firstRoom = dungeon.GetAllRooms().RandomElement();

            //Find furthest room from first
            DungeonRoom furthestFromFirst = dungeon.GetFurthestRoom(firstRoom);

            //Find furthest from *that*
            DungeonRoom startRoom = furthestFromFirst;
            DungeonRoom endRoom = dungeon.GetFurthestRoom(startRoom);

            startRoom.def = MagicAndMythDefOf.StartRoom;
            startRoom.AddTag("start");
            startRoom.SetCriticalPathIndex(0);

            endRoom.def = MagicAndMythDefOf.EndRoom;
            endRoom.AddTag("end");

            BspNode startNode = dungeon.GetNode(startRoom);
            BspNode endNode = dungeon.GetNode(endRoom);
            dungeon.SetCriticalPathEndpoints(startNode, endNode);

            List<DungeonRoom> path = dungeon.FindPathBetween(startRoom, endRoom);

            for (int i = 0; i < path.Count; i++)
            {
                path[i].SetCriticalPathIndex(i);
            }

            endRoom.SetCriticalPathIndex(path.Count - 1);

            for (int i = 0; i < path.Count - 1; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                Dungeon.ConnectRooms(a, b);
                a.AddConnectionTo(map, b);
                b.AddConnectionTo(map, a);
            }
        }

        private void CreateMinimumSpanningTree()
        {
            Log.Message("Generating Minimum Spanning Tree");

            // Convert BSP node connections to dungeon room connections
            MspUtility.CreateMinimumSpanningTree(dungeon.LeafNodes);

            // Transfer BSP connections to rooms
            foreach (var node in dungeon.LeafNodes)
            {
                DungeonRoom room = dungeon.GetRoom(node);

                foreach (var connectedNode in node.connectedNodes)
                {
                    DungeonRoom connectedRoom = dungeon.GetRoom(connectedNode);
                    if (!room.connectedRooms.Contains(connectedRoom))
                    {
                        room.connectedRooms.Add(connectedRoom);
                    }
                }
            }
        }

        private List<RoomConnection> GenerateRoomConnections(Map map)
        {
            Log.Message("Generating connections based on rules");
            List<DungeonRoom> allRooms = dungeon.GetAllRooms().ToList();

            List<RoomConnection> connections = new List<RoomConnection>();
            HashSet<string> processedConnections = new HashSet<string>();

            foreach (var nodePair in dungeon.LeafNodes)
            {
                DungeonRoom roomA = dungeon.GetRoom(nodePair);

                foreach (var connectedNode in nodePair.connectedNodes)
                {
                    DungeonRoom roomB = dungeon.GetRoom(connectedNode);

                    string connectionId = DungeonRoom.GetConnectionId(roomA, roomB);

                    if (!processedConnections.Contains(connectionId))
                    {
                        RoomConnection connection = new RoomConnection(roomA, roomB);
                        roomA.AddConnectionTo(map, roomB);
                        roomB.AddConnectionTo(map, roomA);
                        connection.corridors = CorridoorUtility.GenerateCorridors(dungeon.Map, roomA, roomB);
                        connections.Add(connection);
                        processedConnections.Add(connectionId);
                    }
                }
            }

            if (def.addRandomCorridoors)
            {
                // Add some random connections between non critical path nodes
                List<DungeonRoom> nonCriticalRooms = dungeon.GetAllRooms().Where(x => !x.IsOnCriticalPath).ToList();

                for (int i = 0; i < def.randomCorridoorAmount.RandomInRange; i++)
                {
                    if (nonCriticalRooms.Count < 2)
                        break;

                    DungeonRoom firstNode = nonCriticalRooms.RandomElement();
                    DungeonRoom secondNode = nonCriticalRooms.Where(x => x != firstNode).ToArray().RandomElement();
                    string connectionId = DungeonRoom.GetConnectionId(firstNode, secondNode);

                    if (!processedConnections.Contains(connectionId))
                    {
                        if (firstNode != null && secondNode != null)
                        {
                            RoomConnection connection = new RoomConnection(firstNode, secondNode);
                            firstNode.AddConnectionTo(map, secondNode);
                            secondNode.AddConnectionTo(map, firstNode);
                            connection.corridors = CorridoorUtility.GenerateCorridors(dungeon.Map, firstNode, secondNode);
                            connections.Add(connection);
                            processedConnections.Add(connectionId);
                        }
                    }
                }
            }

            return connections;
        }

        private void EnsureAllRoomsConnected()
        {
            // Check for any disconnected rooms
            foreach (var room in dungeon.GetAllRooms())
            {
                if (room.connectedRooms == null || room.connectedRooms.Count == 0)
                {
                    // Connect to a random room
                    var otherRoom = dungeon.GetAllRooms().Where(r => r != room).RandomElement();
                    room.connectedRooms = new List<DungeonRoom> { otherRoom };
                    otherRoom.connectedRooms.Add(room);
                }
            }
        }

        private void ApplyConnectionsToGrid(List<RoomConnection> connections)
        {
            Log.Message("Applying connections to dungeon grid");

            foreach (var connection in connections)
            {
                foreach (var corridor in connection.corridors)
                {
                    foreach (IntVec3 cell in corridor.path)
                    {
                        dungeon.MarkCellAsFloor(cell);
                    }
                }
            }
        }
        private void ApplyRoomsToGrid()
        {
            Log.Message("Applying room shapes to dungeon grid");

            foreach (var room in dungeon.GetAllRooms())
            {
                foreach (IntVec3 cell in room.roomCellRect)
                {
                    dungeon.MarkCellAsFloor(cell);
                }
            }
        }
        private void ClearWalls()
        {
            foreach (IntVec3 cell in dungeon.Map.AllCells)
            {
                if (dungeon.IsCellFloor(cell))
                {
                    dungeon.Map.thingGrid.ThingsAt(cell)
                        .ToList()
                        .ForEach(t => t.Destroy());

                    dungeon.Map.terrainGrid.SetTerrain(cell, TerrainDefOf.MetalTile);
                }
            }
        }
    }
}
