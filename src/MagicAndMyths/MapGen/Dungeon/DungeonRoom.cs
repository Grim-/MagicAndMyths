using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DungeonRoom
    {
        protected Dungeon ParentDungeon;

        private CellRect _roomCellRect;
        public CellRect RoomCellRect
        {

            get
            {
                if (_roomCellRect == null)
                {
                    _roomCellRect = CellRect.FromCellList(roomCells);
                }

                return _roomCellRect;
            }
            set => _roomCellRect = value;
        }


        public CellRect roomWalls;
        public List<string> tags = new List<string>();
        public List<DungeonRoom> connectedRooms = new List<DungeonRoom>();
        public List<RoomConnection> connections = new List<RoomConnection>();

        public RoomTypeDef def;
        public float distanceFromStart;


        public List<IntVec3> roomCells;
        public bool IsOnCriticalPath => CriticalPathIndex >= 0;
        public int CriticalPathIndex = -1;
        public bool IsWaypoint = false;
        public IntVec3 Center => RoomCellRect.CenterCell;
        public float ProgressionValue
        {
            get
            {
                if (ParentDungeon == null)
                    return 1f;

                var criticalPathRooms = ParentDungeon.GetAllCriticalPathRooms().ToList();
                if (criticalPathRooms.Count <= 1)
                    return 1f;

                if (IsOnCriticalPath)
                {
                    return (float)CriticalPathIndex / (criticalPathRooms.Count - 1);
                }

                return 1f;
            }
        }

        public int DifficultyTier
        {
            get
            {
                int maxTiers = 5;
                return 5;
            }
        }

   
        public IEnumerable<IntVec3> GetCorners()
        {
            yield return new IntVec3(RoomCellRect.minX, 0, RoomCellRect.minZ);
            yield return new IntVec3(RoomCellRect.minX, 0, RoomCellRect.maxZ);
            yield return new IntVec3(RoomCellRect.maxX, 0, RoomCellRect.minZ);
            yield return new IntVec3(RoomCellRect.maxX, 0, RoomCellRect.maxZ);
        }
        public DungeonRoom(Dungeon dungeon)
        {
            tags = new List<string>();
            connectedRooms = new List<DungeonRoom>();
            ParentDungeon = dungeon;
        }

        public static DungeonRoom FromBspNode(Dungeon dungeon, BspNode node, DungeonGenerationContext context, RoomLayoutData roomLayoutData, int minPadding = 2, float roomSizeFactor = 1f)
        {

            node.GenerateComplexRoomGeometry(context, roomLayoutData, minPadding, roomSizeFactor);
            var dungeonRoom = new DungeonRoom(dungeon)
            {
                RoomCellRect = node.roomRect,
                roomWalls = new CellRect(
                    node.roomRect.minX - 1,
                    node.roomRect.minZ - 1,
                    node.roomRect.Width + 2,
                    node.roomRect.Height + 2),
                roomCells = node.roomCells ?? node.roomRect.Cells.ToList()
            };
            dungeonRoom.def = roomLayoutData.def;
            return dungeonRoom;
        }


        public void SetCriticalPathIndex(int index)
        {
            this.CriticalPathIndex = index;
        }

        public void AddConnectionTo(Map map, DungeonRoom OtherRoom, List<Corridoor> corridoors = null)
        {
            if (!HasConnectionTo(OtherRoom))
            {
                RoomConnection newConnection = new RoomConnection(this, OtherRoom);
                connections.Add(newConnection);
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


        public IEnumerable<IntVec3> EntranceCells()
        {
            foreach (var item in connections)
            {
                if (item.Corridoor != null)
                {
                    yield return item.Corridoor.Start;
                    yield return item.Corridoor.End;
                }
            }
        }
        public IntVec3 GetConnectionPointTo(DungeonRoom otherRoom)
        {
            IntVec3 bestEdgePoint = IntVec3.Invalid;
            float bestDistance = float.MaxValue;

            var myEdgePoints = GetRoomEdgePoints();
            var otherEdgePoints = otherRoom.GetRoomEdgePoints();

            foreach (var myPoint in myEdgePoints)
            {
                foreach (var otherPoint in otherEdgePoints)
                {
                    float distance = myPoint.DistanceTo(otherPoint);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestEdgePoint = myPoint;
                    }
                }
            }

            IntVec3 directionToOther = (otherRoom.Center - bestEdgePoint);
            if (directionToOther.x != 0) directionToOther.x = directionToOther.x > 0 ? 1 : -1;
            if (directionToOther.z != 0) directionToOther.z = directionToOther.z > 0 ? 1 : -1;

            if (directionToOther.x != 0 && directionToOther.z != 0)
            {
                if (Math.Abs(otherRoom.Center.x - bestEdgePoint.x) > Math.Abs(otherRoom.Center.z - bestEdgePoint.z))
                    directionToOther.z = 0;
                else
                    directionToOther.x = 0;
            }

            IntVec3 connectionPoint = bestEdgePoint + directionToOther;

            if (!roomCells.Contains(connectionPoint))
                return connectionPoint;

            foreach (var dir in GenAdj.CardinalDirections)
            {
                IntVec3 candidate = bestEdgePoint + dir;
                if (!roomCells.Contains(candidate))
                    return candidate;
            }

            return bestEdgePoint;
        }

        public List<IntVec3> GetRoomEdgePoints()
        {
            List<IntVec3> edgePoints = new List<IntVec3>();
            HashSet<IntVec3> roomCellsSet = new HashSet<IntVec3>(roomCells);

            foreach (var cell in roomCells)
            {
                bool isEdge = false;
                foreach (var adjacent in GenAdj.CardinalDirections)
                {
                    IntVec3 adjCell = cell + adjacent;
                    if (!roomCellsSet.Contains(adjCell))
                    {
                        isEdge = true;
                        break;
                    }
                }

                if (isEdge)
                {
                    edgePoints.Add(cell);
                }
            }

            return edgePoints;
        }
        public bool CanBuildHere(IntVec3 cell)
        {
            return !EntranceCells().Contains(cell);
        }
        public bool CanBuildHere(CellRect cellRect)
        {
            return !EntranceCells().Any(x=> cellRect.Cells.Contains(x));
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
    }

}
