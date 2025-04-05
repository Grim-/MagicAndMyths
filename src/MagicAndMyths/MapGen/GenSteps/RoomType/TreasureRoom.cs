using RimWorld;
using System.Collections.Generic;
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
                DungeonUtil.SpawnTerrain(map, item, terrainDef);
            }

            GenSpawn.Spawn(ThingDefOf.ArchiteCapsule, RoomCellRect.CenterCell, map);
        }
    }
}
