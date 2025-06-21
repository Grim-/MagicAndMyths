using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class SideRoomTypeUpdateStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            if (context.Def.availableSideRoomTypes == null || context.Def.availableSideRoomTypes.Count == 0)
                return;

            var sidePathRooms = context.Dungeon.GetAllSidePathRooms().Where(x=> x.def == null).ToList();
            if (!sidePathRooms.Any())
                return;

            var sideRoomTypesWithRequirements = context.Def.availableSideRoomTypes
                .Where(r => r.minSizeRequired != IntVec2.Invalid)
                .ToList();

            var flexibleSideRoomTypes = context.Def.availableSideRoomTypes
                .Where(r => r.minSizeRequired == IntVec2.Invalid)
                .ToList();

            foreach (var room in sidePathRooms)
            {
                RoomLayoutData newRoomType = null;

                //any side room type with size requirements fits
                if (sideRoomTypesWithRequirements.Any())
                {
                    newRoomType = sideRoomTypesWithRequirements.FirstOrDefault(roomType =>
                        room.RoomCellRect.Width >= roomType.minSizeRequired.x &&
                        room.RoomCellRect.Height >= roomType.minSizeRequired.z);
                }

                //no sized room fits
                if (newRoomType == null && flexibleSideRoomTypes.Any())
                {
                    newRoomType = flexibleSideRoomTypes.RandomElement();
                }


                if (newRoomType == null && context.Def.availableSideRoomTypes.Any())
                {
                    newRoomType = context.Def.availableSideRoomTypes.RandomElement();
                }

                if (newRoomType != null)
                {
                    room.def = newRoomType.def;
                }
            }
        }
    }
}
