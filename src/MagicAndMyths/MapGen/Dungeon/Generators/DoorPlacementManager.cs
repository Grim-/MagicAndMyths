using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class DoorPlacementManager
    {
        private readonly Dungeon dungeon;
        private readonly DungeonGenerationContext dungeonGenerationContext;
        public List<IntVec3> PlacedDoors { get; private set; }

        public DoorPlacementManager(DungeonGenerationContext generationContext)
        {
            this.dungeon = generationContext.Dungeon;
            this.PlacedDoors = new List<IntVec3>();
            this.dungeonGenerationContext = generationContext;
        }

        public void PlaceAllDoors()
        {
            PlacedDoors.Clear();
            foreach (var connection in dungeonGenerationContext.Dungeon.ConnectionManager.AllConnections)
            {
                if (connection.Corridoor != null)
                {
                    var doorInfos = FindOptimalDoorPlacements(connection.SourceRoom, connection.DestinationRoom, connection.Corridoor);

                    foreach (var doorInfo in doorInfos)
                    {
                        if (!PlacedDoors.Contains(doorInfo.Position))
                        {
                            PlacedDoors.Add(doorInfo.Position);

                            if (connection.Corridoor.Width >= 2)
                            {
                                dungeonGenerationContext.Constructor.PlaceDoubleDoor(doorInfo.Position, doorInfo.Rotation, dungeonGenerationContext.Def.DoorDef);
                            }
                            else
                            {
                                dungeonGenerationContext.Constructor.PlaceDoor(doorInfo.Position, doorInfo.Rotation);
                            }

                            Log.Message($"<color=yellow>Placed door at {doorInfo.Position} with rotation {doorInfo.Rotation}</color>");
                        }
                    }
                }
            }
        }

        private struct DoorPlacementInfo
        {
            public IntVec3 Position;
            public Rot4 Rotation;

            public DoorPlacementInfo(IntVec3 position, Rot4 rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }

        private List<DoorPlacementInfo> FindOptimalDoorPlacements(DungeonRoom sourceRoom, DungeonRoom destinationRoom, Corridoor corridor)
        {
            var doorPlacements = new List<DoorPlacementInfo>();
            var corridorCells = new HashSet<IntVec3>(corridor.path ?? new List<IntVec3> { corridor.Start, corridor.End });

            // Find door placement near source room
            var sourceDoor = FindDoorNearRoom(sourceRoom, corridorCells, corridor);
            if (sourceDoor.HasValue)
            {
                doorPlacements.Add(sourceDoor.Value);
            }

            // Find door placement near destination room
            var destinationDoor = FindDoorNearRoom(destinationRoom, corridorCells, corridor);
            if (destinationDoor.HasValue)
            {
                doorPlacements.Add(destinationDoor.Value);
            }

            return doorPlacements;
        }

        private DoorPlacementInfo? FindDoorNearRoom(DungeonRoom room, HashSet<IntVec3> corridorCells, Corridoor corridor)
        {
            var roomEdges = room.GetRoomEdgePoints();

            foreach (var edgeCell in roomEdges)
            {
                foreach (var direction in GenAdj.CardinalDirections)
                {
                    IntVec3 corridorCell = edgeCell + direction;

                    if (corridorCells.Contains(corridorCell))
                    {
                        Rot4 doorRotation = GetDoorRotation(direction);

                        if (IsValidDoorPlacement(corridorCell, doorRotation, corridor))
                        {
                            return new DoorPlacementInfo(corridorCell, doorRotation);
                        }
                    }
                }
            }

            return null;
        }

        private Rot4 GetDoorRotation(IntVec3 corridorDirection)
        {
            if (corridorDirection == IntVec3.North)
                return Rot4.North;
            if (corridorDirection == IntVec3.South)
                return Rot4.South;
            if (corridorDirection == IntVec3.East)
                return Rot4.East;
            if (corridorDirection == IntVec3.West)
                return Rot4.West;
            return Rot4.North;
        }

        private bool IsValidDoorPlacement(IntVec3 position, Rot4 rotation, Corridoor corridor)
        {
            if (!position.InBounds(dungeonGenerationContext.Map))
                return false;

            var corridorCells = new HashSet<IntVec3>(corridor.path ?? new List<IntVec3> { corridor.Start, corridor.End });

            var doorSize = GetDoorSize(corridor.Width);
            var doorCells = GetDoorOccupiedCells(position, rotation, doorSize);

            foreach (var cell in doorCells)
            {
                if (!cell.InBounds(dungeonGenerationContext.Map))
                    return false;

                if (!corridorCells.Contains(cell))
                    return false;
            }

            return true;
        }

        private IntVec2 GetDoorSize(int corridorWidth)
        {
            if (corridorWidth >= 2)
            {
                return new IntVec2(2, 1);
            }
            return new IntVec2(1, 1);
        }

        private List<IntVec3> GetDoorOccupiedCells(IntVec3 position, Rot4 rotation, IntVec2 size)
        {
            var cells = new List<IntVec3>();

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    IntVec3 offset = new IntVec3(x, 0, z);
                    IntVec3 rotatedOffset = offset.RotatedBy(rotation);
                    cells.Add(position + rotatedOffset);
                }
            }

            return cells;
        }
    }
}