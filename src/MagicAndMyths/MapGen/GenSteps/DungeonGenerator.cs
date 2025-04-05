using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DungeonGenerator
    {
        private Map map;
        private GenStepDef_BspDungeon def;
        private BspUtility.BspNode rootNode;
        private List<BspUtility.BspNode> leafNodes = new List<BspUtility.BspNode>();
        private BoolGrid dungeonGrid;
        private BspUtility.BspNode startNode;
        private BspUtility.BspNode endNode;
        private List<BspUtility.BspNode> waypoints = new List<BspUtility.BspNode>();

        public DungeonGenerator(Map map, GenStepDef_BspDungeon def)
        {
            this.map = map;
            this.def = def;
            this.dungeonGrid = new BoolGrid(map);
        }

        public void Generate()
        {
            // Phase 1: Initialization
            InitializeConnectionRules();

            FillMapWithWalls();

            // Phase 2: Structure Generation
            GenerateBspStructure();

            // Phase 3: Early Processing
            ApplyEarlyAutomata();

            // Phase 4: Path Creation
            CreateCriticalPath();

            // Phase 5: Room Assignment
            AssignRoomTypes();

            // Phase 6: Connection Creation
            CreateConnections();

            // Phase 7: Finalization
            ClearWalls();
            ApplyFinalTouches();
        }

        private void InitializeConnectionRules()
        {
            Log.Message("Initializing Connection Rules");

            RoomConnectionRuleManager.RegisterRule(new RoomConnectionRule
            {
                roomType = RoomType.Start,
                maxConnections = 1,
                minConnections = 1,
                priority = 100,
                avoidedConnectionTypes = new List<RoomType> { RoomType.End }
            });

            RoomConnectionRuleManager.RegisterRule(new RoomConnectionRule
            {
                roomType = RoomType.End,
                maxConnections = 1,
                minConnections = 1,
                priority = 100
            });
        }

        private void FillMapWithWalls()
        {
            Log.Message("Filling map with walls...");

            foreach (IntVec3 cell in map.AllCells)
            {
                GenSpawn.Spawn(MagicAndMythDefOf.DungeonWall, cell, map);
                map.terrainGrid.SetUnderTerrain(cell, TerrainDefOf.MetalTile);
            }
        }

        private void GenerateBspStructure()
        {
            Log.Message("Generating BSP tree");

            // Generate BSP tree
            CellRect mapArea = new CellRect(2, 2, map.Size.x - 4, map.Size.z - 4);
            rootNode = BspUtility.GenerateBspTreeWithRoomCount(
                mapArea,
                minRooms: def.minRooms,
                maxRooms: def.maxRooms,
                minRoomSize: def.minRoomSize,
                maxSplitAttempts: 200,
                minSizeMultiplier: def.minSizeMultiplier,
                aspectRatioThreshold: def.aspectRatioThreshold,
                edgeMarginDivisor: def.edgeMarginDivisor);

            // Extract leaf nodes
            BspUtility.GetLeafNodes(rootNode, leafNodes);

            // Generate rooms
            BspUtility.GenerateRooms(leafNodes,
                minPadding: def.minRoomPadding,
                roomSizeFactor: def.roomSizeFactor);
        }

        private void ApplyEarlyAutomata()
        {
            Log.Message("Applying Early Celluar Automata");

            if (def.earlyAutomata != null)
            {
                CellularAutomataManager.ApplyRules(map, dungeonGrid, def.earlyAutomata);
            }
        }

        private void CreateCriticalPath()
        {
            Log.Message("Generating Critical Path");

            // Find start and end rooms
            var endpoints = BspUtility.FindFurthestPair(leafNodes);
            startNode = endpoints.NodeOne;
            endNode = endpoints.NodeTwo;

            // Mark start room
            startNode.def = MagicAndMythDefOf.StartRoom;
            startNode.AddTag("start");
            startNode.IsOnCriticalPath = true;
            startNode.CriticalPathIndex = 0;

            // Mark end room
            endNode.def = MagicAndMythDefOf.EndRoom;
            endNode.AddTag("end");
            endNode.IsOnCriticalPath = true;

            // Find and mark waypoints
            waypoints = BspUtility.SelectWaypoints(leafNodes, startNode, endNode, 2);
            for (int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].AddTag("waypoint");
                waypoints[i].IsOnCriticalPath = true;
                waypoints[i].CriticalPathIndex = i + 1;
                waypoints[i].IsWaypoint = true;
            }

            endNode.CriticalPathIndex = waypoints.Count + 1;

            // Tag all rooms
            foreach (var node in leafNodes)
            {
                node.tag = new DungeonRoom();
            }

            // Connect critical path
            List<BspUtility.BspNode> criticalPathNodes = new List<BspUtility.BspNode> { startNode };
            criticalPathNodes.AddRange(waypoints);
            criticalPathNodes.Add(endNode);

            for (int i = 0; i < criticalPathNodes.Count - 1; i++)
            {
                criticalPathNodes[i].connectedNodes.Add(criticalPathNodes[i + 1]);
                criticalPathNodes[i + 1].connectedNodes.Add(criticalPathNodes[i]);
            }

            for (int i = 0; i < criticalPathNodes.Count - 1; i++)
            {
                var roomA = criticalPathNodes[i];
                var roomB = criticalPathNodes[i + 1];

                // Create a room connection and generate corridors
                var connection = new RoomConnection(roomA, roomB);
                connection.corridors = CorridoorUtility.GenerateCorridors(roomA, roomB);

                // Apply corridors directly to the grid
                foreach (var corridor in connection.corridors)
                {
                    foreach (IntVec3 cell in corridor.path)
                    {
                        if (cell.InBounds(map))
                        {
                            dungeonGrid[cell] = true;
                        }
                    }
                }
            }
        }

        private void AssignRoomTypes()
        {
            Log.Message("Assign Room types to leaf nodes...");
            foreach (var node in leafNodes)
            {
                if (node.HasTag("start"))
                {
                    node.def = MagicAndMythDefOf.StartRoom;
                }
                else if (node.HasTag("end"))
                {
                    node.def = MagicAndMythDefOf.EndRoom;
                }
                else
                {
                    // Assign normal room type
                    RoomTypeDef normalDef = def.availableRoomTypes
                        .Where(x => x.roomType == RoomType.Normal)
                        .RandomElement();
                    node.def = normalDef;
                }
            }
        }

        private void CreateConnections()
        {
            Log.Message("Generating Minimum Spanning Path");
            // Create base MST
            MspUtility.CreateMinimumSpanningTree(leafNodes);


            Log.Message("Applying room shapes to dungeon grid...");
            // Apply rooms to grid
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

            Log.Message("Generating connections based on rules...");
            List<RoomConnection> connections = BspUtility.GenerateRuleBasedConnections(leafNodes);


            //find any rooms with NO connections
            foreach (var item in leafNodes)
            {
                if (item.connectedNodes == null || item.connectedNodes.Count == 0)
                {

                    var otherNode = leafNodes.RandomElement();
                    item.connectedNodes = new List<BspUtility.BspNode>();

                    item.connectedNodes.Add(otherNode);
                }
            }

            Log.Message("Applying connections to dungeon grid...");
            foreach (var connection in connections)
            {
                foreach (var corridor in connection.corridors)
                {
                    foreach (IntVec3 cell in corridor.path)
                    {
                        if (cell.InBounds(map))
                        {
                            dungeonGrid[cell] = true;
                        }
                    }
                }
            }
        }

        private void ClearWalls()
        {
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
        }

        private void ApplyFinalTouches()
        {
            // Apply post-generation cellular automata
            if (def.postGenAutomata != null)
            {
                Log.Message($"Applying Post-Generation Cellular Automata iterations");
                CellularAutomataManager.ApplyRules(map, dungeonGrid, def.postGenAutomata);
            }

            // Apply room designations
            DesignateRoomTypes(map, leafNodes);

            // Apply room content
            foreach (var node in leafNodes)
            {
                if (node.def != null)
                {
                    node.def.DoWorker(map, node.room);
                }
            }

            // Generate obstacles
            ObstacleGenerator.GenerateObstacles(map, rootNode, leafNodes);

            // Spawn doors
            DungeonUtil.SpawnDoorsForRoom(map, leafNodes);
        }

        private void DesignateRoomTypes(Map map, List<BspUtility.BspNode> rooms)
        {
            if (rooms.Count < 4)
            {
                Log.Message("Not enough rooms to designate types");
                return;
            }


            foreach (var node in rooms)
            {
                if (node.tag == null)
                    node.tag = new DungeonRoom();
            }

            var furthestPair = BspUtility.FindFurthestPair(rooms);
            if (furthestPair.NodeOne != null && furthestPair.NodeTwo != null)
            {
                var startNode = furthestPair.NodeOne;
                var endNode = furthestPair.NodeTwo;

                MagicAndMythDefOf.StartRoom.DoWorker(map, startNode.room);
                MagicAndMythDefOf.EndRoom.DoWorker(map, endNode.room);

                foreach (var node in rooms)
                {
                    if (node != startNode && node != endNode)
                    {
                        float distance = Vector3.Distance(
                            node.room.CenterCell.ToVector3(),
                            startNode.room.CenterCell.ToVector3());
                        node.tag.distanceFromStart = distance;
                    }
                }

                List<BspUtility.BspNode> sortedRooms = rooms
                    .Where(r => r != startNode && r != endNode)
                    .OrderBy(r => r.tag.distanceFromStart)
                    .ToList();

                List<RoomTypeDef> randomRooms = def.availableRoomTypes
                    .Where(x => x.roomType == RoomType.Normal)
                    .ToList();

                foreach (var item in sortedRooms)
                {
                    RoomTypeDef roomTypeWorker = randomRooms.RandomElement();
                    roomTypeWorker.DoWorker(map, item.room);
                    item.def = roomTypeWorker;
                }
            }
        }
    }
}
