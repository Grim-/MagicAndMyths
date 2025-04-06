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

            if (MagicAndMythDefOf.MagicAndMythsReturnRune != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMythsReturnRune);
                GenSpawn.Spawn(returnPortal, Room.roomCellRect.CenterCell, map);
            }
        }
    }

}
