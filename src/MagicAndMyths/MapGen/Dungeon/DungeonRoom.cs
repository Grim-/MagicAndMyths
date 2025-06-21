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

        public CellRect roomWalls = CellRect.Empty;
        public List<string> tags = new List<string>();
        public List<DungeonRoom> connectedRooms = new List<DungeonRoom>();
        public List<RoomConnection> connections = new List<RoomConnection>();

        public RoomTypeDef def;
        public float distanceFromStart;
        public RoomShapeBase roomShape;
        public List<IntVec3> roomCells = new List<IntVec3>();

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

        public void SetRoomCells(List<IntVec3> cells)
        {
            roomCells = cells;
            //_roomCellRect = null;
        }

        public void GenerateRoomGeometry(DungeonGenerationContext context, CellRect bounds, RoomLayoutData roomLayoutData, int minPadding = 1, float roomSizeFactor = 1f)
        {
            if (roomLayoutData?.perferredLayouts == null || roomLayoutData.perferredLayouts.Count == 0)
            {
                roomShape = GetRandomRoomShape();
            }
            else
            {
                var layout = roomLayoutData.perferredLayouts.RandomElement();
                roomShape = layout?.GetWorker() ?? GetRandomRoomShape();
            }

            int roomWidth = (int)(bounds.Width * roomSizeFactor);
            int roomHeight = (int)(bounds.Height * roomSizeFactor);

            if (roomLayoutData != null && roomLayoutData.minSizeRequired != IntVec2.Invalid)
            {
                int minWidth = roomLayoutData.minSizeRequired.x;
                int minHeight = roomLayoutData.minSizeRequired.z;
                roomWidth = Math.Max(minWidth, roomWidth);
                roomHeight = Math.Max(minHeight, roomHeight);
            }

            roomWidth = Math.Min(roomWidth, bounds.Width - (minPadding * 2));
            roomHeight = Math.Min(roomHeight, bounds.Height - (minPadding * 2));

            int roomX = bounds.minX + minPadding + (bounds.Width - (minPadding * 2) - roomWidth) / 2;
            int roomZ = bounds.minZ + minPadding + (bounds.Height - (minPadding * 2) - roomHeight) / 2;

            CellRect roomBounds = new CellRect(roomX, roomZ, roomWidth, roomHeight);
            roomCells = roomShape.GenerateRoomCells(context, roomBounds, 1f);

            if (roomCells == null || roomCells.Count == 0)
            {
                roomCells = new List<IntVec3>();
                for (int x = roomBounds.minX; x <= roomBounds.maxX; x++)
                {
                    for (int z = roomBounds.minZ; z <= roomBounds.maxZ; z++)
                    {
                        roomCells.Add(new IntVec3(x, 0, z));
                    }
                }
            }

            if (roomCells.Count > 0)
            {
                int minX = roomCells.Min(c => c.x);
                int maxX = roomCells.Max(c => c.x);
                int minZ = roomCells.Min(c => c.z);
                int maxZ = roomCells.Max(c => c.z);
                RoomCellRect = CellRect.FromCellList(roomCells);
            }
        }

        private RoomShapeBase GetRandomRoomShape()
        {
            RoomShapeBase[] shapes = {
                new RectangleRoomShape(),
                new CircularRoomShape(),
                new CrossRoomShape(),
                new BlobRoomShape()
            };
            return shapes.RandomElement();
        }

        public static DungeonRoom FromBspNode(Dungeon dungeon, BspNode node, DungeonGenerationContext context, RoomLayoutData roomLayoutData, int minPadding = 2, float roomSizeFactor = 1f)
        {
            var dungeonRoom = new DungeonRoom(dungeon);

            dungeonRoom.GenerateRoomGeometry(context, node.rect, roomLayoutData, minPadding, roomSizeFactor);

            dungeonRoom.roomWalls = new CellRect(
                dungeonRoom.RoomCellRect.minX - 1,
                dungeonRoom.RoomCellRect.minZ - 1,
                dungeonRoom.RoomCellRect.Width + 2,
                dungeonRoom.RoomCellRect.Height + 2);

            dungeonRoom.def = roomLayoutData.def;

            foreach (var tag in node.tags)
            {
                dungeonRoom.AddTag(tag);
            }

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

        public bool HasAnyForwardConnections()
        {
            return connections.Any(x => x.SourceRoom == this);
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

        public IntVec3 GetOptimalConnectionPoint(DungeonRoom targetRoom)
        {
            IntVec3 direction = targetRoom.Center - this.Center;
            CellRect sourceRect = this.RoomCellRect;

            List<IntVec3> candidatePoints = new List<IntVec3>();

            if (Math.Abs(direction.x) > Math.Abs(direction.z))
            {
                int faceX = direction.x > 0 ? sourceRect.maxX : sourceRect.minX;
                int centerZ = sourceRect.CenterCell.z;
                int range = Math.Min(3, sourceRect.Height / 3);

                for (int z = centerZ - range; z <= centerZ + range; z++)
                {
                    IntVec3 candidate = new IntVec3(faceX, 0, z);
                    if (this.roomCells.Contains(candidate))
                        candidatePoints.Add(candidate);
                }
            }
            else
            {
                int faceZ = direction.z > 0 ? sourceRect.maxZ : sourceRect.minZ;
                int centerX = sourceRect.CenterCell.x;
                int range = Math.Min(3, sourceRect.Width / 3);

                for (int x = centerX - range; x <= centerX + range; x++)
                {
                    IntVec3 candidate = new IntVec3(x, 0, faceZ);
                    if (this.roomCells.Contains(candidate))
                        candidatePoints.Add(candidate);
                }
            }

            if (candidatePoints.Count == 0)
                return this.Center;

            return candidatePoints.OrderBy(p => p.DistanceToSquared(targetRoom.Center)).First();
        }

        public IntVec3 GetBestEdgePoint(DungeonRoom room, IntVec3 direction)
        {
            var edgePoints = room.GetRoomEdgePoints();

            int primaryAxis = Math.Abs(direction.x) > Math.Abs(direction.z) ? 0 : 1;

            var validEdgePoints = edgePoints.Where(point =>
            {
                IntVec3 pointDirection = point - room.Center;

                if (primaryAxis == 0)
                    return Math.Sign(pointDirection.x) == Math.Sign(direction.x);
                else
                    return Math.Sign(pointDirection.z) == Math.Sign(direction.z);
            }).ToList();

            if (validEdgePoints.Count == 0)
                validEdgePoints = edgePoints;

            IntVec3 targetEdgeCenter = room.Center;
            if (primaryAxis == 0)
                targetEdgeCenter.x += Math.Sign(direction.x) * (room.RoomCellRect.Width / 2);
            else
                targetEdgeCenter.z += Math.Sign(direction.z) * (room.RoomCellRect.Height / 2);

            return validEdgePoints.OrderBy(point => point.DistanceToSquared(targetEdgeCenter)).First();
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
            return !EntranceCells().Any(x => cellRect.Cells.Contains(x));
        }

        public void AddTag(string tag)
        {
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        public void RemoveTag(string tag)
        {
            if (HasTag(tag))
            {
                tags.Remove(tag);
            }
        }

        public void MarkAsMainRoom()
        {
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
