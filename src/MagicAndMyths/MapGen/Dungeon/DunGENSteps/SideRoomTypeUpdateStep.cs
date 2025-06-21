using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class SideRoomTypeUpdateStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
            // Skip if no side room types are configured
            if (context.Def.availableSideRoomTypes == null || context.Def.availableSideRoomTypes.Count == 0)
                return;

            // Get all side path rooms
            var sidePathRooms = context.Dungeon.GetAllSidePathRooms();
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

                // Check if any side room type with size requirements fits
                if (sideRoomTypesWithRequirements.Any())
                {
                    newRoomType = sideRoomTypesWithRequirements.FirstOrDefault(roomType =>
                        room.RoomCellRect.Width >= roomType.minSizeRequired.x &&
                        room.RoomCellRect.Height >= roomType.minSizeRequired.z);
                }

                // If no sized room fits, pick a random flexible one
                if (newRoomType == null && flexibleSideRoomTypes.Any())
                {
                    newRoomType = flexibleSideRoomTypes.RandomElement();
                }

                // If no side room type available, pick from all available
                if (newRoomType == null && context.Def.availableSideRoomTypes.Any())
                {
                    newRoomType = context.Def.availableSideRoomTypes.RandomElement();
                }

                // Update the room type
                if (newRoomType != null)
                {
                    room.def = newRoomType.def;
                }
            }
        }
    }
}
