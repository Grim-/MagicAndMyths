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

        public bool DoWorker(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            ObstacleWorker RoomTypeWorker = (ObstacleWorker)Activator.CreateInstance(workerClass);
            RoomTypeWorker.def = this;
            return RoomTypeWorker.TryPlaceObstacles(map, Dungeon, Room);
        }

    }
}
