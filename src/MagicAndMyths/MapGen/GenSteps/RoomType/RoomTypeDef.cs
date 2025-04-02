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
        public IntRange roomSize;
        public bool roomHasDoors = true;
        public bool roomIsFogged = false;


        public RoomTypeWorker DoWorker(Map map, CellRect RoomCellRect)
        {
            RoomTypeWorker RoomTypeWorker = (RoomTypeWorker)Activator.CreateInstance(roomTypeWorker);
            RoomTypeWorker.ApplyRoom(map, RoomCellRect);
            return RoomTypeWorker;
        }
    }
}
