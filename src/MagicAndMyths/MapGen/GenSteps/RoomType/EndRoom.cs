using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EndRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, CellRect RoomCellRect)
        {
            foreach (var item in RoomCellRect.Cells)
            {
                map.terrainGrid.SetTerrain(item, TerrainDefOf.FlagstoneSandstone);
                map.terrainGrid.SetUnderTerrain(item, TerrainDefOf.FlagstoneSandstone);
            }

            if (MagicAndMythDefOf.MagicAndMythsReturnRune != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMythsReturnRune);
                GenSpawn.Spawn(returnPortal, RoomCellRect.CenterCell, map);
            }
        }
    }

}
