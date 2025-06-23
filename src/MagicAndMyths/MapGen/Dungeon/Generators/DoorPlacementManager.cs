using RimWorld;
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
        public List<IntVec3> PlacedDoorPositions { get; private set; }
        public List<Building_Door> PlacedDoors { get; private set; }


        public DoorPlacementManager(DungeonGenerationContext generationContext)
        {
            this.dungeon = generationContext.Dungeon;
            this.PlacedDoorPositions = new List<IntVec3>();
            this.PlacedDoors = new List<Building_Door>();
            this.dungeonGenerationContext = generationContext;
        }

        public void PlaceAllDoors()
        {
            PlacedDoorPositions.Clear();
            foreach (var connection in dungeonGenerationContext.Dungeon.ConnectionManager.AllConnections)
            {
                if (connection.Corridoor != null)
                {
                    var doorInfos = FindOptimalDoorPlacements(connection, connection.SourceRoom, connection.DestinationRoom, connection.Corridoor);

                    foreach (var doorInfo in doorInfos)
                    {
                        if (!PlacedDoorPositions.Contains(doorInfo.Position))
                        {
                            PlacedDoorPositions.Add(doorInfo.Position);

                            if (connection.Corridoor.Width >= 2)
                            {
                               Building_Door doubleDoor = (Building_Door)dungeonGenerationContext.Constructor.PlaceDoubleDoor(doorInfo.Position, doorInfo.Rotation, dungeonGenerationContext.Def.DoorDef);
                            }
                            else
                            {
                                dungeonGenerationContext.Constructor.PlaceDoor(doorInfo.Position, doorInfo.Rotation);
                            }

                            //Log.Message($"<color=yellow>Placed door at {doorInfo.Position} with rotation {doorInfo.Rotation}</color>");

                            // Check and seal any gaps
                            SealCorridorGaps(doorInfo.Position, doorInfo.Rotation, connection.Corridoor);
                        }
                    }
                }
            }
        }

        private void SealCorridorGaps(IntVec3 doorPosition, Rot4 doorRotation, Corridoor corridor)
        {
            var corridorCells = new HashSet<IntVec3>(corridor.path ?? new List<IntVec3> { corridor.Start, corridor.End });

            // Get perpendicular directions to door facing
            var perpendicularDirs = GetPerpendicularDirections(doorRotation);

            // Get all cells occupied by the door
            var doorCells = GetActualDoorCells(doorPosition, doorRotation);

            // From each door cell, check perpendicular directions for gaps
            foreach (var doorCell in doorCells)
            {
                foreach (var dir in perpendicularDirs)
                {
                    IntVec3 currentCell = doorCell + dir;

                    if (doorCells.Contains(currentCell))
                        continue;

                    if (currentCell.InBounds(dungeonGenerationContext.Map))
                    {
                        dungeonGenerationContext.Constructor.BuildWallsToEdge(currentCell, dir, corridorCells);
                    }
                }
            }
        }

        private List<IntVec3> GetActualDoorCells(IntVec3 doorPosition, Rot4 rotation)
        {
            List<IntVec3> doorCells = new List<IntVec3>();

            IEnumerable<Thing> things = dungeonGenerationContext.Map.thingGrid.ThingsAt(doorPosition);
            ILockableDoor door = null;

            foreach (var thing in things)
            {
                if (thing is ILockableDoor)
                {
                    door = (ILockableDoor)thing;
                    break;
                }
            }

            if (door != null)
            {
                doorCells.AddRange(door.Thing.OccupiedRect().Cells);
            }

            return doorCells;
        }

        private List<IntVec3> GetPerpendicularDirections(Rot4 rotation)
        {
            if (rotation == Rot4.North || rotation == Rot4.South)
            {
                return new List<IntVec3> { IntVec3.East, IntVec3.West };
            }
            else
            {
                return new List<IntVec3> { IntVec3.North, IntVec3.South };
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

        private List<DoorPlacementInfo> FindOptimalDoorPlacements(RoomConnection connection, DungeonRoom sourceRoom, DungeonRoom destinationRoom, Corridoor corridor)
        {
            var doorPlacements = new List<DoorPlacementInfo>();
            var corridorCells = new HashSet<IntVec3>(corridor.path ?? new List<IntVec3> { corridor.Start, corridor.End });

            var sourceDoor = FindDoorNearRoom(connection, sourceRoom, corridorCells, corridor);
            if (sourceDoor.HasValue)
            {
                doorPlacements.Add(sourceDoor.Value);
            }

            var destinationDoor = FindDoorNearRoom(connection, destinationRoom, corridorCells, corridor);
            if (destinationDoor.HasValue)
            {
                doorPlacements.Add(destinationDoor.Value);
            }

            return doorPlacements;
        }

        private DoorPlacementInfo? FindDoorNearRoom(RoomConnection connection, DungeonRoom room, HashSet<IntVec3> corridorCells, Corridoor corridor)
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

                        if (doorRotation == Rot4.Invalid)
                        {

                        }

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
            if (corridorDirection == IntVec3.North || corridorDirection == IntVec3.NorthEast)
                return Rot4.North;
            if (corridorDirection == IntVec3.South || corridorDirection == IntVec3.SouthWest)
                return Rot4.South;
            if (corridorDirection == IntVec3.East || corridorDirection == IntVec3.SouthEast)
                return Rot4.East;
            if (corridorDirection == IntVec3.West ||corridorDirection == IntVec3.NorthWest)
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