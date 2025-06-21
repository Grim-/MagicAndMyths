using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DungeonRoomPathFinder
    {
        public DungeonRoom GetFurthestRoom(DungeonRoom start)
        {
            Queue<(DungeonRoom room, int dist)> queue = new Queue<(DungeonRoom room, int dist)>();
            HashSet<DungeonRoom> visited = new HashSet<DungeonRoom>();
            queue.Enqueue((start, 0));

            DungeonRoom furthest = start;
            int maxDist = 0;

            while (queue.Count > 0)
            {
                var (room, dist) = queue.Dequeue();
                if (!visited.Add(room))
                    continue;

                if (dist > maxDist)
                {
                    maxDist = dist;
                    furthest = room;
                }

                foreach (var neighbor in room.connectedRooms)
                {
                    queue.Enqueue((neighbor, dist + 1));
                }
            }

            return furthest;
        }

        public List<DungeonRoom> FindPathBetween(DungeonRoom start, DungeonRoom end)
        {
            Queue<DungeonRoom> queue = new Queue<DungeonRoom>();
            Dictionary<DungeonRoom, DungeonRoom> cameFrom = new Dictionary<DungeonRoom, DungeonRoom>();
            HashSet<DungeonRoom> visited = new HashSet<DungeonRoom>();

            queue.Enqueue(start);
            visited.Add(start);
            cameFrom[start] = null;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == end)
                    break;

                foreach (var neighbor in current.connectedRooms)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            List<DungeonRoom> path = new List<DungeonRoom>();
            for (var at = end; at != null; at = cameFrom.ContainsKey(at) ? cameFrom[at] : null)
                path.Insert(0, at);

            return path;
        }
        public float GetPathDistance(DungeonRoom start, DungeonRoom end)
        {
            List<DungeonRoom> path = FindPathBetween(start, end);
            float totalDistance = 0f;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 a = path[i].Center.ToVector3();
                Vector3 b = path[i + 1].Center.ToVector3();

                float manhattan = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
                totalDistance += manhattan;
            }

            return totalDistance;
        }

        public HashSet<DungeonRoom> GetRoomsAccessibleFrom(DungeonRoom start, DungeonRoom excludeRoom = null)
        {
            HashSet<DungeonRoom> accessibleRooms = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> queue = new Queue<DungeonRoom>();

            if (start == null)
                return accessibleRooms;

            queue.Enqueue(start);
            accessibleRooms.Add(start);

            while (queue.Count > 0)
            {
                DungeonRoom current = queue.Dequeue();
                foreach (DungeonRoom neighbor in current.connectedRooms)
                {
                    if (neighbor == excludeRoom || accessibleRooms.Contains(neighbor))
                        continue;
                    accessibleRooms.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            return accessibleRooms;
        }

        public HashSet<DungeonRoom> GetEndRooms(DungeonRoom start)
        {
            HashSet<DungeonRoom> visited = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> queue = new Queue<DungeonRoom>();
            HashSet<DungeonRoom> endRooms = new HashSet<DungeonRoom>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var neighbor in current.connectedRooms)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);

                        if (neighbor.connectedRooms.Count() == 1)
                        {
                            endRooms.Add(neighbor);
                        }
                    }
                }
            }

            return endRooms;
        }

        public DungeonRoom FindAccessibleRoomsBefore(DungeonRoom targetRoom, DungeonRoom startRoom)
        {
            HashSet<DungeonRoom> accessibleRooms = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> queue = new Queue<DungeonRoom>();

            if (startRoom == targetRoom)
                return startRoom;

            queue.Enqueue(startRoom);
            accessibleRooms.Add(startRoom);

            while (queue.Count > 0)
            {
                DungeonRoom current = queue.Dequeue();

                foreach (var neighbor in current.connectedRooms)
                {
                    if (neighbor == targetRoom)
                        continue;

                    if (!accessibleRooms.Contains(neighbor))
                    {
                        accessibleRooms.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (targetRoom.IsOnCriticalPath)
            {
                accessibleRooms.RemoveWhere(r => r.IsOnCriticalPath && r.CriticalPathIndex > targetRoom.CriticalPathIndex);
            }

            return accessibleRooms.Any() ? accessibleRooms.RandomElement() : startRoom;
        }

        public RoomPair FindRoomsSeparatedByDoor(IntVec3 doorPosition, Dungeon dungeon)
        {
            DungeonRoom roomA = null;
            DungeonRoom roomB = null;

            foreach (var room in dungeon.Rooms)
            {
                if (room.RoomCellRect.ExpandedBy(1).Contains(doorPosition))
                {
                    if (roomA == null)
                        roomA = room;
                    else if (roomB == null)
                    {
                        roomB = room;
                        break;
                    }
                }
            }

            return new RoomPair(roomA, roomB);
        }

        public HashSet<DungeonRoom> GetRoomsAccessibleFromExcludingDoor(DungeonRoom start, IntVec3 doorPosition, Dungeon dungeon)
        {
            RoomPair roomPair = FindRoomsSeparatedByDoor(doorPosition, dungeon);

            if (roomPair.RoomA == null || roomPair.RoomB == null)
                return GetRoomsAccessibleFrom(start);

            DungeonRoom excludeRoom = null;
            if (start == roomPair.RoomA)
                excludeRoom = roomPair.RoomB;
            else if (start == roomPair.RoomB)
                excludeRoom = roomPair.RoomA;
            else
            {
                var pathToA = FindPathBetween(start, roomPair.RoomA);
                var pathToB = FindPathBetween(start, roomPair.RoomB);

                if (pathToA.Count <= pathToB.Count)
                    excludeRoom = roomPair.RoomB;
                else
                    excludeRoom = roomPair.RoomA;
            }

            return GetRoomsAccessibleFrom(start, excludeRoom);
        }

        public DungeonRoom FindKeyRoomForDoor(IntVec3 doorPosition, DungeonRoom startRoom, Dungeon dungeon)
        {
            var accessibleRooms = GetRoomsAccessibleFromExcludingDoor(startRoom, doorPosition, dungeon);
            return accessibleRooms.Where(r => r != startRoom).RandomElementWithFallback();
        }
    }
}