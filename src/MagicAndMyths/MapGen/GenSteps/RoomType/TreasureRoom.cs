using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class TreasureRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, CellRect RoomCellRect)
        {
            TerrainDef terrainDef = DefDatabase<TerrainDef>.GetNamed("GoldTile");

            DungeonUtil.SpawnTerrainForRoom(map, RoomCellRect, terrainDef);
            GenSpawn.Spawn(ThingDefOf.ArchiteCapsule, RoomCellRect.CenterCell, map);
        }
    }
}
