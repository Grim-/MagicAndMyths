using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class ObstacleDef : Def
    {
        public Type workerClass;
        public ThingDef obstacleDef;
        public float commonality = 1f;
        public int maxCount = 3;
        public int minDistanceFromStart = 0;
        public bool requiresSeparateRooms = false;

        public bool DoWorker(Map map, BspUtility.BspNode rootNode, List<BspUtility.BspNode> leafNodes)
        {
            ObstacleWorker RoomTypeWorker = (ObstacleWorker)Activator.CreateInstance(workerClass);
            RoomTypeWorker.def = this;
            return RoomTypeWorker.TryPlaceObstacles(map, rootNode, leafNodes);
        }

    }
}
