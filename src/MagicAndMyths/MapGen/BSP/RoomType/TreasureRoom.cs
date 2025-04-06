using RimWorld;
using Verse;

namespace MagicAndMyths
{

    public class TreasureRoomDef : RoomTypeDef
    {




    }

    public class TreasureRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, DungeonRoom Room)
        {
            TerrainDef terrainDef = DefDatabase<TerrainDef>.GetNamed("GoldTile");
            DungeonUtil.SpawnTerrainForRoom(map, Room.roomCellRect, terrainDef);
            GenSpawn.Spawn(ThingDefOf.ArchiteCapsule, Room.roomCellRect.CenterCell, map);
        }
    }
}
