using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class TreasureRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, CellRect RoomCellRect)
        {

            TerrainDef terrainDef = DefDatabase<TerrainDef>.GetNamed("GoldTile");

            foreach (var item in RoomCellRect.Cells)
            {
                map.terrainGrid.SetTerrain(item, terrainDef);
                map.terrainGrid.SetUnderTerrain(item, terrainDef);
            }

            GenSpawn.Spawn(ThingDefOf.ArchiteCapsule, RoomCellRect.RandomCell, map);
        }
    }

}
