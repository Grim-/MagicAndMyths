using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MagicAndMyths
{
    public class Dungeon : IExposable
    {
        public Map Map;
        public DungeonGenDef Def;
        public BspNode RootNode;
        public List<BspNode> LeafNodes = new List<BspNode>();
        public BspNode StartNode;
        public BspNode EndNode;

        public Dictionary<BspNode, DungeonRoom> nodeToRoomMap = new Dictionary<BspNode, DungeonRoom>();
        private List<BspNode> sidePathNodes = new List<BspNode>();
        private HashSet<DungeonRoom> hiddenRooms = new HashSet<DungeonRoom>();

        public DungeonGridManager GridManager { get; private set; }
        public DungeonRoomPathFinder Pathfinder { get; private set; }
        public DungeonQueryManager QueryManager { get; private set; }
        public DungeonConnectionManager ConnectionManager { get; private set; }





        private List<BspNode> nodeToRoomMapWorkingKeys = new List<BspNode>();
        private List<DungeonRoom> nodeToRoomMapWorkingValues = new List<DungeonRoom>();
        private List<DungeonRoom> hiddenRoomsWorkingList = new List<DungeonRoom>();

        public IReadOnlyList<BspNode> SidePathNodes => sidePathNodes.AsReadOnly();
        public IReadOnlyCollection<DungeonRoom> HiddenRooms => hiddenRooms;
        public IReadOnlyList<BspNode> BSPNodes => nodeToRoomMap.Keys.ToList();
        public IReadOnlyList<DungeonRoom> Rooms => nodeToRoomMap.Values.ToList();
        public IReadOnlyList<DungeonRoom> NormalRooms => nodeToRoomMap.Values.Where(x=> x.def.roomType == RoomType.Normal).ToList();
        public Dungeon()
        {
        }

        public Dungeon(Map map)
        {
            InitializeForMap(map);
        }

        public void InitializeForMap(Map map)
        {
            Map = map;
            GridManager = new DungeonGridManager(map);
            Pathfinder = new DungeonRoomPathFinder();
            QueryManager = new DungeonQueryManager();
            ConnectionManager = new DungeonConnectionManager(this, map);
;
        }

        // BSP and Room Management
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

        public void AddRoom(BspNode node, DungeonRoom room)
        {
            nodeToRoomMap[node] = room;
        }

        public DungeonRoom GetRoom(BspNode node)
        {
            return nodeToRoomMap.TryGetValue(node, out var room) ? room : null;
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
            if (nodeToRoomMap.Values != null)
            {
                return nodeToRoomMap.Values;
            }

            return new Dictionary<BspNode, DungeonRoom>().Values;
        }

        public IEnumerable<DungeonRoom> GetAllCriticalPathRooms()
        {
            return nodeToRoomMap.Values.Where(x => x.IsOnCriticalPath);
        }

        public IEnumerable<KeyValuePair<BspNode, DungeonRoom>> GetAllMappings()
        {
            return nodeToRoomMap;
        }

        // Side Path Management
        public void AddSidePathNode(BspNode node)
        {
            if (!sidePathNodes.Contains(node))
            {
                sidePathNodes.Add(node);
            }
        }

        public List<DungeonRoom> GetAllSidePathRooms()
        {
            return GetAllRooms()
                .Where(r => !r.IsOnCriticalPath && r.HasTag("side_path"))
                .ToList();
        }


        public void MarkRoomAsHidden(DungeonRoom room)
        {
            if (!hiddenRooms.Contains(room))
            {
                hiddenRooms.Add(room);
                room.AddTag("hidden");
            }
        }

        public DungeonRoom GetRandomHiddenRoom()
        {
            return hiddenRooms.Any() ? hiddenRooms.RandomElement() : null;
        }

        public void ConnectRooms(DungeonRoom roomA, DungeonRoom roomB)
        {
            roomA.AddConnectionTo(Map, roomB);
            roomB.AddConnectionTo(Map, roomA);
        }

        public void MarkCellAsFloor(IntVec3 cell)
        {
            GridManager.MarkCellAsFloor(cell);
        }

        public void MarkCellAsWall(IntVec3 cell)
        {
            GridManager.MarkCellAsWall(cell);
        }

        public bool IsCellFloor(IntVec3 cell)
        {
           return GridManager.IsCellFloor(cell);
        }

        public void MarkCellProtected(IntVec3 cell, bool isprotected)
        {
            GridManager.MarkCellProtected(cell, isprotected);
        }

        public void MarkCellsProtected(IEnumerable<IntVec3> cells, bool isprotected)
        {
            GridManager.MarkCellsProtected(cells, isprotected);
        }

        public bool IsRoomCell(IntVec3 c)
        {
            return Rooms.Any(x => x.roomCells.Contains(c));
        }

        public bool IsPathCell(IntVec3 c)
        {
            return Rooms.Any(x => x.connections.Any(y => y.CellIsOnCorridoor(c)));
        }

        public int GetRoomTypeCount(RoomTypeDef roomTypeDef)
        {
            return Rooms.Count(x => x.def == roomTypeDef);
        }

        public DungeonRoom GetFurthestRoom(DungeonRoom start)
        {
            return Pathfinder.GetFurthestRoom(start);
        }

        public List<DungeonRoom> FindPathBetween(DungeonRoom start, DungeonRoom end)
        {
            return Pathfinder.FindPathBetween(start, end);
        }

        public DungeonRoom FindRoomBefore(DungeonRoom targetRoom)
        {
            if (targetRoom.IsOnCriticalPath && targetRoom.CriticalPathIndex > 0)
            {
                foreach (var connectedRoom in targetRoom.connectedRooms)
                {
                    if (connectedRoom.IsOnCriticalPath && connectedRoom.CriticalPathIndex < targetRoom.CriticalPathIndex)
                    {
                        return connectedRoom;
                    }
                }

                var earlierRooms = Rooms
                    .Where(r => r.IsOnCriticalPath && r.CriticalPathIndex < targetRoom.CriticalPathIndex)
                    .OrderByDescending(r => r.CriticalPathIndex)
                    .ToList();

                if (earlierRooms.Any())
                {
                    return earlierRooms.First();
                }
            }

            return Pathfinder.FindAccessibleRoomsBefore(targetRoom, GetRoom(StartNode));
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref RootNode, "rootNode");
            Scribe_Collections.Look(ref LeafNodes, "leafNodes", LookMode.Deep);
            Scribe_Collections.Look(ref nodeToRoomMap, "nodeToRoomMap", LookMode.Deep, LookMode.Deep,
                ref nodeToRoomMapWorkingKeys, ref nodeToRoomMapWorkingValues);
            Scribe_Defs.Look(ref Def, "def");
            Scribe_Collections.Look(ref sidePathNodes, "sidePathNodes", LookMode.Deep);
            Scribe_Collections.Look(ref hiddenRoomsWorkingList, "hiddenRooms", LookMode.Deep);
            Scribe_Deep.Look(ref StartNode, "startNode");
            Scribe_Deep.Look(ref EndNode, "endNode");

            if (GridManager != null)
            {
                GridManager.ExposeData();
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (hiddenRoomsWorkingList != null)
                {
                    hiddenRooms = new HashSet<DungeonRoom>(hiddenRoomsWorkingList);
                }
                else
                {
                    hiddenRooms = new HashSet<DungeonRoom>();
                }
            }
            else if (Scribe.mode == LoadSaveMode.Saving)
            {
                hiddenRoomsWorkingList = new List<DungeonRoom>(hiddenRooms);
            }
        }
    }
}