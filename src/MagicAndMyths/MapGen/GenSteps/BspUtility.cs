using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class RoomConnection
    {
        public BspUtility.BspNode roomA;
        public BspUtility.BspNode roomB;
        public List<Corridoor> corridors;
        public bool needsDoor = false;
        public bool isLocked = false;
        public IntVec3 doorPosition = IntVec3.Invalid;

        public RoomConnection(BspUtility.BspNode a, BspUtility.BspNode b)
        {
            roomA = a;
            roomB = b;
            corridors = new List<Corridoor>();
        }
    }

    public static class BspUtility
    {
        public class BspNode
        {
            public CellRect rect;
            public BspNode left;
            public BspNode right;
            public CellRect room;
            public CellRect roomWalls;
            public DungeonRoom tag;
            public List<string> tags = new List<string>();
            public List<BspNode> connectedNodes = new List<BspNode>();
            public RoomTypeDef def;

            //TODO 
            public void AddTag(string tag)
            {
                if (tags == null)
                {
                    tags = new List<string>();
                }

                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                }
            }

            public bool HasTag(string tag)
            {
                return tags != null && tags.Contains(tag);
            }

            public bool IsOnCriticalPath { get; set; } = false;
            public int CriticalPathIndex { get; set; } = -1;
            public bool IsWaypoint { get; set; } = false;
        }

        public static BspNode GenerateBspTree(CellRect rootRect, int maxDepth, int minRoomSize = 8)
        {
            BspNode rootNode = new BspNode { rect = rootRect };
            SplitNode(rootNode, 0, maxDepth, minRoomSize);
            return rootNode;
        }

        public static BspNode GenerateBspTreeWithRoomCount(CellRect rootRect, int minRooms, int maxRooms, int minRoomSize = 8, int maxSplitAttempts = 100, float minSizeMultiplier = 1.0f, float aspectRatioThreshold = 1.2f, float edgeMarginDivisor = 2f)
        {
            int initialMaxDepth = (int)Math.Ceiling(Math.Log(maxRooms, 2)) + 1;

            BspNode rootNode = new BspNode { rect = rootRect };
            SplitNode(rootNode, 0, initialMaxDepth, minRoomSize,
                     minSizeMultiplier, aspectRatioThreshold, edgeMarginDivisor);

            List<BspNode> leafNodes = new List<BspNode>();
            GetLeafNodes(rootNode, leafNodes);

            Log.Message($"BSP generated {leafNodes.Count} potential rooms, target: {minRooms}-{maxRooms}");

            int attempts = 0;
            while (leafNodes.Count < minRooms && attempts < maxSplitAttempts)
            {
                // Find the largest leaf node
                BspNode largestNode = null;
                int largestArea = 0;

                foreach (var node in leafNodes)
                {
                    int area = node.rect.Width * node.rect.Height;
                    if (area > largestArea &&
                        node.rect.Width >= minRoomSize * minSizeMultiplier &&
                        node.rect.Height >= minRoomSize * minSizeMultiplier)
                    {
                        largestArea = area;
                        largestNode = node;
                    }
                }

                if (largestNode != null)
                {
                    SplitNode(largestNode, 0, 1, minRoomSize,
                             minSizeMultiplier, aspectRatioThreshold, edgeMarginDivisor);

                    leafNodes.Clear();
                    GetLeafNodes(rootNode, leafNodes);
                    Log.Message($"After split attempt {attempts + 1}: now {leafNodes.Count} rooms");
                }
                else
                {
                    break;
                }

                attempts++;
            }

            if (leafNodes.Count > maxRooms)
            {
                Log.Message($"Too many rooms ({leafNodes.Count}), pruning to {maxRooms}");
                leafNodes.Shuffle();
                var nodesToKeep = leafNodes.Take(maxRooms).ToList();

                foreach (var node in nodesToKeep)
                {
                    node.AddTag("keep");
                }

                //prune the BSP tree
                PruneNonMarkedLeafNodes(rootNode);

                leafNodes.Clear();
                GetLeafNodes(rootNode, leafNodes);
                Log.Message($"After pruning: {leafNodes.Count} rooms");
            }

            return rootNode;
        }



        public static List<RoomConnection> GenerateRoomConnections(List<BspUtility.BspNode> leafNodes)
        {
            List<RoomConnection> connections = new List<RoomConnection>();
            HashSet<string> processedConnections = new HashSet<string>();

            foreach (var node in leafNodes)
            {
                foreach (var connectedNode in node.connectedNodes)
                {
                    string connectionId = GetConnectionId(node, connectedNode);

                    if (!processedConnections.Contains(connectionId))
                    {
                        RoomConnection connection = new RoomConnection(node, connectedNode);
                        connection.corridors = CorridoorUtility.GenerateCorridors(node, connectedNode);
                        DetermineIfConnectionNeedsDoor(connection, leafNodes);
                        connections.Add(connection);
                        processedConnections.Add(connectionId);
                    }
                }
            }

            return connections;
        }

        private static void DetermineIfConnectionNeedsDoor(RoomConnection connection, List<BspUtility.BspNode> allRooms)
        {
            if (connection.roomA.IsOnCriticalPath && connection.roomB.IsOnCriticalPath)
            {
                connection.needsDoor = true;

                if (Mathf.Abs(connection.roomA.CriticalPathIndex - connection.roomB.CriticalPathIndex) == 1)
                {
                    int lowerIndex = Mathf.Min(connection.roomA.CriticalPathIndex, connection.roomB.CriticalPathIndex);
                    if (lowerIndex > 0)
                    {
                        connection.isLocked = true;
                    }
                }
                return;
            }

            RoomTypeDef roomADef = GetRoomTypeDef(connection.roomA);
            RoomTypeDef roomBDef = GetRoomTypeDef(connection.roomB);

            if ((roomADef != null && IsSpecialRoomDef(roomADef)) ||
                (roomBDef != null && IsSpecialRoomDef(roomBDef)))
            {
                connection.needsDoor = true;
                return;
            }

            if (Rand.Value < 0.6f)
            {
                connection.needsDoor = true;
            }
        }

        private static RoomTypeDef GetRoomTypeDef(BspUtility.BspNode node)
        {
            if (node.tag is DungeonRoom dr)
            {
                return DefDatabase<RoomTypeDef>.AllDefs
                    .FirstOrDefault(def => def.roomType == dr.type);
            }
            return null;
        }

        private static bool IsSpecialRoomDef(RoomTypeDef def)
        {
            return def?.roomType == RoomType.Start || def?.roomType == RoomType.End;
        }

        public static void PlaceDoors(Map map, List<BspUtility.BspNode> rooms)
        {
            foreach (var room in rooms)
            {
                // Skip rooms without walls
                if (room.roomWalls == null || !room.roomWalls.EdgeCells.Any())
                    continue;

                // Get all potential door locations on room walls
                List<IntVec3> doorCandidates = new List<IntVec3>();

                // Make a copy of edge cells to avoid enumeration issues
                List<IntVec3> edgeCells = room.roomWalls.EdgeCells.ToList();

                foreach (IntVec3 cell in edgeCells)
                {
                    if (IsGoodDoorLocation(map, cell))
                    {
                        doorCandidates.Add(cell);
                    }
                }

                // If we found candidates, select the best ones
                if (doorCandidates.Count > 0)
                {
                    // Sort candidates by quality score
                    doorCandidates = doorCandidates
                        .OrderByDescending(c => ScoreDoorLocation(map, c))
                        .ToList();

                    // Calculate how many doors to place
                    int doorsToPlace = CalculateDoorsForRoom(room, doorCandidates.Count);

                    // Place doors, ensuring good spacing
                    List<IntVec3> placedDoorPositions = new List<IntVec3>();
                    foreach (IntVec3 candidate in doorCandidates)
                    {
                        // Skip if too close to an existing door
                        if (IsTooCloseToExistingDoors(candidate, placedDoorPositions))
                            continue;

                        // Place the door
                        Thing existing = candidate.GetFirstBuilding(map);
                        if (existing != null) existing.Destroy();

                        Building_Door door = (Building_Door)GenSpawn.Spawn(ThingDefOf.Door, candidate, map);

                        // Lock doors on critical path (except first door)
                        if (room.IsOnCriticalPath && room.CriticalPathIndex > 0)
                        {
                            door.SetForbidden(true);
                        }
                        else
                        {
                            door.SetFaction(Faction.OfAncientsHostile);
                        }

                        placedDoorPositions.Add(candidate);

                        // Stop if we've placed enough doors
                        if (placedDoorPositions.Count >= doorsToPlace)
                            break;
                    }
                }
            }
        }

        private static bool IsGoodDoorLocation(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map)) return false;

            // Check if there's a wall here
            Building wall = cell.GetFirstBuilding(map);
            if (wall == null || wall.def != MagicAndMythDefOf.DungeonWall)
                return false;

            // Check orientation (doors need support on opposite sides)
            IntVec3 north = new IntVec3(cell.x, cell.y, cell.z + 1);
            IntVec3 south = new IntVec3(cell.x, cell.y, cell.z - 1);
            IntVec3 east = new IntVec3(cell.x + 1, cell.y, cell.z);
            IntVec3 west = new IntVec3(cell.x - 1, cell.y, cell.z);

            bool canPlaceHorizontal = IsWall(map, east) && IsWall(map, west);
            bool canPlaceVertical = IsWall(map, north) && IsWall(map, south);

            if (!canPlaceHorizontal && !canPlaceVertical)
                return false;

            // Check if there's open space on BOTH sides of the door
            bool hasValidPath = false;

            if (canPlaceHorizontal)
            {
                bool northIsPassable = IsPassable(map, north);
                bool southIsPassable = IsPassable(map, south);

                // For a door to be useful, you need access from BOTH sides
                if (northIsPassable && southIsPassable)
                {
                    // Check if there's enough room on both sides
                    int northOpenSpace = CountAdjacentOpenSpace(map, north);
                    int southOpenSpace = CountAdjacentOpenSpace(map, south);

                    if (northOpenSpace >= 2 && southOpenSpace >= 2)
                        hasValidPath = true;
                }
            }

            if (canPlaceVertical && !hasValidPath)
            {
                bool eastIsPassable = IsPassable(map, east);
                bool westIsPassable = IsPassable(map, west);

                // For a door to be useful, you need access from BOTH sides
                if (eastIsPassable && westIsPassable)
                {
                    // Check if there's enough room on both sides
                    int eastOpenSpace = CountAdjacentOpenSpace(map, east);
                    int westOpenSpace = CountAdjacentOpenSpace(map, west);

                    if (eastOpenSpace >= 2 && westOpenSpace >= 2)
                        hasValidPath = true;
                }
            }

            return hasValidPath;
        }
        private static int CountAdjacentOpenSpace(Map map, IntVec3 cell)
        {
            int count = 0;
            foreach (IntVec3 adj in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (adj.InBounds(map) && IsPassable(map, adj))
                    count++;
            }
            return count;
        }

        private static int ScoreDoorLocation(Map map, IntVec3 cell)
        {
            int score = 0;

            // Check orientation
            IntVec3 north = new IntVec3(cell.x, cell.y, cell.z + 1);
            IntVec3 south = new IntVec3(cell.x, cell.y, cell.z - 1);
            IntVec3 east = new IntVec3(cell.x + 1, cell.y, cell.z);
            IntVec3 west = new IntVec3(cell.x - 1, cell.y, cell.z);

            // Check if door has wall support
            bool hasHorizontalSupport = IsWall(map, east) && IsWall(map, west);
            bool hasVerticalSupport = IsWall(map, north) && IsWall(map, south);

            if (hasHorizontalSupport) score += 20;
            if (hasVerticalSupport) score += 20;

            // Count open adjacent cells - more open space is better
            int openCount = 0;
            if (IsPassable(map, north)) openCount++;
            if (IsPassable(map, south)) openCount++;
            if (IsPassable(map, east)) openCount++;
            if (IsPassable(map, west)) openCount++;

            score += 10 * openCount;

            // Penalize corner doors
            int adjacentWalls = 0;
            foreach (IntVec3 adj in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (IsWall(map, adj))
                    adjacentWalls++;
            }

            if (adjacentWalls > 2)
                score -= 30; // Heavily penalize corners

            // Bonus for being away from edges (prevents doors on outer map boundaries)
            IntVec3 mapSize = map.Size;
            if (cell.x > 5 && cell.x < mapSize.x - 5) score += 10;
            if (cell.z > 5 && cell.z < mapSize.z - 5) score += 10;

            return score;
        }

        private static int CalculateDoorsForRoom(BspUtility.BspNode room, int candidateCount)
        {
            // Calculate based on room size and importance
            int baseDoorsCount = 1; // At least one door

            // More doors for larger rooms
            int roomArea = room.room.Width * room.room.Height;
            if (roomArea > 150) baseDoorsCount++;
            if (roomArea > 300) baseDoorsCount++;

            // Critical path nodes need at least one door
            if (room.IsOnCriticalPath)
                baseDoorsCount = Mathf.Max(baseDoorsCount, 1);

            // Cap at available candidates
            return Mathf.Min(baseDoorsCount, candidateCount);
        }

        private static bool IsTooCloseToExistingDoors(IntVec3 candidate, List<IntVec3> existingDoors)
        {
            // Don't place doors too close to each other
            const int MIN_DOOR_SPACING = 3;

            foreach (IntVec3 existingDoor in existingDoors)
            {
                float distance = (candidate - existingDoor).LengthHorizontal;
                if (distance < MIN_DOOR_SPACING)
                    return true;
            }

            return false;
        }

        private static bool IsWall(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map))
                return false;

            Building building = cell.GetFirstBuilding(map);
            return building != null && building.def == MagicAndMythDefOf.DungeonWall;
        }

        private static bool IsPassable(Map map, IntVec3 cell)
        {
            if (!cell.InBounds(map))
                return false;

            Building building = cell.GetFirstBuilding(map);
            if (building == null)
                return true;

            if (building.def != MagicAndMythDefOf.DungeonWall && !building.def.passability.Equals(Traversability.Impassable))
            {
                return true;
            }

            return false;
        }
    
        private static string GetConnectionId(BspUtility.BspNode node1, BspUtility.BspNode node2)
        {
            ulong id1 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node1);
            ulong id2 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node2);
            return id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";
        }
    


    // New method to prune the BSP tree
        private static bool PruneNonMarkedLeafNodes(BspNode node)
        {
            if (node == null) return false;

            if (node.left == null && node.right == null)
            {
                return node.HasTag("keep");
            }


            bool keepLeft = PruneNonMarkedLeafNodes(node.left);
            bool keepRight = PruneNonMarkedLeafNodes(node.right);

     
            if (!keepLeft) node.left = null;
            if (!keepRight) node.right = null;

            // If both children pruned, this becomes a leaf
            if (node.left == null && node.right == null)
            {
                //mark it to be removed too
                return false;
            }
            return true;
        }
        public static void SplitNode(BspNode node, int depth, int maxDepth, int minRoomSize, float minSizeMultiplier = 1.0f, float aspectRatioThreshold = 1.5f, float edgeMarginDivisor = 2f)
        {
            if (depth >= maxDepth ||
                node.rect.Width < minRoomSize * minSizeMultiplier ||
                node.rect.Height < minRoomSize * minSizeMultiplier)
                return;

            // Calculate current aspect ratio
            float currentAspectRatio = (float)Math.Max(node.rect.Width, node.rect.Height) /
                                      Math.Max(1, Math.Min(node.rect.Width, node.rect.Height));

            bool splitHorizontal;
            if (currentAspectRatio >= aspectRatioThreshold)
            {
                // Force split along long axis
                splitHorizontal = node.rect.Width > node.rect.Height;
            }
            else
            {
                // If aspect ratio is acceptable
                splitHorizontal = node.rect.Width > node.rect.Height ?
                    Rand.Value < 0.7f : Rand.Value < 0.3f;
            }

            int minMargin = (int)(minRoomSize / edgeMarginDivisor);

            if (splitHorizontal)
            {
                if (node.rect.Width < minRoomSize * minSizeMultiplier * 2 + minMargin * 2)
                {
                    splitHorizontal = false;
                }
            }
            else
            {
                if (node.rect.Height < minRoomSize * minSizeMultiplier * 2 + minMargin * 2)
                {
                    splitHorizontal = true;
                }
            }

            // Final check - if we still can't split properly, return
            if (splitHorizontal && node.rect.Width < minRoomSize * minSizeMultiplier * 2 + minMargin * 2)
                return;
            if (!splitHorizontal && node.rect.Height < minRoomSize * minSizeMultiplier * 2 + minMargin * 2)
                return;

            if (splitHorizontal)
            {
                float splitRatio = Rand.Range(0.4f, 0.6f);
                int splitPos = node.rect.minX + (int)(node.rect.Width * splitRatio);

                // Ensure margins
                splitPos = Math.Max(node.rect.minX + minMargin,
                          Math.Min(node.rect.maxX - minMargin, splitPos));

                node.left = new BspNode
                {
                    rect = new CellRect(node.rect.minX, node.rect.minZ,
                                      splitPos - node.rect.minX, node.rect.Height)
                };

                node.right = new BspNode
                {
                    rect = new CellRect(splitPos, node.rect.minZ,
                                      node.rect.maxX - splitPos, node.rect.Height)
                };
            }
            else
            {
                float splitRatio = Rand.Range(0.4f, 0.6f);
                int splitPos = node.rect.minZ + (int)(node.rect.Height * splitRatio);

                // Ensure margins
                splitPos = Math.Max(node.rect.minZ + minMargin,
                          Math.Min(node.rect.maxZ - minMargin, splitPos));

                node.left = new BspNode
                {
                    rect = new CellRect(node.rect.minX, node.rect.minZ,
                                      node.rect.Width, splitPos - node.rect.minZ)
                };

                node.right = new BspNode
                {
                    rect = new CellRect(node.rect.minX, splitPos,
                                      node.rect.Width, node.rect.maxZ - splitPos)
                };
            }

            SplitNode(node.left, depth + 1, maxDepth, minRoomSize,
                      minSizeMultiplier, aspectRatioThreshold, edgeMarginDivisor);
            SplitNode(node.right, depth + 1, maxDepth, minRoomSize,
                      minSizeMultiplier, aspectRatioThreshold, edgeMarginDivisor);
        }

        public static void GetLeafNodes(BspNode node, List<BspNode> leafNodes)
        {
            if (node == null)
                return;

            if (node.left == null && node.right == null)
            {
                leafNodes.Add(node);
            }
            else
            {
                GetLeafNodes(node.left, leafNodes);
                GetLeafNodes(node.right, leafNodes);
            }
        }

        public static void GenerateRooms(List<BspNode> leafNodes, int minPadding = 1, float roomSizeFactor = 0.75f)
        {
            foreach (BspNode leaf in leafNodes)
            {
                int maxRoomWidth = (int)(leaf.rect.Width * roomSizeFactor);
                int maxRoomHeight = (int)(leaf.rect.Height * roomSizeFactor);

                float targetAspectRatio = 1.5f;

                int roomWidth, roomHeight;

                float maxAspectRatio = (float)Math.Max(maxRoomWidth, maxRoomHeight) /
                                       Math.Max(1, Math.Min(maxRoomWidth, maxRoomHeight));

                if (maxAspectRatio > targetAspectRatio)
                {
                    if (maxRoomWidth > maxRoomHeight)
                    {
                        //Width is longer, so constrain it based on height
                        roomHeight = Rand.Range(4, Math.Max(4, maxRoomHeight));
                        roomWidth = Rand.Range(
                            Math.Max(4, (int)(roomHeight * 0.8f)),
                            Math.Min(maxRoomWidth, (int)(roomHeight * targetAspectRatio))
                        );
                    }
                    else
                    {
                        roomWidth = Rand.Range(4, Math.Max(4, maxRoomWidth));
                        roomHeight = Rand.Range(
                            Math.Max(4, (int)(roomWidth * 0.8f)),
                            Math.Min(maxRoomHeight, (int)(roomWidth * targetAspectRatio))
                        );
                    }
                }
                else
                {
                    roomWidth = Rand.Range(4, Math.Max(5, maxRoomWidth));
                    roomHeight = Rand.Range(4, Math.Max(5, maxRoomHeight));
                }

                roomWidth = Math.Max(4, roomWidth);
                roomHeight = Math.Max(4, roomHeight);

                int x = leaf.rect.minX + Rand.Range(minPadding, Math.Max(minPadding, leaf.rect.Width - roomWidth - minPadding));
                int z = leaf.rect.minZ + Rand.Range(minPadding, Math.Max(minPadding, leaf.rect.Height - roomHeight - minPadding));

                // Create the room
                leaf.room = new CellRect(x, z, roomWidth, roomHeight);
                leaf.roomWalls = new CellRect(x - 1, z - 1, roomWidth + 2, roomHeight + 2);
            }
        }



        public static Tuple<BspNode, BspNode> FindFurthestPair(List<BspNode> nodes)
        {
            BspNode node1 = null;
            BspNode node2 = null;
            float maxDistance = 0f;

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    float distance = Vector3.Distance(
                        nodes[i].room.CenterCell.ToVector3(),
                        nodes[j].room.CenterCell.ToVector3());

                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        node1 = nodes[i];
                        node2 = nodes[j];
                    }
                }
            }

            return new Tuple<BspNode, BspNode>(node1, node2);
        }  
    }
}
