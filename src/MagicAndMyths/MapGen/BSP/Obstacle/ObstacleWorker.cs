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
        public abstract bool TryPlaceObstacles(Map map, Dungeon Dungeon, DungeonRoom Room);
    }
}
