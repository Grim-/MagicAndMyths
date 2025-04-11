using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
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

            Log.Message("Creating Minimum spanning tree...");
            CreateMinimumSpanningTree();
            Log.Message("Creating room connections..");
            List<RoomConnection> connections = GenerateRoomConnections(map);
            EnsureAllRoomsConnected();

            Log.Message("Designating critical path"); 
            DesignateCriticalPath();
            ProcessSidePaths();


            Log.Message("Drawing connections");
            ApplyConnectionsToGrid(connections);
            Log.Message("Drawing rooms");
            ApplyRoomsToGrid();
            ClearWalls();

            if (def.postGenAutomata != null)
            {
                Log.Message("Applying Post-Generation Cellular Automata");
                CellularAutomataManager.ApplyRules(dungeon.Map, dungeon, def.postGenAutomata);
            }



            Log.Message("Applying Room workers to rooms");
            foreach (var room in dungeon.GetAllRooms())
            {
                if (room.def != null)
                {
                    room.def.DoWorker(dungeon.Map, room);
                }
            }

            Log.Message("Generating Obstacles");
            ObstacleGenerator.GenerateObstacles(dungeon.Map, dungeon);

            dungeon.Map.GetComponent<MapComp_ModifierManager>().AddModifier(new MapModifier_FoodDrain(dungeon.Map));
            dungeon.Map.GetComponent<MapComp_ModifierManager>().AddModifier(new MapModifier_Temperature(dungeon.Map));        
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
            Log.Message("Generating BSP tree structure with side paths");
            CellRect mapArea = new CellRect(2, 2, dungeon.Map.Size.x - 4, dungeon.Map.Size.z - 4);

            //BspNode rootNode = BspUtility.GenerateBspTreeWithSideRooms(
            //    mapArea,
            //    mainRoomCount: Math.Max(3,def.roomAmount.RandomInRange),
            //    sideRoomCount: def.sideRoomCount.RandomInRange, 
            //    minRoomSize: def.minRoomSize,
            //    maxSplitAttempts: 200,
            //    minSizeMultiplier: def.minSizeMultiplier,
            //    aspectRatioThreshold: def.aspectRatioThreshold,
            //    edgeMarginDivisor: def.edgeMarginDivisor);

            BspNode rootNode = BspUtility.GenerateBspTreeWithRoomCount(
                mapArea,
                minRooms : def.roomAmount.min,
                maxRooms : def.roomAmount.max,
                minRoomSize: def.minRoomSize,
                maxSplitAttempts: 200,
                aspectRatioThreshold: def.aspectRatioThreshold,
                edgeMarginDivisor: def.edgeMarginDivisor);

            List<BspNode> leafNodes = new List<BspNode>();
            BspUtility.GetLeafNodes(rootNode, leafNodes);

            // Track side path nodes separately
            foreach (var node in leafNodes)
            {
                if (node.HasTag("side_path"))
                {
                    dungeon.AddSidePathNode(node);
                }
            }

            dungeon.SetBspStructure(rootNode, leafNodes);

            BspUtility.GenerateRoomGeometry(dungeon.LeafNodes,
                minPadding: def.minRoomPadding,
                roomSizeFactor: def.roomSizeFactor);
        }
        private void ProcessSidePaths()
        {
            Log.Message("Processing side paths...");

            var criticalPathRooms = dungeon.GetAllRooms()
                .Where(r => r.IsOnCriticalPath)
                .OrderBy(r => r.CriticalPathIndex)
                .ToList();

            var sidePathNodes = dungeon.SidePathNodes;
            var sidePathRooms = new List<DungeonRoom>();

            foreach (var node in sidePathNodes)
            {
                DungeonRoom room = dungeon.GetRoom(node);
                if (room != null)
                {
                    room.AddTag("side_path");
                    sidePathRooms.Add(room);
                }
            }

            ConnectSidePathsToMainPath(criticalPathRooms, sidePathRooms);

            if (def.allowHiddenSidePaths && def.hiddenSidePathChance > 0)
            {
                HideRandomSidePaths(sidePathRooms, def.hiddenSidePathChance);
            }
        }

        private void ConnectSidePathsToMainPath(List<DungeonRoom> criticalPathRooms, List<DungeonRoom> sidePathRooms)
        {
            if (!sidePathRooms.Any() || !criticalPathRooms.Any())
                return;

            foreach (var sideRoom in sidePathRooms)
            {
                var orderedMainRooms = criticalPathRooms
                    .OrderBy(r => (r.Center - sideRoom.Center).LengthHorizontalSquared)
                    .ToList();

                DungeonRoom connectedMainRoom = null;

                foreach (var mainRoom in orderedMainRooms)
                {
                    // Connect the rooms
                    Dungeon.ConnectRooms(sideRoom, mainRoom);
                    sideRoom.AddConnectionTo(map, mainRoom);
                    mainRoom.AddConnectionTo(map, sideRoom);
                    connectedMainRoom = mainRoom;

                    break;
                }

                if (connectedMainRoom != null)
                {
                    PreventForwardConnections(sideRoom, connectedMainRoom, criticalPathRooms);
                }
            }
        }
        private void PreventForwardConnections(DungeonRoom sideRoom, DungeonRoom connectedMainRoom, List<DungeonRoom> criticalPathRooms)
        {
            var forwardRooms = criticalPathRooms
                .Where(r => r.CriticalPathIndex > connectedMainRoom.CriticalPathIndex)
                .ToList();

            foreach (var forwardRoom in forwardRooms)
            {
                if (sideRoom.IsConnectedTo(forwardRoom))
                {
                    sideRoom.connectedRooms.Remove(forwardRoom);
                    forwardRoom.connectedRooms.Remove(sideRoom);

                    var connection = sideRoom.connections.FirstOrDefault(c => c.DestinationRoom == forwardRoom);
                    if (connection != null)
                    {
                        sideRoom.connections.Remove(connection);
                    }

                    connection = forwardRoom.connections.FirstOrDefault(c => c.DestinationRoom == sideRoom);
                    if (connection != null)
                    {
                        forwardRoom.connections.Remove(connection);
                    }
                }
            }
        }
        private void HideRandomSidePaths(List<DungeonRoom> sidePathRooms, float hiddenChance)
        {
            foreach (var room in sidePathRooms)
            {
                if (Rand.Value < hiddenChance)
                {
                    dungeon.MarkRoomAsHidden(room);
                }
            }
        }

        //this is shit
        private void ProcessPlannedRooms(List<RoomTypeDef> plannedRooms)
        {
            if (plannedRooms == null || plannedRooms.Count == 0)
                return;

            dungeon.LeafNodes.Sort((a, b) =>
                (b.rect.Width * b.rect.Height).CompareTo(a.rect.Width * a.rect.Height));

            HashSet<BspNode> assignedNodes = new HashSet<BspNode>();

            foreach (var roomType in plannedRooms)
            {
                if (roomType.minSize == IntVec2.Invalid)
                    continue;

                BspNode bestNode = null;
                foreach (var node in dungeon.LeafNodes)
                {
                    if (assignedNodes.Contains(node))
                        continue;

                    // Check if node is large enough with padding)
                    if (node.rect.Width >= roomType.minSize.x + (def.minRoomPadding * 2) &&
                        node.rect.Height >= roomType.minSize.z + (def.minRoomPadding * 2))
                    {
                        bestNode = node;
                        break;
                    }
                }

                if (bestNode != null)
                {
                    bestNode.roomRect = bestNode.GenerateRoomGeometryWithSize(
                        roomType.minSize.x, roomType.minSize.z, def.minRoomPadding);

                    DungeonRoom room = DungeonRoom.FromBspNode(dungeon, bestNode);
                    room.def = roomType;
                    dungeon.AddRoom(bestNode, room);
                    assignedNodes.Add(bestNode);
                }
            }

            foreach (var node in dungeon.LeafNodes)
            {
                if (assignedNodes.Contains(node) || dungeon.HasMapping(node))
                    continue;

                DungeonRoom room = DungeonRoom.FromBspNode(dungeon, node);
                var normalRoomTypes = plannedRooms.Where(x => x.roomType == RoomType.Normal).ToList();
                if (normalRoomTypes.Any())
                {
                    room.def = normalRoomTypes.RandomElement();
                }

                dungeon.AddRoom(node, room);
            }
        }

        private void CreateRoomsFromBspNodes()
        {
            Log.Message("Creating dungeon rooms from BSP nodes");

            foreach (var node in dungeon.LeafNodes)
            {
                if (dungeon.HasMapping(node))
                    continue;

                DungeonRoom room = DungeonRoom.FromBspNode(dungeon, node);
                dungeon.AddRoom(node, room);
            }
        }

        private void ApplyEarlyAutomata()
        {
            Log.Message("Applying Early Cellular Automata");

            if (def.earlyAutomata != null)
            {
                CellularAutomataManager.ApplyRules(dungeon.Map, dungeon, def.earlyAutomata);
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
