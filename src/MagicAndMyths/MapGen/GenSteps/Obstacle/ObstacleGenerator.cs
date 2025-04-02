using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class ObstacleGenerator
    {
        /// <summary>
        /// Places obstacles throughout the dungeon after room generation is complete
        /// </summary>
        public static void GenerateObstacles(Map map, BspUtility.BspNode rootNode, List<BspUtility.BspNode> leafNodes)
        {
            if (leafNodes.Count <= 1)
                return;

            // Get all available obstacle defs
            List<ObstacleDef> availableObstacles = DefDatabase<ObstacleDef>.AllDefs.ToList();
            if (availableObstacles.Count == 0)
            {
                Log.Warning("No obstacle defs found for dungeon generation");
                return;
            }

            // Determine number of obstacles to place based on dungeon size
            int obstacleCount = DetermineObstacleCount(leafNodes);
            Log.Message($"Attempting to place {obstacleCount} obstacles in dungeon with {leafNodes.Count} rooms");

            // Keep track of placed obstacles
            HashSet<ObstacleDef> placedObstacles = new HashSet<ObstacleDef>();

            // First pass: try to place one of each type if possible
            foreach (ObstacleDef obstacleDef in availableObstacles)
            {
                if (placedObstacles.Count >= obstacleCount)
                    break;

                if (TryPlaceObstacle(map, rootNode, leafNodes, obstacleDef))
                {
                    placedObstacles.Add(obstacleDef);
                }
            }

            // Second pass: fill remaining slots with weighted random selection
            int remainingSlots = obstacleCount - placedObstacles.Count;
            for (int i = 0; i < remainingSlots; i++)
            {
                ObstacleDef selectedDef = SelectObstacleDef(availableObstacles);
                if (selectedDef != null)
                {
                    TryPlaceObstacle(map, rootNode, leafNodes, selectedDef);
                }
            }

            Log.Message($"Successfully placed obstacles in dungeon");
        }

        /// <summary>
        /// Determines how many obstacles to place based on dungeon size
        /// </summary>
        private static int DetermineObstacleCount(List<BspUtility.BspNode> leafNodes)
        {
            // Base count on number of rooms
            int baseCount = Mathf.Max(1, leafNodes.Count / 3);

            // Add some randomness
            int variance = Mathf.Max(1, baseCount / 2);
            int finalCount = baseCount + Rand.RangeInclusive(-variance, variance);

            // Never use more obstacles than rooms-1 (to ensure at least one free room)
            return Mathf.Min(finalCount, leafNodes.Count - 1);
        }

        /// <summary>
        /// Attempts to place a specific obstacle in the dungeon
        /// </summary>
        private static bool TryPlaceObstacle(Map map, BspUtility.BspNode rootNode, List<BspUtility.BspNode> leafNodes, ObstacleDef obstacleDef)
        {
            try
            {
                return obstacleDef.DoWorker(map, rootNode, leafNodes);
            }
            catch (Exception ex)
            {
                Log.Error($"Error placing obstacle {obstacleDef.defName}: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Selects an obstacle def based on weighted probability
        /// </summary>
        private static ObstacleDef SelectObstacleDef(List<ObstacleDef> availableObstacles)
        {
            if (availableObstacles.Count == 0)
                return null;

            // Calculate total commonality
            float totalCommonality = availableObstacles.Sum(def => def.commonality);

            // Select a random point in the probability space
            float selection = Rand.Range(0, totalCommonality);

            // Find which obstacle this corresponds to
            float runningTotal = 0;
            foreach (ObstacleDef def in availableObstacles)
            {
                runningTotal += def.commonality;
                if (selection <= runningTotal)
                    return def;
            }

            // Fallback in case of rounding errors
            return availableObstacles.RandomElement();
        }
    }
}
