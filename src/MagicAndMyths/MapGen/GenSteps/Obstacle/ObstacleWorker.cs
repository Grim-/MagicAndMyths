using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public abstract class ObstacleWorker
    {
        public ObstacleDef def;

        /// <summary>
        /// Places obstacles and solutions in the dungeon
        /// </summary>
        /// <param name="map">The map</param>
        /// <param name="rootNode">The BSP root node with all room data</param>
        /// <param name="leafNodes">List of leaf nodes (rooms)</param>
        /// <returns>True if placement was successful</returns>
        public abstract bool TryPlaceObstacles(Map map, BspUtility.BspNode rootNode, List<BspUtility.BspNode> leafNodes);

        /// <summary>
        /// Finds a suitable room for placing an obstacle based on criteria
        /// </summary>
        protected BspUtility.BspNode FindSuitableObstacleRoom(List<BspUtility.BspNode> leafNodes, BspUtility.BspNode startRoom, HashSet<BspUtility.BspNode> excludedRooms = null)
        {
            excludedRooms = excludedRooms ?? new HashSet<BspUtility.BspNode>();

            List<BspUtility.BspNode> suitableRooms = new List<BspUtility.BspNode>();

            foreach (var node in leafNodes)
            {
                if (excludedRooms.Contains(node))
                    continue;

                if (node.room == null)
                    continue;

                if (def.minDistanceFromStart > 0 && startRoom != null)
                {
                    float distance = Vector3.Distance(
                        startRoom.room.CenterCell.ToVector3(),
                        node.room.CenterCell.ToVector3());

                    if (distance < def.minDistanceFromStart)
                        continue;
                }

                // Check if the room has enough space
                if (node.room.Width < 5 || node.room.Height < 5)
                    continue;

                suitableRooms.Add(node);
            }

            if (suitableRooms.Count == 0)
                return null;

            return suitableRooms.RandomElement();
        }

        /// <summary>
        /// Finds a position within a room to place an obstacle or solution
        /// </summary>
        protected IntVec3 FindPlacementPosition(Map map, CellRect roomRect, IntVec3 centerBias = default, float centerBiasStrength = 0.5f)
        {
            List<IntVec3> validCells = roomRect.Cells
                .Where(c => c.Standable(map) && c.GetEdifice(map) == null)
                .ToList();

            if (validCells.Count == 0)
                return IntVec3.Invalid;

            if (centerBias != default && centerBiasStrength > 0)
            {
                if (centerBias == default)
                    centerBias = roomRect.CenterCell;

                // Sort cells by distance to center
                validCells.Sort((a, b) =>
                {
                    float distA = (a - centerBias).LengthHorizontalSquared;
                    float distB = (b - centerBias).LengthHorizontalSquared;
                    return distA.CompareTo(distB);
                });

                // Take the closest cells based on bias strength
                int cellCount = Math.Max(1, (int)(validCells.Count * centerBiasStrength));
                return validCells.Take(cellCount).RandomElement();
            }

            return validCells.RandomElement();
        }
    }
}
