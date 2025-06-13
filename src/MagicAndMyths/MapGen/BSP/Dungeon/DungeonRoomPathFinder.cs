using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    // Handles pathfinding and room accessibility
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
    }
}