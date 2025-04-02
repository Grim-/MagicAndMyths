using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class BspUtility
    {
        public class BspNode
        {
            public CellRect rect;
            public BspNode left;
            public BspNode right;
            public CellRect room;
            public CellRect roomWalls;
            public object tag;
            public List<string> tags = new List<string>();
            public List<BspNode> connectedNodes = new List<BspNode>();


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
                var nodesToRemove = leafNodes.Skip(maxRooms).ToList();

                // Add a marker to the nodes we want to keep
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

            bool splitHorizontal = Rand.Value < 0.5f;

            // Bias split direction based on aspect ratio
            if (node.rect.Width > node.rect.Height * aspectRatioThreshold)
                splitHorizontal = true;
            else if (node.rect.Height > node.rect.Width * aspectRatioThreshold)
                splitHorizontal = false;

            if (splitHorizontal)
            {
                int margin = (int)(minRoomSize / edgeMarginDivisor);
                int splitPos = Rand.Range(
                    node.rect.minX + margin,
                    node.rect.maxX - margin
                );

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
                int margin = (int)(minRoomSize / edgeMarginDivisor);
                int splitPos = Rand.Range(
                    node.rect.minZ + margin,
                    node.rect.maxZ - margin
                );

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
                int roomWidth = Rand.Range(4, Math.Max(4, maxRoomWidth));
                int roomHeight = Rand.Range(4, Math.Max(4, maxRoomHeight));
                int x = leaf.rect.minX + Rand.Range(minPadding, Math.Max(minPadding, leaf.rect.Width - roomWidth - minPadding));
                int z = leaf.rect.minZ + Rand.Range(minPadding, Math.Max(minPadding, leaf.rect.Height - roomHeight - minPadding));
                leaf.room = new CellRect(x, z, roomWidth, roomHeight);

                leaf.roomWalls = new CellRect(x - 1, z - 1, roomWidth + 2, roomHeight + 2);
            }
        }
        public static void AddRandomConnections(List<BspNode> nodes, int connectionCount)
        {
            if (nodes.Count <= 2)
                return;

            for (int i = 0; i < connectionCount; i++)
            {
                int node1Index = Rand.Range(0, nodes.Count);
                int node2Index = Rand.Range(0, nodes.Count);

                if (node1Index != node2Index)
                {
                    BspNode node1 = nodes[node1Index];
                    BspNode node2 = nodes[node2Index];

                    if (!node1.connectedNodes.Contains(node2))
                    {
                        node1.connectedNodes.Add(node2);
                        node2.connectedNodes.Add(node1);
                    }
                }
            }
        }

        public static List<Corridoor> GenerateCorridorPoints(BspNode node1, BspNode node2)
        {
            if (node1.room == null || node2.room == null)
                return new List<Corridoor>();

            IntVec3 center1 = node1.room.CenterCell;
            IntVec3 center2 = node2.room.CenterCell;
            List<Corridoor> corridorSegments = new List<Corridoor>();

            IntVec3 corner = new IntVec3(center1.x, 0, center2.z);

            corridorSegments.Add(new Corridoor(center1, new IntVec3(corner.x, 0, center1.z)));
            corridorSegments.Add(new Corridoor(new IntVec3(corner.x, 0, center1.z), corner));
            corridorSegments.Add(new Corridoor(corner, new IntVec3(center2.x, 0, corner.z)));
            corridorSegments.Add(new Corridoor(new IntVec3(center2.x, 0, corner.z), center2));

            return corridorSegments;
        }

        public static void ApplyCorridorsToGrid(List<Corridoor> corridorSegments, Map map, BoolGrid grid)
        {
            foreach (var segment in corridorSegments)
            {
                DrawLineBetweenPoints(segment.Start, segment.End, map, grid);
            }
        }

        private static void DrawLineBetweenPoints(IntVec3 start, IntVec3 end, Map map, BoolGrid grid)
        {
            int dx = Math.Sign(end.x - start.x);
            int dz = Math.Sign(end.z - start.z);

            if (dx != 0)
            {
                for (int x = start.x; x != end.x + dx; x += dx)
                {
                    IntVec3 pos = new IntVec3(x, 0, start.z);
                    if (pos.InBounds(map))
                    {
                        grid[pos] = true;
                    }
                }
            }

            if (dz != 0)
            {
                for (int z = start.z; z != end.z + dz; z += dz)
                {
                    IntVec3 pos = new IntVec3(end.x, 0, z);
                    if (pos.InBounds(map))
                    {
                        grid[pos] = true;
                    }
                }
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



        public static void CreateMinimumSpanningTree(List<BspUtility.BspNode> nodes)
        {
            if (nodes.Count <= 1)
                return;

            foreach (var node in nodes)
            {
                node.connectedNodes.Clear();
            }

            HashSet<BspNode> treeNodes = new HashSet<BspUtility.BspNode>();

            treeNodes.Add(nodes[Rand.Range(0, nodes.Count)]);

            while (treeNodes.Count < nodes.Count)
            {
                BspNode bestNode1 = null;
                BspNode bestNode2 = null;
                float shortestDistance = float.MaxValue;

                foreach (var treeNode in treeNodes)
                {
                    foreach (var node in nodes)
                    {
                        if (!treeNodes.Contains(node))
                        {
                            float distance = Vector3.Distance(
                                treeNode.room.CenterCell.ToVector3(),
                                node.room.CenterCell.ToVector3());

                            if (distance < shortestDistance)
                            {
                                shortestDistance = distance;
                                bestNode1 = treeNode;
                                bestNode2 = node;
                            }
                        }
                    }
                }

                if (bestNode1 != null && bestNode2 != null)
                {
                    bestNode1.connectedNodes.Add(bestNode2);
                    bestNode2.connectedNodes.Add(bestNode1);
                    treeNodes.Add(bestNode2);
                }
                else
                    break;
            }
        }
    }


    public class Corridoor
    {
        public IntVec3 Start;
        public IntVec3 End;

        public Corridoor(IntVec3 start, IntVec3 end)
        {
            Start = start;
            End = end;
        }
    }
}
