using RimWorld;
using System;
using System.Collections.Generic;
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

        public override bool TryPlaceObstacles(Map map, BspUtility.BspNode rootNode, List<BspUtility.BspNode> leafNodes)
        {
            BspUtility.BspNode startRoom = leafNodes.Find(n => n.tag is DungeonRoom dr && dr.type == RoomType.Start);

            var roomPair = FindRoomPairForDoor(leafNodes, startRoom);
            if (roomPair == null)
                return false;

            BspUtility.BspNode roomBefore = roomPair.Item1;
            BspUtility.BspNode roomAfter = roomPair.Item2; 

            IntVec3 doorPos = FindDoorLocation(map, roomBefore, roomAfter);
            if (!doorPos.IsValid)
                return false;

            if (doorPos.GetEdifice(map) is Building wall)
            {
                wall.Destroy();
            }

            Building_Door door = (Building_Door)GenSpawn.Spawn(ThingDefOf.Door, doorPos, map);
            door.SetForbidden(true);

            IntVec3 obstaclePos = FindPlacementPosition(map, roomBefore.room);
            if (!obstaclePos.IsValid)
                return false;

            Obstacle obstacle = (Obstacle)GenSpawn.Spawn(def.obstacleDef, obstaclePos, map);


            CompMechanism_UnlockLinkedDoor unlockMechanism = obstacle.GetComp<CompMechanism_UnlockLinkedDoor>();
            if (unlockMechanism != null)
            {
                unlockMechanism.SetLinkedDoor(door);
            }

            BspUtility.BspNode keyRoom = roomBefore;
            if (def.requiresSeparateRooms)
            {
                HashSet<BspUtility.BspNode> excludedRooms = new HashSet<BspUtility.BspNode> { roomBefore, roomAfter };
                keyRoom = FindSuitableObstacleRoom(leafNodes, startRoom, excludedRooms);
                if (keyRoom == null)
                    keyRoom = roomBefore;
            }

            IntVec3 keyPos = FindPlacementPosition(map, keyRoom.room);
            if (keyPos.IsValid)
            {
                ThingDef keyDef = Def.keyDef;
                GenSpawn.Spawn(keyDef, keyPos, map);
            }

            return true;
        }

        private Tuple<BspUtility.BspNode, BspUtility.BspNode> FindRoomPairForDoor(List<BspUtility.BspNode> leafNodes, BspUtility.BspNode startRoom)
        {
            List<Tuple<BspUtility.BspNode, BspUtility.BspNode>> candidatePairs = new List<Tuple<BspUtility.BspNode, BspUtility.BspNode>>();

            foreach (var node in leafNodes)
            {
                // Skip start room or rooms too close to start
                if (node == startRoom || (startRoom != null && def.minDistanceFromStart > 0))
                {
                    float distance = Vector3.Distance(
                        startRoom.room.CenterCell.ToVector3(),
                        node.room.CenterCell.ToVector3());

                    if (distance < def.minDistanceFromStart)
                        continue;
                }

                // Get connected rooms
                foreach (var connectedNode in node.connectedNodes)
                {
                    if (connectedNode != startRoom)
                    {
                        candidatePairs.Add(new Tuple<BspUtility.BspNode, BspUtility.BspNode>(node, connectedNode));
                    }
                }
            }

            return candidatePairs.Count > 0 ? candidatePairs.RandomElement() : null;
        }

        private IntVec3 FindDoorLocation(Map map, BspUtility.BspNode room1, BspUtility.BspNode room2)
        {
            // Look for potential door spots using the roomWalls EdgeCells
            List<IntVec3> potentialDoorSpots = new List<IntVec3>();

            foreach (IntVec3 cell in room1.roomWalls.EdgeCells)
            {
                // Check if this wall cell is adjacent to the other room
                if (cell.InBounds(map) && IsAdjacentToRoom(cell, room2.room))
                {
                    // If there's no building here, it's likely already a door or corridor
                    if (cell.GetEdifice(map) != null)
                    {
                        potentialDoorSpots.Add(cell);
                    }
                }
            }

            return potentialDoorSpots.Count > 0 ? potentialDoorSpots.RandomElement() : IntVec3.Invalid;
        }

        private bool IsAdjacentToRoom(IntVec3 cell, CellRect room)
        {
            foreach (IntVec3 adj in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (room.Contains(adj))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
