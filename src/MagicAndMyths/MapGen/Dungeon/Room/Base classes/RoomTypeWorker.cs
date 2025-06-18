using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public abstract class RoomTypeWorker
    {
        public RoomTypeDef def;
        public DungeonRoom currentRoom;

        public virtual void ApplyRoom(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            //if (def.roomIsFogged)
            //{
            //    map.fogGrid.Refog(Room.roomCellRect);
            //}

            if (def.canModifyTerrain)
            {
                ModifyRoomTerrain();
            }

            if (def.canModifyFloor)
            {
                ModifyRoomFloor();
            }

            if (def.canModifyWalls)
            {
                ModifyRoomWalls();
            }
        }


        public virtual bool CanApply(Dungeon Dungeon, DungeonRoom DungeonRoom)
        {
            if (def.maxRoomTypeCount > 0 && Dungeon.GetRoomTypeCount(def) >= def.maxRoomTypeCount)
            {
                return false;
            }

            return true;
        }


        protected virtual void ModifyRoomTerrain()
        {

        }

        protected virtual void ModifyRoomFloor()
        {

        }

        protected virtual void ModifyRoomWalls()
        {

        }
    }

}
