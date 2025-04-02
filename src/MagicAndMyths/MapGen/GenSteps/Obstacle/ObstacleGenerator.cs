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

            List<ObstacleDef> availableObstacles = DefDatabase<ObstacleDef>.AllDefs.ToList();
            if (availableObstacles.Count == 0)
            {
                Log.Warning("No obstacle defs found for dungeon generation");
                return;
            }

            int obstacleCount = DetermineObstacleCount(leafNodes);
            Log.Message($"Attempting to place {obstacleCount} obstacles in dungeon with {leafNodes.Count} rooms");

            Dictionary<ObstacleDef, int> placedObstacles = new Dictionary<ObstacleDef, int>();


            foreach (ObstacleDef obstacleDef in availableObstacles)
            {
                if (placedObstacles.Count >= obstacleCount)
                    break;

                if (!placedObstacles.ContainsKey(obstacleDef))
                {
                    placedObstacles.Add(obstacleDef, 0);
                }

                //reached count limit for this obstacle
                if (placedObstacles[obstacleDef] >= obstacleDef.maxCount)
                {
                    continue;
                }

                if (TryPlaceObstacle(map, rootNode, leafNodes, obstacleDef))
                {
                    placedObstacles[obstacleDef]++;
                }
            }

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
            int baseCount = Mathf.Max(1, leafNodes.Count / 3);

            int variance = Mathf.Max(1, baseCount / 2);
            int finalCount = baseCount + Rand.RangeInclusive(-variance, variance);

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

            float totalCommonality = availableObstacles.Sum(def => def.commonality);

            float selection = Rand.Range(0, totalCommonality);

            float runningTotal = 0;
            foreach (ObstacleDef def in availableObstacles)
            {
                runningTotal += def.commonality;
                if (selection <= runningTotal)
                    return def;
            }

            return availableObstacles.RandomElement();
        }
    }
}
