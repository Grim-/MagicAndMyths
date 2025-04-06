using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class CorridoorUtility
    {
        public static List<Corridoor> GenerateCorridors(Map map, DungeonRoom roomA, DungeonRoom roomB)
        {
            List<Corridoor> corridors = new List<Corridoor>();

            IntVec3 centerA = roomA.roomCellRect.CenterCell;
            IntVec3 centerB = roomB.roomCellRect.CenterCell;

            Corridoor mainCorridor = new Corridoor(centerA, centerB);

            IntVec3 startPoint = FindNearestEdgePoint(roomA.roomCellRect, centerB);
            IntVec3 endPoint = FindNearestEdgePoint(roomB.roomCellRect, centerA);

            LShapedCorridorPath straightCorridorPath = new LShapedCorridorPath();
            mainCorridor.path = straightCorridorPath.GeneratePath(startPoint, endPoint, map);
            corridors.Add(mainCorridor);
            return corridors;
        }

        private static IntVec3 FindNearestEdgePoint(CellRect room, IntVec3 target)
        {
            // Try horizontal alignment first
            if (target.z >= room.minZ && target.z <= room.maxZ)
            {
                if (target.x < room.minX)
                    return new IntVec3(room.minX, 0, target.z);
                else if (target.x > room.maxX)
                    return new IntVec3(room.maxX, 0, target.z);
            }

            // Try vertical alignment
            if (target.x >= room.minX && target.x <= room.maxX)
            {
                if (target.z < room.minZ)
                    return new IntVec3(target.x, 0, room.minZ);
                else if (target.z > room.maxZ)
                    return new IntVec3(target.x, 0, room.maxZ);
            }

            IntVec3 closestPoint = new IntVec3(room.minX, 0, room.minZ);
            float minDist = float.MaxValue;

            // Check each corner
            foreach (IntVec3 corner in GetCorners(room))
            {
                float dist = (corner - target).LengthHorizontalSquared;
                if (dist < minDist)
                {
                    minDist = dist;
                    closestPoint = corner;
                }
            }

            return closestPoint;
        }

        // Helper to get room corners
        private static IEnumerable<IntVec3> GetCorners(CellRect room)
        {
            yield return new IntVec3(room.minX, 0, room.minZ);
            yield return new IntVec3(room.minX, 0, room.maxZ);
            yield return new IntVec3(room.maxX, 0, room.minZ);
            yield return new IntVec3(room.maxX, 0, room.maxZ);
        }
    }
}
