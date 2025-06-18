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
            // Find existing doors placed by DoorPlacementManager
            var existingDoors = map.listerBuildings.allBuildingsNonColonist
                .Where(door => door is ILockableDoor)
                .ToList();

            if (!existingDoors.Any())
            {
                Log.Message("ObstacleWorker_KeyAndDoor: No suitable doors found to lock.");
                return false;
            }

            // Select a random door to lock
            var selectedDoor = existingDoors.RandomElement();

            if (selectedDoor is ILockableDoor lockableDoor)
            {
                Log.Message($"ObstacleWorker_KeyAndDoor: Locked door at {lockableDoor.Position}.");
                lockableDoor.Lock();


                // Find room to place key (accessible from start but not requiring going through the locked door)
                DungeonRoom keyRoom = FindKeyRoom(dungeon, lockableDoor.Position, room);
                if (keyRoom == null)
                {
                    Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a suitable room for the key.");
                    lockableDoor.Unlock();
                    return false;
                }

                Log.Message($"ObstacleWorker_KeyAndDoor: Selected key room: {keyRoom.RoomCellRect.CenterCell}.");

                IntVec3 keyPos = FindKeyPlacementPosition(map, keyRoom);
                if (!keyPos.IsValid)
                {
                    Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a valid placement position for the key in {keyRoom.RoomCellRect.CenterCell}.");
                    lockableDoor.Unlock();
                    return false;
                }

                // Place the key
                Thing_Key keyThing = (Thing_Key)GenSpawn.Spawn(MagicAndMythDefOf.DungeonTestKey, keyPos, map);
                lockableDoor.SetKeyReference(keyThing, Def.KeyColorChoices.RandomElement());
                Log.Message($"ObstacleWorker_KeyAndDoor: Placed key at {keyPos} in {keyRoom.RoomCellRect.CenterCell}.");

                return true;
            }
            return false;
        }

        private DungeonRoom FindKeyRoom(Dungeon dungeon, IntVec3 doorPosition, DungeonRoom startRoom)
        {
            if (startRoom == null)
                return null;
            var accessibleRooms = dungeon.RoomsAccessibleFrom(startRoom).ToList();

            if (accessibleRooms.Count > 1)
            {
                return accessibleRooms.Where(r => r != startRoom).RandomElement();
            }

            return null;
        }

        private IntVec3 FindKeyPlacementPosition(Map map, DungeonRoom keyRoom)
        {
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