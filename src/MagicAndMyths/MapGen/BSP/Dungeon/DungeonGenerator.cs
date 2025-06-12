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
        private DungeonGenDef parentGenStep;
        private Map map;

        private int MapMargin = 4;
        public DungeonGenerator(Map map, DungeonGenDef def)
        {
            this.parentGenStep = def;
            this.map = map;
            this.dungeon = new Dungeon(map);
            this.dungeon.Def = def;
            if (map.Parent is DungeonMapParent dungeonMapParent)
            {
                dungeonMapParent.SetDungeon(this.dungeon);
            }
        }

        public void Generate()
        {
            // Phase 1: Initialize the dungeon
            FillMapWithWalls();

            // Phase 2: Generate BSP structure
            GenerateBspStructure();

            ProcessPlannedRooms(parentGenStep.availableRoomTypes);

            // Phase 3: Create rooms from BSP nodes
            CreateRoomsFromBspNodes();
            //ProtectImportantAreas();

            // Phase 4: Apply early processing
            ApplyEarlyAutomata();

            Log.Message("Creating Minimum spanning tree...");
            CreateMinimumSpanningTree();

            Log.Message("Creating room connections..");
            dungeon.ConnectionManager.GenerateConnectionsFromRoomGraph();

            Log.Message("Designating critical path");
            DesignateCriticalPath();

            Log.Message("Processing side paths...");
            ProcessSidePaths();
            EnsureAllRoomsConnected();
            Log.Message("Assigning room types");
            AssignRoomTypes();
            Log.Message("Drawing connections");
            dungeon.ConnectionManager.ApplyConnectionsToGrid();
            Log.Message("Drawing rooms");
            ApplyRoomsToGrid();
            ClearWalls();
            //ProtectImportantAreas();

            if (parentGenStep.postGenAutomata != null)
            {
                Log.Message("Applying Post-Generation Cellular Automata");
                CellularAutomataManager.ApplyRules(dungeon.Map, dungeon, parentGenStep.postGenAutomata);
            }

            ApplyRoomWorkers();
        }

        protected void ApplyRoomWorkers()
        {
            Log.Message("Applying Room workers to rooms");
            foreach (var room in dungeon.GetAllRooms())
            {
                if (room.def != null)
                {
                    room.def.DoWorker(dungeon.Map, dungeon, room);
                }
            }
        }

        public void ProtectImportantAreas()
        {
            // Protect all room walls
            foreach (var room in GeneratedDungeon.Rooms)
            {
                GeneratedDungeon.MarkCellsProtected(room.roomWalls.ExpandedBy(1).Cells, true);
            }

            // Protect all corridor/path cells
            foreach (var connection in GeneratedDungeon.ConnectionManager.AllConnections)
            {
                foreach (var item in connection.corridors)
                {
                    GeneratedDungeon.MarkCellsProtected(item.CellRect.ExpandedBy(1), true);
                }
            }

            // Protect room entrance cells
            foreach (var room in GeneratedDungeon.Rooms)
            {
                var entranceCells = room.EntranceCells().ToList();
                foreach (var entranceCell in entranceCells)
                {
                    // Protect wall cells adjacent to entrance cells
                    foreach (var adjacentCell in GenAdjFast.AdjacentCellsCardinal(entranceCell))
                    {
                        if (adjacentCell.InBounds(map) && room.roomWalls.Contains(adjacentCell) && adjacentCell.GetFirstBuilding(map) != null)
                        {
                            GeneratedDungeon.MarkCellProtected(adjacentCell, true);
                        }
                    }
                }
            }
        }
        private void FillMapWithWalls()
        {
            //Log.Message("Filling map with walls...");

            foreach (IntVec3 cell in dungeon.Map.AllCells)
            {

                Thing thing = ThingMaker.MakeThing(this.parentGenStep.WallDef, this.parentGenStep.WallStuffDef != null ? this.parentGenStep.WallStuffDef : GenStuff.DefaultStuffFor(this.parentGenStep.WallDef));
                GenSpawn.Spawn(thing, cell, dungeon.Map);
                dungeon.Map.terrainGrid.SetUnderTerrain(cell, this.parentGenStep.TerrainDef);
            }
        }



        private void GenerateBspStructure()
        {
            //Log.Message("Generating BSP tree structure with side paths");
            CellRect mapArea = new CellRect(MapMargin / 2, MapMargin /2, dungeon.Map.Size.x - MapMargin, dungeon.Map.Size.z - MapMargin);

            int mainRoomCount = parentGenStep.roomAmount.RandomInRange;
            int sideRoomcount = parentGenStep.sideRoomCount.RandomInRange;

            int minRoomsRequired = mainRoomCount + sideRoomcount;

            BspNode rootNode = BspUtility.GenerateBspTreeWithSideRooms(
                mapArea,
                totalRoomCount: minRoomsRequired,
                mainRoomCount: mainRoomCount,
                sideRoomCount: sideRoomcount,
                minRoomSize: parentGenStep.minRoomSize,
                maxSplitAttempts: 200,
                aspectRatioThreshold: parentGenStep.aspectRatioThreshold,
                edgeMarginDivisor: 4f);

            List<BspNode> leafNodes = new List<BspNode>();
            BspUtility.GetLeafNodes(rootNode, leafNodes);

            foreach (var node in leafNodes)
            {
                if (node.HasTag("side_path"))
                {
                    dungeon.AddSidePathNode(node);
                }
            }

            dungeon.SetBspStructure(rootNode, leafNodes);

            BspUtility.GenerateRoomGeometry(dungeon.LeafNodes,
                minPadding: parentGenStep.minRoomPadding,
                roomSizeFactor: parentGenStep.roomSizeFactor.RandomInRange);
        }

        private void ProcessSidePaths()
        {
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

            if (sidePathRooms.Count == 0) 
                return;

            var sidePathChains = CreateSidePathChains(sidePathRooms, criticalPathRooms);
            ConnectSidePathChainsToMainPath(criticalPathRooms, sidePathChains);

            if (parentGenStep.allowHiddenSidePaths && parentGenStep.hiddenSidePathChance > 0)
            {
                HideRandomSidePathChains(sidePathChains, parentGenStep.hiddenSidePathChance);
            }
        }

        private List<List<DungeonRoom>> CreateSidePathChains(List<DungeonRoom> sidePathRooms, List<DungeonRoom> criticalPathRooms)
        {
            var chains = new List<List<DungeonRoom>>();
            var usedRooms = new HashSet<DungeonRoom>();

            while (sidePathRooms.Any(r => !usedRooms.Contains(r)))
            {
                var availableRooms = sidePathRooms.Where(r => !usedRooms.Contains(r)).ToList();
                var chainLength = DetermineChainLength();
                var chain = BuildSidePathChain(availableRooms, chainLength, usedRooms, criticalPathRooms);

                if (chain.Count > 0)
                {
                    chains.Add(chain);
                    foreach (var room in chain)
                    {
                        usedRooms.Add(room);
                    }
                }
                else
                {
                    break;
                }
            }

            return chains;
        }

        private int DetermineChainLength()
        {
            if (Rand.Value < parentGenStep.longSidePathChance)
            {
                return parentGenStep.sidePathLength.RandomInRange;
            }
            return 1;
        }

        private List<DungeonRoom> BuildSidePathChain(List<DungeonRoom> availableRooms, int targetLength,
            HashSet<DungeonRoom> usedRooms, List<DungeonRoom> criticalPathRooms)
        {
            var chain = new List<DungeonRoom>();

            if (availableRooms.Count == 0) return chain;

            var startRoom = availableRooms.RandomElement();
            chain.Add(startRoom);
            var currentRoom = startRoom;

            for (int i = 1; i < targetLength && chain.Count < availableRooms.Count; i++)
            {
                var nextRoom = SelectNextRoomInChain(currentRoom, availableRooms, usedRooms, chain, criticalPathRooms);
                if (nextRoom == null) break;

                dungeon.ConnectRooms(currentRoom, nextRoom);
                chain.Add(nextRoom);
                currentRoom = nextRoom;

                if (parentGenStep.allowBranchingSidePaths && Rand.Chance(parentGenStep.branchingChance))
                {
                    CreateSidePathBranch(currentRoom, availableRooms, usedRooms, chain, criticalPathRooms);
                }
            }

            return chain;
        }

        private DungeonRoom SelectNextRoomInChain(DungeonRoom currentRoom, List<DungeonRoom> availableRooms,
            HashSet<DungeonRoom> usedRooms, List<DungeonRoom> chainSoFar, List<DungeonRoom> criticalPathRooms)
        {
            var candidates = availableRooms
                .Where(r => !usedRooms.Contains(r) && !chainSoFar.Contains(r))
                .ToList();

            if (candidates.Count == 0) return null;

            if (Rand.Value < parentGenStep.meanderingChance)
            {
                return candidates.RandomElement();
            }
            else
            {
                return candidates
                    .OrderBy(r => (r.Center - currentRoom.Center).LengthHorizontalSquared)
                    .FirstOrDefault();
            }
        }
        private void CreateSidePathBranch(DungeonRoom branchPoint, List<DungeonRoom> availableRooms,
            HashSet<DungeonRoom> usedRooms, List<DungeonRoom> mainChain, List<DungeonRoom> criticalPathRooms)
        {
            var branchLength = Rand.Range(1, Math.Max(2, parentGenStep.sidePathLength.max / 2));
            var branch = BuildSidePathChain(availableRooms, branchLength, usedRooms, criticalPathRooms);

            if (branch.Count > 0)
            {
                dungeon.ConnectRooms(branchPoint, branch[0]);
                foreach (var room in branch)
                {
                    usedRooms.Add(room);
                    room.AddTag("side_path_branch");
                }
            }
        }

        private void ConnectSidePathChainsToMainPath(List<DungeonRoom> criticalPathRooms, List<List<DungeonRoom>> chains)
        {
            if (!chains.Any() || !criticalPathRooms.Any()) return;

            foreach (var chain in chains)
            {
                if (chain.Count == 0) continue;

                var chainEntrance = SelectChainEntrance(chain, criticalPathRooms);
                var nearestMainRoom = criticalPathRooms
                    .OrderBy(r => (r.Center - chainEntrance.Center).LengthHorizontalSquared)
                    .First();

                dungeon.ConnectRooms(chainEntrance, nearestMainRoom);

                PreventForwardConnectionsForChain(chain, nearestMainRoom, criticalPathRooms);
            }
        }

        private DungeonRoom SelectChainEntrance(List<DungeonRoom> chain, List<DungeonRoom> criticalPathRooms)
        {
            return chain
                .OrderBy(r => criticalPathRooms.Min(cr => (cr.Center - r.Center).LengthHorizontalSquared))
                .First();
        }

        private void PreventForwardConnectionsForChain(List<DungeonRoom> chain, DungeonRoom connectedMainRoom,
            List<DungeonRoom> criticalPathRooms)
        {
            var forwardRooms = criticalPathRooms
                .Where(r => r.CriticalPathIndex > connectedMainRoom.CriticalPathIndex)
                .ToList();

            foreach (var chainRoom in chain)
            {
                foreach (var forwardRoom in forwardRooms)
                {
                    if (chainRoom.IsConnectedTo(forwardRoom))
                    {
                        chainRoom.connectedRooms.Remove(forwardRoom);
                        forwardRoom.connectedRooms.Remove(chainRoom);

                        var connection = chainRoom.connections.FirstOrDefault(c => c.DestinationRoom == forwardRoom);
                        if (connection != null)
                        {
                            chainRoom.connections.Remove(connection);
                        }

                        connection = forwardRoom.connections.FirstOrDefault(c => c.DestinationRoom == chainRoom);
                        if (connection != null)
                        {
                            forwardRoom.connections.Remove(connection);
                        }
                    }
                }
            }
        }

        private void HideRandomSidePathChains(List<List<DungeonRoom>> chains, float hiddenChance)
        {
            foreach (var chain in chains)
            {
                if (Rand.Value < hiddenChance)
                {
                    foreach (var room in chain)
                    {
                        dungeon.MarkRoomAsHidden(room);
                    }
                }
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
                    dungeon.ConnectRooms(sideRoom, mainRoom);
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
                    if (node.rect.Width >= roomType.minSize.x + (parentGenStep.minRoomPadding * 2) &&
                        node.rect.Height >= roomType.minSize.z + (parentGenStep.minRoomPadding * 2))
                    {
                        bestNode = node;
                        break;
                    }
                }

                if (bestNode != null)
                {
                    bestNode.roomRect = bestNode.GenerateRoomGeometryWithSize(
                        roomType.minSize.x, roomType.minSize.z, parentGenStep.minRoomPadding);

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
                dungeon.AddRoom(node, room);
            }
        }

        private void CreateRoomsFromBspNodes()
        {
            // Log.Message("Creating dungeon rooms from BSP nodes");

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
            //Log.Message("Applying Early Cellular Automata");

            if (parentGenStep.earlyAutomata != null)
            {
                CellularAutomataManager.ApplyRules(dungeon.Map, dungeon, parentGenStep.earlyAutomata);
            }
        }

        private void AssignRoomTypes()
        {
            foreach (var node in dungeon.LeafNodes)
            {
                DungeonRoom room = dungeon.GetRoom(node);
                if (room.def != null)
                    continue;

                if (room.IsOnCriticalPath)
                {
                    if (Rand.Chance(parentGenStep.noRoomChanceCriticalPath))
                    {
                        var criticalRooms = parentGenStep.availableRoomTypes
                        .Where(x => x.roomType == RoomType.Normal && x.roomIsOnCriticalPath == true)
                        .ToList();

                        if (criticalRooms.Count > 0)
                        {
                            room.def = criticalRooms.RandomElement();
                        }
                        else
                        {
                            Log.Error("No critical path room types found!");
                        }
                    }
                }
                else
                {
                    if (Rand.Chance(parentGenStep.noRoomChanceSidePath))
                    {

                        var sideRooms = parentGenStep.availableSideRoomTypes
                            .Where(x => x.roomType == RoomType.Normal && x.roomIsOnCriticalPath == false)
                            .ToList();

                        if (sideRooms.Count > 0)
                        {
                            room.def = sideRooms.RandomElement();
                        }
                        else
                        {
                            Log.Error("No side path room types found!");
                        }
                    }

                }

                if (dungeon.StartNode != null)
                {
                    float distance = Vector3.Distance(
                        room.Center.ToVector3(),
                        dungeon.GetRoom(dungeon.StartNode).Center.ToVector3());
                    room.distanceFromStart = distance;
                }
            }
        }

        private void DesignateCriticalPath()
        {
            // Log.Message("Designating Critical Path Using Graph Distance");

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
                dungeon.ConnectRooms(a, b);
            }
        }

        private void CreateMinimumSpanningTree()
        {
            //Log.Message("Generating Minimum Spanning Tree");

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
            List<RoomConnection> connections = new List<RoomConnection>();
            HashSet<string> processedConnections = new HashSet<string>();

            foreach (var room in dungeon.GetAllRooms())
            {
                foreach (var connectedRoom in room.connectedRooms)
                {
                    string connectionId = DungeonRoom.GetConnectionId(room, connectedRoom);

                    if (!processedConnections.Contains(connectionId))
                    {
                        RoomConnection connection = new RoomConnection(room, connectedRoom);

                        //// Choose corridor style based on room properties
                        //CorridorPathBase pathGenerator = ChooseCorridorStyle(room, connectedRoom);

                        //// Set width based on room importance
                        //int corridorWidth = DetermineCorridorWidth(room, connectedRoom);

                        // Generate with enhanced utility
                        connection.corridors = CorridoorUtility.GenerateCorridors(
                            map, room, connectedRoom);

                        connections.Add(connection);
                        processedConnections.Add(connectionId);
                    }
                }
            }

            // Add random corridors with variety
            if (parentGenStep.addRandomCorridoors)
            {
                //CorridoorUtility.AddRandomCorridorsWithVariety(connections, processedConnections);
            }

            return connections;
        }


        private void EnsureAllRoomsConnected()
        {
            foreach (var room in dungeon.GetAllRooms())
            {
                if (room.connectedRooms == null || room.connectedRooms.Count == 0)
                {
                    var otherRoom = dungeon.GetAllRooms().Where(r => r != room && !r.IsOnCriticalPath).RandomElement();
                    room.connectedRooms = new List<DungeonRoom> { otherRoom };
                    otherRoom.connectedRooms.Add(room);
                }
            }
        }

        private void ApplyConnectionsToGrid(List<RoomConnection> connections)
        {
            // Log.Message("Applying connections to dungeon grid");

            foreach (var connection in connections)
            {
                foreach (var corridor in connection.corridors)
                {
                    foreach (IntVec3 cell in corridor.CellRect.Cells)
                    {
                        dungeon.MarkCellAsFloor(cell);
                    }
                }
            }
        }
        private void ApplyRoomsToGrid()
        {
            // Log.Message("Applying room shapes to dungeon grid");
            FillMapWithWalls();

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

                    dungeon.Map.terrainGrid.SetTerrain(cell, parentGenStep.TerrainDef);
                }
            }
        }
    }
}