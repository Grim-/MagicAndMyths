using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class Dungeon
    {
        public Map Map { get; private set; }
        public BspNode RootNode { get; private set; }
        public List<BspNode> LeafNodes { get; private set; } = new List<BspNode>();
        public Dictionary<BspNode, DungeonRoom> nodeToRoomMap = new Dictionary<BspNode, DungeonRoom>();
        public BoolGrid DungeonGrid { get; private set; }

        public BoolGrid ProtectionGrid { get; private set; }

        public List<BspNode> SidePathNodes { get; private set; } = new List<BspNode>();
        public HashSet<DungeonRoom> HiddenRooms { get; private set; } = new HashSet<DungeonRoom>();
        public List<BspNode> BSPNodes => nodeToRoomMap.Keys.ToList();
        public List<DungeonRoom> Rooms => nodeToRoomMap.Values.ToList();


        public BspNode StartNode { get; private set; }
        public BspNode EndNode { get; private set; }

        public Dungeon(Map map)
        {
            Map = map;
            DungeonGrid = new BoolGrid(map);
            ProtectionGrid = new BoolGrid(map);
        }

        public void MarkCellProtected(IntVec3 cell, bool Protected)
        {
            if (cell.InBounds(Map))
            {
                ProtectionGrid[cell] = Protected;
            }
        }

        public void MarkCellsProtected(IEnumerable<IntVec3> cells, bool Protected)
        {
            foreach (var c in cells)
            {
                if (c.InBounds(Map))
                {
                    ProtectionGrid[c] = Protected;
                }
            }
        }

        public void AddSidePathNode(BspNode node)
        {
            if (!SidePathNodes.Contains(node))
            {
                SidePathNodes.Add(node);
            }
        }

        public void MarkRoomAsHidden(DungeonRoom room)
        {
            if (!HiddenRooms.Contains(room))
            {
                HiddenRooms.Add(room);
                room.AddTag("hidden");
            }
        }
        public bool IsRoomCell(IntVec3 c)
        {
            return Rooms.Any(x => x.roomCellRect.Contains(c));
        }
        public bool IsPathCell(IntVec3 c)
        {
            return Rooms.Any(x => x.connections.Any(y => y.CellIsOnCorridoor(c)));
        }

        public DungeonRoom GetRandomHiddenRoom()
        {
            return HiddenRooms.Any() ? HiddenRooms.RandomElement() : null;
        }

        public List<DungeonRoom> GetAllSidePathRooms()
        {
            return GetAllRooms()
                .Where(r => !r.IsOnCriticalPath && r.HasTag("side_path"))
                .ToList();
        }
        public void AddRoom(BspNode node, DungeonRoom room)
        {
            nodeToRoomMap[node] = room;
        }

        public DungeonRoom GetRoom(BspNode node)
        {
            return nodeToRoomMap.TryGetValue(node, out var room) ? room : null;
        }



        public RoomPair FindRoomPairForDoor(Dungeon dungeon, DungeonRoom currentRoom)
        {
            // If the current room is on the critical path, find a connected room also on the critical path
            if (currentRoom.IsOnCriticalPath)
            {
                foreach (var connectedRoom in currentRoom.connectedRooms)
                {
                    if (connectedRoom.IsOnCriticalPath &&
                        connectedRoom.CriticalPathIndex > currentRoom.CriticalPathIndex)
                    {
                        return new RoomPair(currentRoom, connectedRoom);
                    }
                }
            }

            // Otherwise, just pick any connected room
            if (currentRoom.connectedRooms.Count > 0)
            {
                var connectedRoom = currentRoom.connectedRooms.RandomElement();
                return new RoomPair(currentRoom, connectedRoom);
            }

            return null;
        }


        /// <summary>
        /// Finds a room that is "before" the given room (earlier in the critical path or accessible without going through it)
        /// </summary>
        public DungeonRoom FindRoomBefore(DungeonRoom targetRoom)
        {
            // If the room is on the critical path, try to find a room earlier on the path
            if (targetRoom.IsOnCriticalPath && targetRoom.CriticalPathIndex > 0)
            {
                // First check directly connected rooms with lower index
                foreach (var connectedRoom in targetRoom.connectedRooms)
                {
                    if (connectedRoom.IsOnCriticalPath && connectedRoom.CriticalPathIndex < targetRoom.CriticalPathIndex)
                    {
                        return connectedRoom;
                    }
                }

                // If none found, try to find any room with a lower critical path index
                var earlierRooms = Rooms
                    .Where(r => r.IsOnCriticalPath && r.CriticalPathIndex < targetRoom.CriticalPathIndex)
                    .OrderByDescending(r => r.CriticalPathIndex)  // Prefer rooms closer to target
                    .ToList();

                if (earlierRooms.Any())
                {
                    return earlierRooms.First();
                }
            }

            // If not on critical path or no earlier room found, find rooms accessible without going through target
            HashSet<DungeonRoom> accessibleRooms = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> queue = new Queue<DungeonRoom>();

            // Start from the start room
            DungeonRoom startRoom = GetRoom(StartNode);
            if (startRoom == targetRoom)
            {
                // If target is the start room, there's no "before" room
                // Return the start room itself as a fallback
                return startRoom;
            }

            queue.Enqueue(startRoom);
            accessibleRooms.Add(startRoom);

            while (queue.Count > 0)
            {
                DungeonRoom current = queue.Dequeue();

                foreach (var neighbor in current.connectedRooms)
                {
                    // Don't go through the target room
                    if (neighbor == targetRoom)
                        continue;

                    if (!accessibleRooms.Contains(neighbor))
                    {
                        accessibleRooms.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // Remove rooms that would be "after" the target on critical path
            if (targetRoom.IsOnCriticalPath)
            {
                accessibleRooms.RemoveWhere(r => r.IsOnCriticalPath && r.CriticalPathIndex > targetRoom.CriticalPathIndex);
            }

            // Return a random accessible room, or start room as fallback
            return accessibleRooms.Any() ? accessibleRooms.RandomElement() : startRoom;
        }
        public BspNode GetNode(DungeonRoom room)
        {
            return nodeToRoomMap.FirstOrDefault(x => x.Value == room).Key;
        }

        public bool HasMapping(BspNode node)
        {
            return nodeToRoomMap.ContainsKey(node);
        }

        public IEnumerable<DungeonRoom> GetAllRooms()
        {
            return nodeToRoomMap.Values;
        }

        public IEnumerable<KeyValuePair<BspNode, DungeonRoom>> GetAllMappings()
        {
            return nodeToRoomMap;
        }

        public bool IsIsolatedWall(IntVec3 cell)
        {
            foreach (IntVec3 adj in GenAdjFast.AdjacentCells8Way(cell))
            {
                if (!adj.InBounds(this.Map) || !this.IsCellFloor(adj))
                {
                    return false;
                }
            }

            return true;
        }
        public void SetBspStructure(BspNode rootNode, List<BspNode> leafNodes)
        {
            RootNode = rootNode;
            LeafNodes = leafNodes;
        }

        public void SetCriticalPathEndpoints(BspNode start, BspNode end)
        {
            StartNode = start;
            EndNode = end;
        }


        public DungeonRoom GetNearestRoom(DungeonRoom start)
        {
            Queue<(DungeonRoom room, int dist)> queue = new Queue<(DungeonRoom room, int dist)>();
            HashSet<DungeonRoom> visited = new HashSet<DungeonRoom>();
            queue.Enqueue((start, 0));

            DungeonRoom nearest = start;

            //TODO
            return nearest;
        }


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
        public void MarkCellAsWall(IntVec3 cell)
        {
            if (cell.InBounds(Map))
            {
                DungeonGrid[cell] = false;
            }
        }
        public void MarkCellAsFloor(IntVec3 cell)
        {
            if (cell.InBounds(Map))
            {
                DungeonGrid[cell] = true;
            }
        }

        public bool IsCellFloor(IntVec3 cell)
        {
            return cell.InBounds(Map) && DungeonGrid[cell];
        }

        public static void ConnectRooms(DungeonRoom roomA, DungeonRoom roomB)
        {
            if (!roomA.connectedRooms.Contains(roomB))
                roomA.connectedRooms.Add(roomB);

            if (!roomB.connectedRooms.Contains(roomA))
                roomB.connectedRooms.Add(roomA);
        }
    }
}
