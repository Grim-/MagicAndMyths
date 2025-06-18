using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonConnectionManager
    {
        private Dungeon dungeon;
        private Map map;
        private Dictionary<string, RoomConnection> connections;
        public IReadOnlyCollection<RoomConnection> AllConnections => connections.Values;

        public DungeonConnectionManager(Dungeon dungeon, Map map)
        {
            this.dungeon = dungeon;
            this.map = map;
            this.connections = new Dictionary<string, RoomConnection>();
        }

        public RoomConnection ConnectRooms(DungeonRoom roomA, DungeonRoom roomB)
        {
            string connectionId = GetConnectionId(roomA, roomB);

            if (connections.ContainsKey(connectionId))
            {
                return connections[connectionId];
            }

            RoomConnection connection = new RoomConnection(roomA, roomB);
            //probably should be a dungeon gen step
            //connection.corridors = CorridoorUtility.GenerateCorridors(map, roomA, roomB, 1);
            connections[connectionId] = connection;

            if (!roomA.connectedRooms.Contains(roomB))
                roomA.connectedRooms.Add(roomB);
            if (!roomB.connectedRooms.Contains(roomA))
                roomB.connectedRooms.Add(roomA);

            return connection;
        }

        public void DisconnectRooms(DungeonRoom roomA, DungeonRoom roomB)
        {
            string connectionId = GetConnectionId(roomA, roomB);

            if (connections.ContainsKey(connectionId))
            {
                connections.Remove(connectionId);
                roomA.connectedRooms.Remove(roomB);
                roomB.connectedRooms.Remove(roomA);
            }
        }

        public RoomConnection GetConnection(DungeonRoom roomA, DungeonRoom roomB)
        {
            string connectionId = GetConnectionId(roomA, roomB);
            return connections.TryGetValue(connectionId, out var connection) ? connection : null;
        }

        public bool AreRoomsConnected(DungeonRoom roomA, DungeonRoom roomB)
        {
            string connectionId = GetConnectionId(roomA, roomB);
            return connections.ContainsKey(connectionId);
        }

        public List<RoomConnection> GetConnectionsForRoom(DungeonRoom room)
        {
            return connections.Values
                .Where(c => c.roomA == room || c.roomB == room)
                .ToList();
        }

        public void ApplyConnectionsToGrid()
        {
            foreach (var connection in connections.Values)
            {
                foreach (IntVec3 cell in connection.Corridoor.path)
                {
                    dungeon.MarkCellAsFloor(cell);
                }
            }
        }

        public void GenerateConnectionsFromRoomGraph()
        {
            foreach (var room in dungeon.GetAllRooms())
            {
                foreach (var connectedRoom in room.connectedRooms)
                {
                    ConnectRooms(room, connectedRoom);
                }
            }
        }

        private string GetConnectionId(DungeonRoom roomA, DungeonRoom roomB)
        {
            ulong idA = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(roomA);
            ulong idB = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(roomB);
            return idA < idB ? $"{idA}-{idB}" : $"{idB}-{idA}";
        }
    }
}