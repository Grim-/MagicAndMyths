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
            //fnd a suitable room pair connected by a corridor
            var potentialDoorConnections = dungeon.GetAllRooms()
                .SelectMany(r => r.connections)
                .Where(c => c.corridors != null && c.corridors.Any())
                .ToList();

            if (!potentialDoorConnections.Any())
            {
                Log.Message("ObstacleWorker_KeyAndDoor: No suitable corridor connections found for door placement.");
                return false;
            }

            var selectedConnection = potentialDoorConnections.RandomElement();
            DungeonRoom roomBefore = selectedConnection.roomA;
            DungeonRoom roomAfter = selectedConnection.roomB;
            var corridor = selectedConnection.corridors.First();

            IntVec3 doorPos = IntVec3.Invalid;
            foreach (var cell in corridor.path)
            {
                foreach (var dir in GenAdj.CardinalDirections)
                {
                    IntVec3 adjacent = cell + dir;
                    if (adjacent.InBounds(map) && roomAfter.roomCellRect.Contains(adjacent))
                    {
                        doorPos = cell;
                        break;
                    }
                }
                if (doorPos.IsValid)
                    break;
            }

            if (!doorPos.IsValid || !doorPos.Walkable(map) || map.thingGrid.CellContains(doorPos, Def.doorDef))
            {
                Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a valid door position between {roomBefore.roomCellRect.CenterCell} and {roomAfter.roomCellRect.CenterCell}.");
                return false;
            }

            Building_LockableDoor door = (Building_LockableDoor)GenSpawn.Spawn(MagicAndMythDefOf.DungeonLockedDoor, doorPos, map);
            door.Lock();

            Log.Message($"ObstacleWorker_KeyAndDoor: Placed locked door at {doorPos}.");

            DungeonRoom keyRoom = FindKeyRoom(dungeon, roomBefore, roomAfter);
            if (keyRoom == null)
            {
                Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a suitable room for the key (before {roomAfter.roomCellRect.CenterCell}).");
                door.Destroy();
                return false;
            }

            Log.Message($"ObstacleWorker_KeyAndDoor: Selected key room: {keyRoom.roomCellRect.CenterCell}.");

            IntVec3 keyPos = FindKeyPlacementPosition(map, keyRoom);
            if (!keyPos.IsValid)
            {
                Log.Message($"ObstacleWorker_KeyAndDoor: Could not find a valid placement position for the key in {keyRoom.roomCellRect.CenterCell}.");
                door.Destroy();
                return false;
            }

            // 6. Place the key
            Key keyThing = (Key)GenSpawn.Spawn(MagicAndMythDefOf.DungeonTestKey, keyPos, map);
            door.SetKeyReference(keyThing, KeyColorChoices.RandomElement());
            Log.Message($"ObstacleWorker_KeyAndDoor: Placed key ({Def.keyDef.defName}) at {keyPos} in {keyRoom.roomCellRect.CenterCell}.");

            return true;
        }


        private List<Color> KeyColorChoices = new List<Color>()
        {
            Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.white, Color.cyan
        };

        private DungeonRoom FindKeyRoom(Dungeon dungeon, DungeonRoom roomBefore, DungeonRoom roomAfter)
        {
            HashSet<DungeonRoom> accessibleFromStart = new HashSet<DungeonRoom>();
            Queue<DungeonRoom> queue = new Queue<DungeonRoom>();
            DungeonRoom startRoom = dungeon.GetRoom(dungeon.StartNode);

            if (startRoom == null)
                return null;

            queue.Enqueue(startRoom);
            accessibleFromStart.Add(startRoom);

            while (queue.Count > 0)
            {
                DungeonRoom current = queue.Dequeue();
                foreach (DungeonRoom neighbor in current.connectedRooms)
                {
                    if (neighbor == roomAfter || accessibleFromStart.Contains(neighbor))
                        continue;
                    accessibleFromStart.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            var potentialKeyRooms = accessibleFromStart.Where(r => r != roomBefore).ToList();
            if (potentialKeyRooms.Any())
            {
                return potentialKeyRooms.RandomElement();
            }

            return null;
        }

        private IntVec3 FindKeyPlacementPosition(Map map, DungeonRoom keyRoom)
        {
            for (int i = 0; i < 20; i++)
            {
                IntVec3 randomCell = keyRoom.roomCellRect.Cells.RandomElement();
                if (randomCell.InBounds(map) && randomCell.Walkable(map))
                {
                    return randomCell;
                }
            }
            return IntVec3.Invalid;
        }
    }
}
