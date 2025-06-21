using RimWorld;
using Verse;

namespace MagicAndMyths
{
    public class EndRoom : RoomTypeWorker
    {
        public override void ApplyRoom(DungeonGenerationContext dungeonGenerationContext, DungeonRoom Room)
        {
            base.ApplyRoom(dungeonGenerationContext, Room);

            if (MagicAndMythDefOf.MagicAndMyths_ReturnPortal != null)
            {
                Building_ReturnPortal returnPortal = (Building_ReturnPortal)ThingMaker.MakeThing(MagicAndMythDefOf.MagicAndMyths_ReturnPortal);
                GenSpawn.Spawn(returnPortal, Room.RoomCellRect.CenterCell, dungeonGenerationContext.Map);
            }
        }
    }
}
