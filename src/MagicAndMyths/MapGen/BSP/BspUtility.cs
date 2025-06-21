using LudeonTK;
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
        // Configuration for BSP generation
        public struct BspConfig
        {
            public int minRoomSize;
            public float splitBufferRatio;     // Was splitBufferDivisor, now clearer as ratio
            public float maxElongationRatio;   // Maximum width/height ratio before forcing split
            public float minSplitPosition;     // Was hardcoded 0.3f
            public float maxSplitPosition;     // Was hardcoded 0.6f
            public int maxSplitAttempts;

            public int MinSplitBuffer => (int)(minRoomSize / splitBufferRatio);
            public int MinSplittableDimension => (minRoomSize * 2) + (MinSplitBuffer * 2);

            public static BspConfig Default => new BspConfig
            {
                minRoomSize = 10,
                splitBufferRatio = 2f,
                maxElongationRatio = 1.5f,
                minSplitPosition = 0.3f,
                maxSplitPosition = 0.6f,
                maxSplitAttempts = 300
            };
        }

        // Tweakable values for runtime adjustment
        [TweakValue("MagicAndMyths", 0.1f, 3f)]
        public static float splitBufferRatio = 2f;

        [TweakValue("MagicAndMyths", 0.1f, 3f)]
        public static float maxElongationRatio = 1.5f;

        [TweakValue("MagicAndMyths", 0.1f, 1f)]
        public static float minSplitPosition = 0.3f;

        [TweakValue("MagicAndMyths", 0.1f, 1f)]
        public static float maxSplitPosition = 0.6f;

        public static BspNode GenerateBspTreeWithSideRooms(
            CellRect rootRect,
            int totalRoomCount,
            int mainRoomCount,
            int sideRoomCount,
            int minRoomSize = 10,
            int maxSplitAttempts = 300)
        {
            // Create config from parameters and tweakable values
            var config = new BspConfig
            {
                minRoomSize = minRoomSize,
                splitBufferRatio = splitBufferRatio,
                maxElongationRatio = maxElongationRatio,
                minSplitPosition = minSplitPosition,
                maxSplitPosition = maxSplitPosition,
                maxSplitAttempts = maxSplitAttempts
            };


            BspNode rootNode = GenerateInitialBspTree(rootRect, totalRoomCount, config);

            List<BspNode> leafNodes = GetAllLeafNodes(rootNode);
            Log.Message($"BSP generated {leafNodes.Count} potential rooms, target: {mainRoomCount} main + {sideRoomCount} side = {totalRoomCount} total");


            if (leafNodes.Count < totalRoomCount)
            {
                leafNodes = AttemptToReachTargetRoomCount(rootNode, leafNodes, totalRoomCount, config);
            }


            SelectAndMarkRooms(leafNodes, mainRoomCount, sideRoomCount);


            PruneUnmarkedLeafNodes(rootNode);

            return rootNode;
        }

        private static BspNode GenerateInitialBspTree(CellRect rootRect, int targetRoomCount, BspConfig config)
        {
            int initialMaxDepth = (int)Math.Ceiling(Math.Log(targetRoomCount, 2)) + 1;
            BspNode rootNode = new BspNode { rect = rootRect };

            SplitNode(rootNode, 0, initialMaxDepth, config);

            return rootNode;
        }

        private static List<BspNode> AttemptToReachTargetRoomCount(BspNode rootNode, List<BspNode> leafNodes, int targetCount, BspConfig config)
        {
            int attempts = 0;

            while (leafNodes.Count < targetCount && attempts < config.maxSplitAttempts)
            {
                BspNode largestSplittableNode = FindLargestSplittableNode(leafNodes, config);

                if (largestSplittableNode != null)
                {
           
                    float adaptiveMultiplier = Math.Max(0.5f, 1f - (attempts * 0.01f));
                    var adaptiveConfig = config;
                    adaptiveConfig.minRoomSize = (int)(config.minRoomSize * adaptiveMultiplier);

                    SplitNode(largestSplittableNode, 0, 1, adaptiveConfig);
                    leafNodes = GetAllLeafNodes(rootNode);
                }
                else
                {
                    break;
                }

                attempts++;
            }

            if (leafNodes.Count < targetCount)
            {
                Log.Warning($"Could only generate {leafNodes.Count} rooms out of {targetCount} desired");
            }

            return leafNodes;
        }

        private static BspNode FindLargestSplittableNode(List<BspNode> leafNodes, BspConfig config)
        {
            BspNode largestNode = null;
            int largestArea = 0;

            foreach (var node in leafNodes)
            {
                int area = node.rect.Area;
                if (area > largestArea && CanSplitNode(node.rect, config))
                {
                    largestArea = area;
                    largestNode = node;
                }
            }

            return largestNode;
        }

        private static bool CanSplitNode(CellRect rect, BspConfig config)
        {
            return rect.Width >= config.MinSplittableDimension ||
                   rect.Height >= config.MinSplittableDimension;
        }

        private static void SelectAndMarkRooms(List<BspNode> leafNodes, int mainRoomCount, int sideRoomCount)
        {
            if (leafNodes.Count == 0) return;

            leafNodes.Shuffle();


            var mainPathNodes = leafNodes.Take(Math.Min(mainRoomCount, leafNodes.Count)).ToList();
            foreach (var node in mainPathNodes)
            {
                node.AddTag("keep");
            }


            if (leafNodes.Count > mainRoomCount)
            {
                int actualSideRoomCount = Math.Min(sideRoomCount, leafNodes.Count - mainRoomCount);
                var sidePathNodes = leafNodes.Skip(mainRoomCount).Take(actualSideRoomCount).ToList();

                foreach (var node in sidePathNodes)
                {
                    node.AddTag("keep");
                    node.AddTag("side_path");
                }
            }
        }

        private static void SplitNode(BspNode node, int depth, int maxDepth, BspConfig config)
        {
            if (depth >= maxDepth || node == null) return;

            // Determine split orientation
            bool splitHorizontal = ShouldSplitHorizontally(node.rect, config);

            // Check if split is possible
            if (!CanSplitInDirection(node.rect, splitHorizontal, config))
                return;

            // Calculate split position
            int splitPos = CalculateSplitPosition(node.rect, splitHorizontal, config);

            // Create child nodes
            CreateChildNodes(node, splitHorizontal, splitPos);

            // Recursively split children
            SplitNode(node.left, depth + 1, maxDepth, config);
            SplitNode(node.right, depth + 1, maxDepth, config);
        }

        private static bool ShouldSplitHorizontally(CellRect rect, BspConfig config)
        {
            float aspectRatio = (float)rect.Width / rect.Height;

            // Force split on longer dimension if too elongated
            if (aspectRatio > config.maxElongationRatio)
                return true;
            if (aspectRatio < 1f / config.maxElongationRatio)
                return false;

            // Otherwise, prefer splitting the longer dimension with some randomness
            return rect.Width > rect.Height ? Rand.Value < 0.6f : Rand.Value < 0.4f;
        }

        private static bool CanSplitInDirection(CellRect rect, bool horizontal, BspConfig config)
        {
            int dimension = horizontal ? rect.Width : rect.Height;
            return dimension >= config.MinSplittableDimension;
        }

        private static int CalculateSplitPosition(CellRect rect, bool horizontal, BspConfig config)
        {
            float splitRatio = Rand.Range(config.minSplitPosition, config.maxSplitPosition);
            int minBuffer = config.MinSplitBuffer;

            if (horizontal)
            {
                int split = rect.minX + (int)(rect.Width * splitRatio);
                return Mathf.Clamp(split, rect.minX + minBuffer + config.minRoomSize, rect.maxX - (minBuffer + config.minRoomSize));
            }
            else
            {
                int split = rect.minZ + (int)(rect.Height * splitRatio);
                return Mathf.Clamp(split, rect.minZ + minBuffer + config.minRoomSize, rect.maxZ - (minBuffer + config.minRoomSize));
            }
        }

        private static void CreateChildNodes(BspNode node, bool horizontal, int splitPos)
        {
            if (horizontal)
            {
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
        }

        private static List<BspNode> GetAllLeafNodes(BspNode rootNode)
        {
            List<BspNode> leafNodes = new List<BspNode>();
            CollectLeafNodes(rootNode, leafNodes);
            return leafNodes;
        }

        private static void CollectLeafNodes(BspNode node, List<BspNode> leafNodes)
        {
            if (node == null) return;

            if (node.IsLeaf())
            {
                leafNodes.Add(node);
            }
            else
            {
                CollectLeafNodes(node.left, leafNodes);
                CollectLeafNodes(node.right, leafNodes);
            }
        }

        private static bool PruneUnmarkedLeafNodes(BspNode node)
        {
            if (node == null) return false;

            if (node.IsLeaf())
            {
                return node.HasTag("keep");
            }

            bool keepLeft = PruneUnmarkedLeafNodes(node.left);
            bool keepRight = PruneUnmarkedLeafNodes(node.right);

            if (!keepLeft) node.left = null;
            if (!keepRight) node.right = null;
            return node.left != null || node.right != null;
        }

        public static void GetLeafNodes(BspNode node, List<BspNode> leafNodes)
        {
            CollectLeafNodes(node, leafNodes);
        }
    }
}