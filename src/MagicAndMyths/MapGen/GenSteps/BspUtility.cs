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

        public static List<RoomConnection> GenerateRuleBasedConnections(List<BspUtility.BspNode> leafNodes)
        {
            List<RoomConnection> potentialConnections = new List<RoomConnection>();
            HashSet<string> processedConnections = new HashSet<string>();

            foreach (var node in leafNodes)
            {
                foreach (var connectedNode in node.connectedNodes)
                {
                    string connectionId = GetConnectionId(node, connectedNode);

                    if (!processedConnections.Contains(connectionId))
                    {
                        var connection = new RoomConnection(node, connectedNode);
                        connection.corridors = CorridoorUtility.GenerateCorridors(node, connectedNode);
                        potentialConnections.Add(connection);
                        processedConnections.Add(connectionId);
                    }
                }
            }

            List<RoomConnection> validConnections = RoomConnectionRuleManager.ApplyRules(leafNodes, potentialConnections);

            foreach (var connection in validConnections)
            {
                DetermineIfConnectionNeedsDoor(connection, leafNodes);
            }

            UpdateNodeConnections(leafNodes, validConnections);

            return validConnections;
        }

        private static void UpdateNodeConnections(List<BspUtility.BspNode> nodes, List<RoomConnection> validConnections)
        {
            // Clear existing connections
            foreach (var node in nodes)
            {
                if (node.IsOnCriticalPath)
                {
                    continue;
                }

                node.connectedNodes.Clear();
            }

            // Set connections based on validated connections
            foreach (var connection in validConnections)
            {
                connection.roomA.connectedNodes.Add(connection.roomB);
                connection.roomB.connectedNodes.Add(connection.roomA);
            }
        }

        public static string GetConnectionId(BspUtility.BspNode node1, BspUtility.BspNode node2)
        {
            ulong id1 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node1);
            ulong id2 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node2);
            return id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";
        }

        private static void DetermineIfConnectionNeedsDoor(RoomConnection connection, List<BspUtility.BspNode> allRooms)
        {
            // Reuse existing door determination logic
            if (connection.roomA.IsOnCriticalPath && connection.roomB.IsOnCriticalPath)
            {
                connection.needsDoor = true;

                if (Math.Abs(connection.roomA.CriticalPathIndex - connection.roomB.CriticalPathIndex) == 1)
                {
                    int lowerIndex = Math.Min(connection.roomA.CriticalPathIndex, connection.roomB.CriticalPathIndex);
                    if (lowerIndex > 0)
                    {
                        connection.isLocked = true;
                    }
                }
                return;
            }

            // Check room definitions
            RoomTypeDef roomADef = GetRoomTypeDef(connection.roomA);
            RoomTypeDef roomBDef = GetRoomTypeDef(connection.roomB);

            if ((roomADef != null && IsSpecialRoomDef(roomADef)) ||
                (roomBDef != null && IsSpecialRoomDef(roomBDef)))
            {
                connection.needsDoor = true;
                return;
            }

            // Random chance for normal rooms
            if (Rand.Value < 0.6f)
            {
                connection.needsDoor = true;
            }
        }
        public static List<BspUtility.BspNode> SelectWaypoints(List<BspUtility.BspNode> nodes, BspUtility.BspNode start, BspUtility.BspNode end, int count)
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


        private static RoomTypeDef GetRoomTypeDef(BspUtility.BspNode node)
        {
            return node.def;
        }

        private static bool IsSpecialRoomDef(RoomTypeDef def)
        {
            return def?.roomType == RoomType.Start || def?.roomType == RoomType.End;
        }


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

        public static BspNodePair FindFurthestPair(List<BspNode> nodes)
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

            return new BspNodePair(node1, node2);
        }

        public class BspNodePair
        {
            public BspNode NodeOne;
            public BspNode NodeTwo;

            public BspNodePair(BspNode nodeOne, BspNode nodeTwo)
            {
                NodeOne = nodeOne;
                NodeTwo = nodeTwo;
            }
        }
    }



}
