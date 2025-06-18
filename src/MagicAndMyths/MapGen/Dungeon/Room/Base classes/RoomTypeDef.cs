using RimWorld;
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
        public List<RoomLayoutDef> perferredLayouts;
        public List<string> roomTags = new List<string>();


        public int maxRoomTypeCount = 5;

        public IntVec2 minSize = IntVec2.Invalid;
        public IntVec2 maxSize = IntVec2.Invalid;
        public bool requiresExactSize = false;

        public bool roomIsOnCriticalPath = true;

        public bool roomIsFogged = true;

        public bool canModifyFloor = true;
        public bool canModifyWalls = true;
        public bool canModifyTerrain = true;

        public ThingDef roomWallDef;
        public TerrainDef roomTerrainDef;
        public TerrainDef roomFloorDef;


        public ThingDef RoomWallDef => roomWallDef != null ? roomWallDef : MagicAndMythDefOf.DungeonWall;

        public TerrainDef RoomTerrainDef => roomFloorDef != null ? roomFloorDef : TerrainDefOf.SoilRich;

        public TerrainDef RoomFloorDef => roomFloorDef != null ? roomFloorDef : TerrainDefOf.Voidmetal;

        public RoomTypeWorker DoWorker(Map map,  Dungeon Dungeon, DungeonRoom Room)
        {
            RoomTypeWorker RoomTypeWorker = (RoomTypeWorker)Activator.CreateInstance(roomTypeWorker);
            RoomTypeWorker.def = Room.def;
            RoomTypeWorker.currentRoom = Room;
            RoomTypeWorker.ApplyRoom(map, Dungeon, Room);
            return RoomTypeWorker;
        }

        public bool CanApply(Dungeon Dungeon, DungeonRoom Room)
        {
            RoomTypeWorker RoomTypeWorker = (RoomTypeWorker)Activator.CreateInstance(roomTypeWorker);
            RoomTypeWorker.def = Room.def;
            RoomTypeWorker.currentRoom = Room;
            return true;
        }
    }
}
