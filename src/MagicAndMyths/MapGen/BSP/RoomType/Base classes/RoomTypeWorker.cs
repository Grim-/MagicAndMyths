using Verse;

namespace MagicAndMyths
{
    public abstract class RoomTypeWorker
    {
        public RoomTypeDef def;

        public abstract void ApplyRoom(Map map, DungeonRoom Room);
    }

}
