using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EndRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, DungeonRoom Room)
        {
            foreach (var item in Room.roomCellRect.Cells)
            {
                map.terrainGrid.SetTerrain(item, TerrainDefOf.FlagstoneSandstone);
                map.terrainGrid.SetUnderTerrain(item, TerrainDefOf.FlagstoneSandstone);
            }

            if (MagicAndMythDefOf.MagicAndMyths_ReturnPortal != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_ReturnPortal);
                GenSpawn.Spawn(returnPortal, Room.roomCellRect.CenterCell, map);
            }
        }
    }

}
