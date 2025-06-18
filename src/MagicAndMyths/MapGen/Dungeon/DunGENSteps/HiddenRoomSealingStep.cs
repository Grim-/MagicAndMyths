using System.Collections.Generic;
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

            var corridorCells = new HashSet<IntVec3>(connection.Corridoor.path ?? new List<IntVec3>());
            var visibleRoomCells = new HashSet<IntVec3>(visibleRoom.roomCells);

            List<IntVec3> sealPoints = new List<IntVec3>();

            foreach (var corridorCell in corridorCells)
            {
                foreach (var dir in GenAdj.CardinalDirections)
                {
                    IntVec3 adjacentCell = corridorCell + dir;
                    if (visibleRoomCells.Contains(adjacentCell))
                    {
                        sealPoints.Add(corridorCell);
                        break;
                    }
                }
            }

            foreach (var sealPoint in sealPoints)
            {
                SealCorridorAtPoint(context, sealPoint, connection.Corridoor, corridorCells);
            }
        }

        private void SealCorridorAtPoint(DungeonGenerationContext context, IntVec3 sealPoint, Corridoor corridor, HashSet<IntVec3> corridorCells)
        {
            PlaceEmptyWall(context, sealPoint);

            if (corridor.Width > 1)
            {
                foreach (var dir in GenAdj.CardinalDirections)
                {
                    context.Constructor.BuildWallsToEdge(sealPoint, dir, corridorCells, MagicAndMythDefOf.EmptyDungeonWall);
                }
            }
        }

        private void PlaceEmptyWall(DungeonGenerationContext context, IntVec3 cell)
        {
            context.Constructor.ClearCell(cell);
            context.Constructor.PlaceThing(cell, MagicAndMythDefOf.EmptyDungeonWall);
            context.Dungeon.GridManager.MarkCellAsWall(cell);
        }
    }
}