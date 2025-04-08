using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class DungeonRoom
    {
        protected Dungeon ParentDungeon;
        public CellRect roomCellRect;
        public CellRect roomWalls;
        public List<string> tags = new List<string>();
        public List<DungeonRoom> connectedRooms = new List<DungeonRoom>();
        public List<RoomConnection> connections = new List<RoomConnection>();

        public RoomTypeDef def;
        public float distanceFromStart;
        public bool IsOnCriticalPath => CriticalPathIndex >= 0;
        public int CriticalPathIndex = -1;
        public bool IsWaypoint = false;
        public IntVec3 Center => roomCellRect.CenterCell;

        public DungeonRoom(Dungeon dungeon)
        {
            tags = new List<string>();
            connectedRooms = new List<DungeonRoom>();
            ParentDungeon = dungeon;
        }

        public static DungeonRoom FromBspNode(Dungeon dungeon, BspNode node, int minPadding = 2, float roomSizeFactor = 0.95f)
        {
            if (node.roomRect.Area == 0)
            {
                node.roomRect = node.GenerateRoomGeometry(minPadding, roomSizeFactor);
            }
            var dungeonRoom = new DungeonRoom(dungeon)
            {
                roomCellRect = node.roomRect,
                roomWalls = new CellRect(
                    node.roomRect.minX - 1,
                    node.roomRect.minZ - 1,
                    node.roomRect.Width + 2,
                    node.roomRect.Height + 2)
            };
            return dungeonRoom;
        }

        public void SetCriticalPathIndex(int index)
        {
            this.CriticalPathIndex = index;
        }

        public void AddConnectionTo(Map map, DungeonRoom OtherRoom)
        {
            if (!HasConnectionTo(OtherRoom))
            {
                RoomConnection newConnection = new RoomConnection(this, OtherRoom);
                newConnection.corridors = CorridoorUtility.GenerateCorridors(map, this, OtherRoom);
                connections.Add(newConnection);
                ParentDungeon.MarkCellsProtected(newConnection.GetAllCells(), true);
            }
        }
        public void RemoveConnectionTo(DungeonRoom OtherRoom)
        {
            if (HasConnectionTo(OtherRoom))
            {
                connections.RemoveWhere(x => x.DestinationRoom == OtherRoom);
            }
        }

        public bool HasConnectionTo(DungeonRoom OtherRoom)
        {
            return connections.Any(x => x.DestinationRoom == OtherRoom);
        }


        public bool IsConnectedTo(DungeonRoom OtherRoom)
        {
            return connectedRooms.Contains(OtherRoom);
        }

        public void AddTag(string tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }

        public static string GetConnectionId(DungeonRoom room1, DungeonRoom room2)
        {
            ulong id1 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(room1);
            ulong id2 = (ulong)System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(room2);
            return id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";
        }

        /// <summary>
        /// Gets all rooms that are ahead of this room on the critical path
        /// </summary>
        public List<DungeonRoom> GetRoomsAhead()
        {
            if (!IsOnCriticalPath)
                return new List<DungeonRoom>();

            return connectedRooms
                .Where(r => r.IsOnCriticalPath && r.CriticalPathIndex > CriticalPathIndex)
                .ToList();
        }

        /// <summary>
        /// Gets all rooms that are behind this room on the critical path
        /// </summary>
        public List<DungeonRoom> GetRoomsBehind()
        {
            if (!IsOnCriticalPath)
                return new List<DungeonRoom>();

            return connectedRooms
                .Where(r => r.IsOnCriticalPath && r.CriticalPathIndex < CriticalPathIndex)
                .ToList();
        }

        /// <summary>
        /// Gets all rooms accessible from this room without passing through the blocked room
        /// </summary>
        public HashSet<DungeonRoom> GetAccessibleRoomsExcluding(DungeonRoom blockedRoom)
        {
            HashSet<DungeonRoom> accessible = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> toVisit = new Queue<DungeonRoom>();

            toVisit.Enqueue(this);
            accessible.Add(this);

            while (toVisit.Count > 0)
            {
                DungeonRoom current = toVisit.Dequeue();

                foreach (var neighbor in current.connectedRooms)
                {
                    if (neighbor == blockedRoom || accessible.Contains(neighbor))
                        continue;

                    accessible.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }

            return accessible;
        }

        /// <summary>
        /// Gets rooms that are behind this room and accessible without passing through any room ahead
        /// </summary>
        public List<DungeonRoom> GetAccessibleRoomsBehind()
        {
            if (!IsOnCriticalPath)
                return new List<DungeonRoom>();

            // Get all rooms ahead
            var roomsAhead = GetRoomsAhead();

            // Get all accessible rooms without going through any room ahead
            HashSet<DungeonRoom> accessible = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> toVisit = new Queue<DungeonRoom>();

            toVisit.Enqueue(this);
            accessible.Add(this);

            while (toVisit.Count > 0)
            {
                DungeonRoom current = toVisit.Dequeue();

                foreach (var neighbor in current.connectedRooms)
                {
                    if (roomsAhead.Contains(neighbor) || accessible.Contains(neighbor))
                        continue;

                    accessible.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }

            return accessible
                .Where(r => r != this && r.IsOnCriticalPath && r.CriticalPathIndex < CriticalPathIndex)
                .ToList();
        }
    }
}
