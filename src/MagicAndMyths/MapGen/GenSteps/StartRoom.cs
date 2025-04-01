using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class StartRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, CellRect RoomCellRect)
        {
            foreach (var item in RoomCellRect.Cells)
            {
                map.terrainGrid.SetTerrain(item, TerrainDefOf.MetalTile);
                map.terrainGrid.SetUnderTerrain(item, TerrainDefOf.MetalTile);
            }

            if (MagicAndMythDefOf.MagicAndMythsReturnRune != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMythsReturnRune);
                GenSpawn.Spawn(returnPortal, RoomCellRect.RandomCell, map);
            }
        }
    }

}
