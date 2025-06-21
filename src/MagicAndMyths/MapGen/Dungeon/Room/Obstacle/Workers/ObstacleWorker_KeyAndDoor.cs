using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class ObstacleDef_KeyAndDoor : ObstacleDef
    {
        public ThingDef keyDef;
        public ThingDef doorDef;
        public ThingDef doorStuffing;
        public List<Color> KeyColorChoices = new List<Color>()
        {
            Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.white, Color.cyan
        };
        public ObstacleDef_KeyAndDoor()
        {
            workerClass = typeof(ObstacleWorker_KeyAndDoor);
        }
    }

    public class ObstacleWorker_KeyAndDoor : ObstacleWorker
    {
        public ObstacleDef_KeyAndDoor Def => (ObstacleDef_KeyAndDoor)def;

        public override bool TryPlaceObstacles(Map map, Dungeon dungeon, DungeonRoom room)
        {
            var existingDoors = map.listerBuildings.allBuildingsNonColonist
                .Where(door => door is ILockableDoor)
                .ToList();

            if (!existingDoors.Any())
            {
                Log.Message("ObstacleWorker_KeyAndDoor: No suitable doors found to lock.");
                return false;
            }

            var selectedDoor = existingDoors.RandomElement();
            if (selectedDoor is ILockableDoor lockableDoor)
            {
                Log.Message($"ObstacleWorker_KeyAndDoor: Attempting to lock door at {lockableDoor.Position}.");

                DungeonRoom keyRoom = FindKeyRoom(dungeon, lockableDoor.Position, room);
                if (keyRoom == null)
                {
                    Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a suitable room for the key.");
                    return false;
                }

                Log.Message($"ObstacleWorker_KeyAndDoor: Selected key room: {keyRoom.RoomCellRect.CenterCell}.");

                IntVec3 keyPos = FindKeyPlacementPosition(map, keyRoom);
                if (!keyPos.IsValid)
                {
                    Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a valid placement position for the key in {keyRoom.RoomCellRect.CenterCell}.");
                    return false;
                }

                lockableDoor.Lock();
                Thing_Key keyThing = (Thing_Key)GenSpawn.Spawn(MagicAndMythDefOf.DungeonTestKey, keyPos, map);
                lockableDoor.SetKeyReference(keyThing, Def.KeyColorChoices.RandomElement());

                Log.Message($"ObstacleWorker_KeyAndDoor: Successfully placed key at {keyPos} in room {keyRoom.RoomCellRect.CenterCell} for door at {lockableDoor.Position}.");
                return true;
            }

            return false;
        }

        private DungeonRoom FindKeyRoom(Dungeon dungeon, IntVec3 doorPosition, DungeonRoom startRoom)
        {
            if (startRoom == null)
                return null;

            RoomPair roomPair = dungeon.Pathfinder.FindRoomsSeparatedByDoor(doorPosition, dungeon);
            if (!roomPair.IsValid())
            {
                Log.Warning($"Could not find rooms separated by door at {doorPosition}");
                return null;
            }

            var accessibleRooms = dungeon.Pathfinder.GetRoomsAccessibleFrom(startRoom);
            var keyRoomCandidates = accessibleRooms.Where(r => r != startRoom && r != roomPair.RoomA && r != roomPair.RoomB).ToList();

            if (!keyRoomCandidates.Any())
            {
                keyRoomCandidates = accessibleRooms.Where(r => r != startRoom).ToList();
            }

            if (!keyRoomCandidates.Any())
            {
                Log.Warning($"No accessible rooms found for key placement from start room {startRoom?.RoomCellRect.CenterCell}");
                return null;
            }

            return keyRoomCandidates.RandomElement();
        }

        private IntVec3 FindKeyPlacementPosition(Map map, DungeonRoom keyRoom)
        {
            var walkableCells = keyRoom.roomCells.Where(c => c.InBounds(map) && c.Walkable(map)).ToList();

            if (walkableCells.Any())
            {
                return walkableCells.RandomElement();
            }

            for (int i = 0; i < 20; i++)
            {
                IntVec3 randomCell = keyRoom.roomCells.RandomElement();
                if (randomCell.InBounds(map) && randomCell.Walkable(map))
                {
                    return randomCell;
                }
            }

            return IntVec3.Invalid;
        }
    }
}