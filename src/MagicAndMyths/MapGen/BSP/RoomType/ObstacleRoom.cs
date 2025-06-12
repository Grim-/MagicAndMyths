using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class ObstacleRoomDef : RoomTypeDef
    {
        public List<ObstacleDef> obstacles = new List<ObstacleDef>();

        public ObstacleRoomDef()
        {
            this.roomTypeWorker = typeof(ObstacleRoom);
        }
    }
    public class ObstacleRoom : RoomTypeWorker
    {
        ObstacleRoomDef Def => (ObstacleRoomDef)def;

        public override void ApplyRoom(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            base.ApplyRoom(map, Dungeon, Room);
            ObstacleGenerator.TryPlaceObstacle(map, Dungeon, this.currentRoom, Def.obstacles.RandomElement());
        }
    }
}
