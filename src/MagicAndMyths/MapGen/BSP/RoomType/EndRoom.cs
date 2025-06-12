using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EndRoom : RoomTypeWorker
    {
        public override void ApplyRoom(Map map, Dungeon Dungeon, DungeonRoom Room)
        {
            base.ApplyRoom(map, Dungeon, Room);

            if (MagicAndMythDefOf.MagicAndMyths_ReturnPortal != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_ReturnPortal);
                GenSpawn.Spawn(returnPortal, Room.roomCellRect.CenterCell, map);
            }
        }
    }

}
