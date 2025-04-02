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
        public int randomCorridoorAmount = 2;
        public int maxDepth = 8;
        public int minRoomSize = 8;
        public int minRoomPadding = 3;

        // Lower room size factor to avoid rooms filling their entire BSP partition
        public float roomSizeFactor = 0.65f;

        public int minRooms = 5;
        public int maxRooms = 8;

        //size multiplier for more balanced divisions
        public float minSizeMultiplier = 1.2f;

        public float aspectRatioThreshold = 1.3f;
        //higher edge margin divisor for better spacing
        public float edgeMarginDivisor = 1.5f;

        public bool addRandomCorridoors = true;

        public List<RoomTypeDef> availableRoomTypes;

        public List<CelluarAutomataDef> earlyAutomata;
        public List<CelluarAutomataDef> postGenAutomata;
    }

    public class GenStep_BspDungeon : GenStep
    {
        GenStepDef_BspDungeon Def => (GenStepDef_BspDungeon)def;
        public override int SeedPart => 654321;

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

            if (Def.earlyAutomata != null)
            {
                Log.Message($"Applyng Early Celluar Automata iterations");
                CellularAutomataManager.ApplyRules(map, dungeonGrid, Def.earlyAutomata);
            }

            BspUtility.GenerateRooms(leafNodes, minPadding: Def.minRoomPadding, roomSizeFactor: Def.roomSizeFactor);

            var endpoints = BspUtility.FindFurthestPair(leafNodes);
            BspUtility.BspNode startNode = endpoints.Item1;
            BspUtility.BspNode endNode = endpoints.Item2;

            startNode.AddTag("start");
            endNode.AddTag("end");

            List<BspUtility.BspNode> waypoints = SelectWaypoints(leafNodes, startNode, endNode, 2);
            foreach (var waypoint in waypoints)
            {
                waypoint.AddTag("waypoint");
            }

            startNode.IsOnCriticalPath = true;
            startNode.CriticalPathIndex = 0;

            endNode.IsOnCriticalPath = true;
            endNode.CriticalPathIndex = waypoints.Count + 1;

            for (int i = 0; i < waypoints.Count; i++)
            {
                waypoints[i].IsOnCriticalPath = true;
                waypoints[i].CriticalPathIndex = i + 1;
                waypoints[i].IsWaypoint = true;
            }

            foreach (var node in leafNodes)
            {
                node.tag = new DungeonRoom();
            }

            List<BspUtility.BspNode> criticalPathNodes = new List<BspUtility.BspNode> { startNode };
            criticalPathNodes.AddRange(waypoints);
            criticalPathNodes.Add(endNode);

            for (int i = 0; i < criticalPathNodes.Count - 1; i++)
            {
                criticalPathNodes[i].connectedNodes.Add(criticalPathNodes[i + 1]);
                criticalPathNodes[i + 1].connectedNodes.Add(criticalPathNodes[i]);
            }


            MspUtility.CreateMinimumSpanningTree(leafNodes);

            ApplyRooms(map, leafNodes, dungeonGrid);

            List<RoomConnection> connections = BspUtility.GenerateRoomConnections(leafNodes);


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



            if (Def.postGenAutomata != null)
            {
                Log.Message($"Applyng PostGenerationg Celluar Automata iterations");
                CellularAutomataManager.ApplyRules(map, dungeonGrid, Def.postGenAutomata);
            }

            DesignateRoomTypes(map, leafNodes);
            ObstacleGenerator.GenerateObstacles(map, rootNode, leafNodes);
            DungeonUtil.SpawnDoorsForRoom(map, leafNodes);
           // map.fogGrid.Refog(CellRect.FromCellList(map.AllCells));      
        }

        private static List<BspUtility.BspNode> SelectWaypoints(List<BspUtility.BspNode> nodes, BspUtility.BspNode start, BspUtility.BspNode end, int count)
        {
            var possibleWaypoints = nodes.Where(n => n != start && n != end).ToList();

            var straightLineDir = (end.room.CenterCell.ToVector3() - start.room.CenterCell.ToVector3()).normalized;
            var startPos = start.room.CenterCell.ToVector3();

            possibleWaypoints.Sort((a, b) => {
                var aPos = a.room.CenterCell.ToVector3();
                var bPos = b.room.CenterCell.ToVector3();

                var aProj = Vector3.Dot((aPos - startPos), straightLineDir);
                var bProj = Vector3.Dot((bPos - startPos), straightLineDir);

                return -((aPos - (startPos + straightLineDir * aProj)).magnitude
                       .CompareTo((bPos - (startPos + straightLineDir * bProj)).magnitude));
            });

            return possibleWaypoints.Take(count).ToList();
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
                        node.tag.distanceFromStart = distance;
                    }
                }

                List<BspUtility.BspNode> sortedRooms = rooms
                    .Where(r => r != startNode && r != endNode)
                    .OrderBy(r => r.tag.distanceFromStart)
                    .ToList();

                List<RoomTypeDef> randomRooms = Def.availableRoomTypes
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
