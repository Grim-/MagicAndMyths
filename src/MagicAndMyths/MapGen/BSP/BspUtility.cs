using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class BspUtility
    {
        public static BspNode GenerateBspTreeWithSideRooms(CellRect rootRect, int totalRoomCount, int mainRoomCount, int sideRoomCount, int minRoomSize = 10, int maxSplitAttempts = 100, float maxElongationRatio = 1.2f, float splitBufferDivisor = 2f)
        {
            int totalRooms = totalRoomCount;
            int initialMaxDepth = (int)Math.Ceiling(Math.Log(totalRooms, 2)) + 1;
            BspNode rootNode = new BspNode { rect = rootRect };
            SplitNode(rootNode, 0, initialMaxDepth, minRoomSize, maxElongationRatio, splitBufferDivisor);

            List<BspNode> leafNodes = new List<BspNode>();
            GetLeafNodes(rootNode, leafNodes);

            Log.Message($"BSP generated {leafNodes.Count} potential rooms, target: {mainRoomCount} main + {sideRoomCount} side = {totalRooms} total");

            int attempts = 0;
            while (leafNodes.Count < totalRooms && attempts < maxSplitAttempts)
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
                    SplitNode(largestNode, 0, 1, minRoomSize, maxElongationRatio, splitBufferDivisor);
                    leafNodes.Clear();
                    GetLeafNodes(rootNode, leafNodes);
                }
                else
                {
                    break;
                }

                attempts++;
            }

            if (leafNodes.Count > mainRoomCount)
            {
                leafNodes.Shuffle();

                var mainPathNodes = leafNodes.Take(mainRoomCount).ToList();
                int actualSideRoomCount = Math.Min(sideRoomCount, leafNodes.Count - mainRoomCount);
                var sidePathNodes = leafNodes.Skip(mainRoomCount).Take(actualSideRoomCount).ToList();

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
                }
            }

            return rootNode;
        }

        private static bool ShouldSplit(BspNode node, int depth, int maxDepth, int minRoomSize, float minSizeMultiplier)
        {
            int requiredSize = (int)(minRoomSize * minSizeMultiplier);

            return depth < maxDepth &&
                   node.rect.Width >= requiredSize &&
                   node.rect.Height >= requiredSize;
        }

        private static bool CanSplit(CellRect rect, bool horizontal, int minRoomSize, int minSplitBuffer, float minSizeMultiplier)
        {
            int requiredSize = (int)(minRoomSize * minSizeMultiplier);

            if (horizontal)
                return rect.Width >= (requiredSize * 2) + (minSplitBuffer * 2);
            else
                return rect.Height >= (requiredSize * 2) + (minSplitBuffer * 2);
        }

        public static void SplitNode(BspNode node, int depth, int maxDepth, int minRoomSize, float baseElongationRatio = 1.5f, float splitBufferDivisor = 2f, float minSizeMultiplier = 1.2f)
        {
            if (!ShouldSplit(node, depth, maxDepth, minRoomSize, minSizeMultiplier))
                return;

            float maxElongationRatio = Rand.Range(baseElongationRatio * 0.8f, baseElongationRatio * 1.4f);
            int minSplitBuffer = (int)(minRoomSize / splitBufferDivisor);

            bool splitHorizontal = DetermineSplitOrientation(node.rect, maxElongationRatio, minSplitBuffer);

            if (!CanSplit(node.rect, splitHorizontal, minRoomSize, minSplitBuffer, minSizeMultiplier))
                return;

            int splitPos = GetSplitPosition(node.rect, splitHorizontal, minSplitBuffer);

            var (leftRect, rightRect) = GetChildRects(node.rect, splitHorizontal, splitPos);
            node.left = new BspNode { rect = leftRect };
            node.right = new BspNode { rect = rightRect };

            SplitNode(node.left, depth + 1, maxDepth, minRoomSize, maxElongationRatio, splitBufferDivisor, minSizeMultiplier);
            SplitNode(node.right, depth + 1, maxDepth, minRoomSize, maxElongationRatio, splitBufferDivisor, minSizeMultiplier);
        }

        private static bool DetermineSplitOrientation(CellRect rect, float maxElongationRatio, int minSplitBuffer)
        {
            float currentRatio = (float)Math.Max(rect.Width, rect.Height) / Math.Max(1, Math.Min(rect.Width, rect.Height));

            if (currentRatio >= maxElongationRatio)
            {
                return rect.Width > rect.Height;
            }

            return rect.Width > rect.Height ? Rand.Value < 0.6f : Rand.Value < 0.4f;
        }

        private static int GetSplitPosition(CellRect rect, bool horizontal, int minSplitBuffer)
        {
            float splitRatio = Rand.Range(0.45f, 0.55f);

            if (horizontal)
            {
                int split = rect.minX + (int)(rect.Width * splitRatio);
                return Mathf.Clamp(split, rect.minX + minSplitBuffer, rect.maxX - minSplitBuffer);
            }
            else
            {
                int split = rect.minZ + (int)(rect.Height * splitRatio);
                return Mathf.Clamp(split, rect.minZ + minSplitBuffer, rect.maxZ - minSplitBuffer);
            }
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

            if (!keepLeft)
                node.left = null;
            if (!keepRight)
                node.right = null;

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
    }
}