using System;
using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class RoomTypeDef : Def
    {
        public RoomType roomType = RoomType.End;
        public Type roomTypeWorker;
        public List<ObstacleDef> roomObstacles;

        public IntVec2 minSize;
        public IntVec2 maxSize;


        public RoomTypeWorker DoWorker(Map map, DungeonRoom Room)
        {
            RoomTypeWorker RoomTypeWorker = (RoomTypeWorker)Activator.CreateInstance(roomTypeWorker);
            RoomTypeWorker.def = Room.def;
            RoomTypeWorker.ApplyRoom(map, Room);
            return RoomTypeWorker;
        }
    }
}
