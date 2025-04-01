using System;
using Verse;

namespace MagicAndMyths
{
    public class RoomTypeDef : Def
    {
        public RoomType roomType = RoomType.End;
        public Type roomTypeWorker;


        public RoomTypeWorker DoWorker(Map map, CellRect RoomCellRect)
        {
            RoomTypeWorker RoomTypeWorker = (RoomTypeWorker)Activator.CreateInstance(roomTypeWorker);
            RoomTypeWorker.ApplyRoom(map, RoomCellRect);
            return RoomTypeWorker;
        }
    }

}
