using Verse;

namespace MagicAndMyths
{
    public abstract class RoomTypeWorker
    {
        public CellRect RoomCellRect;

        public abstract void ApplyRoom(Map map, CellRect RoomCellRect);
    }

}
