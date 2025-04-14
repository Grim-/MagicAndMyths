using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class BspNode
    {
        public CellRect rect;
        public BspNode left;
        public BspNode right;
        public List<string> tags = new List<string>();
        public List<BspNode> connectedNodes = new List<BspNode>();
        public CellRect roomRect;

        public bool IsLeaf()
        {
            return left == null && right == null;
        }

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

        public CellRect GenerateRoomGeometryWithSize(int width, int height, int minPadding = 1)
        {
            int x = rect.minX + Mathf.Max(minPadding, (rect.Width - width) / 2);
            int z = rect.minZ + Mathf.Max(minPadding, (rect.Height - height) / 2);
            x = Mathf.Min(x, rect.maxX - width - minPadding);
            z = Mathf.Min(z, rect.maxZ - height - minPadding);

            return new CellRect(x, z, width, height);
        }

        public CellRect GenerateRoomGeometry(int minPadding = 1, float roomSizeFactor = 0.75f)
        {
            // Calculate room dimensions
            int maxRoomWidth = (int)(rect.Width * roomSizeFactor);
            int maxRoomHeight = (int)(rect.Height * roomSizeFactor);

            float targetAspectRatio = 1.5f;

            int roomWidth, roomHeight;

            float maxAspectRatio = (float)Math.Max(maxRoomWidth, maxRoomHeight) /
                                   Math.Max(1, Math.Min(maxRoomWidth, maxRoomHeight));

            if (maxAspectRatio > targetAspectRatio)
            {
                if (maxRoomWidth > maxRoomHeight)
                {
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

            int x = rect.minX + Rand.Range(minPadding, Math.Max(minPadding, rect.Width - roomWidth - minPadding));
            int z = rect.minZ + Rand.Range(minPadding, Math.Max(minPadding, rect.Height - roomHeight - minPadding));

            return new CellRect(x, z, roomWidth, roomHeight);
        }
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

    public static class BspUtility
    {
        public static BspNode GenerateBspTreeWithSideRooms(
                    CellRect rootRect, int mainRoomCount, int sideRoomCount,
                    int minRoomSize = 8, int maxSplitAttempts = 100,
                    float aspectRatioThreshold = 1.2f,
                    float edgeMarginDivisor = 2f)
        {
            // Calculate the total number of rooms we want to generate
            int totalRooms = mainRoomCount + sideRoomCount;

            // Use original algorithm to generate enough rooms
            int initialMaxDepth = (int)Math.Ceiling(Math.Log(totalRooms, 2)) + 1;
            BspNode rootNode = new BspNode { rect = rootRect };
            SplitNode(rootNode, 0, initialMaxDepth, minRoomSize, aspectRatioThreshold, edgeMarginDivisor);

            List<BspNode> leafNodes = new List<BspNode>();
            GetLeafNodes(rootNode, leafNodes);

            Log.Message($"BSP generated {leafNodes.Count} potential rooms, target: {mainRoomCount} main + {sideRoomCount} side = {totalRooms} total");

            // Handle too few nodes (keep trying to split to get enough rooms)
            int attempts = 0;
            while (leafNodes.Count < totalRooms && attempts < maxSplitAttempts)
            {
                // Find the largest leaf node
                BspNode largestNode = null;
                int largestArea = 0;

                foreach (var node in leafNodes)
                {
                    int area = node.rect.Width * node.rect.Height;
                    if (area > largestArea &&
                        node.rect.Width >= minRoomSize &&
                        node.rect.Height >= minRoomSize)
                    {
                        largestArea = area;
                        largestNode = node;
                    }
                }

                if (largestNode != null)
                {
                    SplitNode(largestNode, 0, 1, minRoomSize, aspectRatioThreshold, edgeMarginDivisor);
                    leafNodes.Clear();
                    GetLeafNodes(rootNode, leafNodes);
                   // Log.Message($"After split attempt {attempts + 1}: now {leafNodes.Count} rooms");
                }
                else
                {
                    break;
                }

                attempts++;
            }

            if (leafNodes.Count > mainRoomCount)
            {
                //Log.Message($"Classifying rooms: {mainRoomCount} main rooms + up to {sideRoomCount} side rooms");
                leafNodes.Shuffle();


                var mainPathNodes = leafNodes.Take(mainRoomCount).ToList();

                int actualSideRoomCount = Math.Min(sideRoomCount, leafNodes.Count - mainRoomCount);
                var sidePathNodes = leafNodes.Skip(mainRoomCount).Take(actualSideRoomCount).ToList();

                //Log.Message($"Selected {mainPathNodes.Count} main rooms and {sidePathNodes.Count} side rooms");

                foreach (var node in mainPathNodes)
                {
                    node.AddTag("keep");
                }

                foreach (var node in sidePathNodes)
                {
                    node.AddTag("side_path");
                    node.AddTag("keep");
                }

                PruneNonMarkedLeafNodes(rootNode);
                leafNodes.Clear();
                GetLeafNodes(rootNode, leafNodes);
                //Log.Message($"After classification: {leafNodes.Count} total rooms kept");
            }
            else if (leafNodes.Count < totalRooms)
            {
                Log.Warning($"Could only generate {leafNodes.Count} rooms out of {totalRooms} desired");
                foreach (var node in leafNodes)
                {
                    node.AddTag("keep");
                }

                if (leafNodes.Count > mainRoomCount)
                {
                    leafNodes.Shuffle();

                    var mainPathNodes = leafNodes.Take(mainRoomCount).ToList();
                    var sidePathNodes = leafNodes.Skip(mainRoomCount).ToList();

                    foreach (var node in sidePathNodes)
                    {
                        node.AddTag("side_path");
                    }

                    //Log.Message($"Marked {sidePathNodes.Count} excess rooms as side paths");
                }
            }

            return rootNode;
        }



        public static BspNode GenerateBspTreeWithRoomCount(CellRect rootRect, int minRooms, int maxRooms, int minRoomSize = 8, int maxSplitAttempts = 100, float aspectRatioThreshold = 1.2f, float edgeMarginDivisor = 2f)
        {
            int initialMaxDepth = (int)Math.Ceiling(Math.Log(maxRooms, 2)) + 1;

            BspNode rootNode = new BspNode { rect = rootRect };
            SplitNode(rootNode, 0, initialMaxDepth, minRoomSize, aspectRatioThreshold, edgeMarginDivisor);

            List<BspNode> leafNodes = new List<BspNode>();
            GetLeafNodes(rootNode, leafNodes);

            //Log.Message($"BSP generated {leafNodes.Count} potential rooms, target: {minRooms}-{maxRooms}");

            int attempts = 0;
            while (leafNodes.Count < minRooms && attempts < maxSplitAttempts)
            {
                BspNode largestNode = null;
                int largestArea = 0;

                foreach (var node in leafNodes)
                {
                    int area = node.rect.Width * node.rect.Height;
                    if (area > largestArea &&
                        node.rect.Width >= minRoomSize &&
                        node.rect.Height >= minRoomSize)
                    {
                        largestArea = area;
                        largestNode = node;
                    }
                }

                if (largestNode != null)
                {
                    SplitNode(largestNode, 0, 1, minRoomSize, aspectRatioThreshold, edgeMarginDivisor);

                    leafNodes.Clear();
                    GetLeafNodes(rootNode, leafNodes);
                    //Log.Message($"After split attempt {attempts + 1}: now {leafNodes.Count} rooms");
                }
                else
                {
                    break;
                }

                attempts++;
            }

            if (leafNodes.Count > maxRooms)
            {
                //Log.Message($"Too many rooms ({leafNodes.Count}), pruning to {maxRooms}");
                leafNodes.Shuffle();
                var nodesToKeep = leafNodes.Take(maxRooms).ToList();

                foreach (var node in nodesToKeep)
                {
                    node.AddTag("keep");
                }

                PruneNonMarkedLeafNodes(rootNode);

                leafNodes.Clear();
                GetLeafNodes(rootNode, leafNodes);
                //Log.Message($"After pruning: {leafNodes.Count} rooms");
            }

            return rootNode;
        }

        public static void SplitNode(BspNode node, int depth, int maxDepth, int minRoomSize, float aspectRatioThreshold = 1.5f, float edgeMarginDivisor = 2f)
        {
            if (!ShouldSplit(node, depth, maxDepth, minRoomSize))
                return;

            int minMargin = (int)(minRoomSize / edgeMarginDivisor);
            bool splitHorizontal = DetermineSplitOrientation(node.rect, aspectRatioThreshold, minMargin);

            if (!CanSplit(node.rect, splitHorizontal, minRoomSize,  minMargin))
                return;

            int splitPos = GetSplitPosition(node.rect, splitHorizontal, minMargin);

            var (leftRect, rightRect) = GetChildRects(node.rect, splitHorizontal, splitPos);
            node.left = new BspNode { rect = leftRect };
            node.right = new BspNode { rect = rightRect };

            SplitNode(node.left, depth + 1, maxDepth, minRoomSize, aspectRatioThreshold, edgeMarginDivisor);
            SplitNode(node.right, depth + 1, maxDepth, minRoomSize,  aspectRatioThreshold, edgeMarginDivisor);
        }

        private static bool ShouldSplit(BspNode node, int depth, int maxDepth, int minRoomSize)
        {
            return depth < maxDepth &&
                   node.rect.Width >= minRoomSize &&
                   node.rect.Height >= minRoomSize;
        }
        private static bool DetermineSplitOrientation(CellRect rect, float aspectRatioThreshold, int minMargin)
        {
            float aspectRatio = (float)Math.Max(rect.Width, rect.Height) / Math.Max(1, Math.Min(rect.Width, rect.Height));

            if (aspectRatio >= aspectRatioThreshold)
            {
                return rect.Width > rect.Height;
            }

            //square-ish areas, slightly prefer to alternate splitting direction
            return rect.Width > rect.Height ? Rand.Value < 0.6f : Rand.Value < 0.4f;
        }

        private static int GetSplitPosition(CellRect rect, bool horizontal, int minMargin)
        {
            float splitRatio = Rand.Range(0.45f, 0.55f);

            if (horizontal)
            {
                int split = rect.minX + (int)(rect.Width * splitRatio);
                return Mathf.Clamp(split, rect.minX + minMargin, rect.maxX - minMargin);
            }
            else
            {
                int split = rect.minZ + (int)(rect.Height * splitRatio);
                return Mathf.Clamp(split, rect.minZ + minMargin, rect.maxZ - minMargin);
            }
        }

        private static bool CanSplit(CellRect rect, bool horizontal, int minRoomSize, int minMargin)
        {
            if (horizontal)
                return rect.Width >= minRoomSize * 2 + minMargin * 2;
            else
                return rect.Height >= minRoomSize * 2 + minMargin * 2;
        }
        private static (CellRect, CellRect) GetChildRects(CellRect rect, bool horizontal, int splitPos)
        {
            if (horizontal)
            {
                return (
                    new CellRect(rect.minX, rect.minZ, splitPos - rect.minX, rect.Height),
                    new CellRect(splitPos, rect.minZ, rect.maxX - splitPos, rect.Height)
                );
            }
            else
            {
                return (
                    new CellRect(rect.minX, rect.minZ, rect.Width, splitPos - rect.minZ),
                    new CellRect(rect.minX, splitPos, rect.Width, rect.maxZ - splitPos)
                );
            }
        }

        private static bool PruneNonMarkedLeafNodes(BspNode node)
        {
            if (node == null) return false;

            if (node.IsLeaf())
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
                return false;
            }
            return true;
        }

        public static void GetLeafNodes(BspNode node, List<BspNode> leafNodes)
        {
            if (node == null)
                return;

            if (node.IsLeaf())
            {
                leafNodes.Add(node);
            }
            else
            {
                GetLeafNodes(node.left, leafNodes);
                GetLeafNodes(node.right, leafNodes);
            }
        }

        public static void GenerateRoomGeometry(List<BspNode> leafNodes, int minPadding = 1, float roomSizeFactor = 0.75f)
        {
            foreach (BspNode leaf in leafNodes)
            {
                leaf.roomRect = leaf.GenerateRoomGeometry(minPadding, roomSizeFactor);
            }
        }
        public static List<BspNode> SelectWaypoints(List<BspNode> nodes, BspNode start, BspNode end, int count)
        {
            var possibleWaypoints = nodes.Where(n => n != start && n != end).ToList();

            var straightLineDir = (end.roomRect.CenterCell.ToVector3() - start.roomRect.CenterCell.ToVector3()).normalized;
            var startPos = start.roomRect.CenterCell.ToVector3();

            possibleWaypoints.Sort((a, b) => {
                var aPos = a.roomRect.CenterCell.ToVector3();
                var bPos = b.roomRect.CenterCell.ToVector3();

                var aProj = Vector3.Dot((aPos - startPos), straightLineDir);
                var bProj = Vector3.Dot((bPos - startPos), straightLineDir);

                return -((aPos - (startPos + straightLineDir * aProj)).magnitude
                       .CompareTo((bPos - (startPos + straightLineDir * bProj)).magnitude));
            });

            return possibleWaypoints.Take(count).ToList();
        }
    }
}
