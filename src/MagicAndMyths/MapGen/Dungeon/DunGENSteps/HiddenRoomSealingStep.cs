using System.Linq;
using Verse;

namespace MagicAndMyths
{


    public class HiddenRoomSealingStep : IDungeonGenerationStep
    {
        public void Execute(DungeonGenerationContext context)
        {
             var hiddenRooms = context.Dungeon.GetAllRooms()
                .Where(room => room.HasTag("hidden"))
                .ToList();

            if (hiddenRooms.Count == 0)
                return;

            foreach (var hiddenRoom in hiddenRooms)
            {
                SealRoomConnections(context, hiddenRoom);
                context.Map.fogGrid.Refog(hiddenRoom.RoomCellRect.ExpandedBy(2));
            }
        }

        private void SealRoomConnections(DungeonGenerationContext context, DungeonRoom hiddenRoom)
        {
            var connectionsToSeal = context.Dungeon.ConnectionManager
                .GetConnectionsForRoom(hiddenRoom)
                .ToList();

            foreach (var connection in connectionsToSeal)
            {
                SealConnection(context, connection, hiddenRoom);
            }
        }

        private void SealConnection(DungeonGenerationContext context, RoomConnection connection, DungeonRoom hiddenRoom)
        {
            DungeonRoom visibleRoom = connection.roomA == hiddenRoom ? connection.roomB : connection.roomA;
            IntVec3 sealCell = DetermineClosestSealPoint(connection.Corridoor.Start, connection.Corridoor.End, visibleRoom, hiddenRoom);
            PlaceWallAtCell(context, sealCell);
            context.Map.fogGrid.Refog(connection.Corridoor.CellRect.ExpandedBy(2));
        }

        private IntVec3 DetermineClosestSealPoint(IntVec3 startCell, IntVec3 endCell, DungeonRoom visibleRoom, DungeonRoom hiddenRoom)
        {
            float distanceStartToVisible = (startCell - visibleRoom.Center).LengthHorizontalSquared;
            float distanceEndToVisible = (endCell - visibleRoom.Center).LengthHorizontalSquared;

            return distanceStartToVisible < distanceEndToVisible ? startCell : endCell;
        }

        private void PlaceWallAtCell(DungeonGenerationContext context, IntVec3 cell)
        {
            context.Constructor.PlaceThing(cell, MagicAndMythDefOf.EmptyDungeonWall);
        }
    }
}
