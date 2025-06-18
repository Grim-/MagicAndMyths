using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class CorridoorUtility
    {
        public static Corridoor GenerateCorridor(Map map, Dungeon Dungeon, DungeonRoom roomA, DungeonRoom roomB, int width = 2, bool smoothCorners = true)
        {
            ConnectionPoints connectionPoints = FindOptimalConnectionPoints(roomA, roomB);
            IntVec3 startPoint = connectionPoints.Start;
            IntVec3 endPoint = connectionPoints.End;
            CorridorPathBase pathGenerator = GetRandomWildCorridorStyle(startPoint, endPoint, map);
            pathGenerator.smoothCorners = smoothCorners;
            List<IntVec3> corridorPath = pathGenerator.GeneratePathWithWidth(startPoint, endPoint, map, width);

            List<IntVec3> clippedPath = corridorPath.Where(cell =>
            {
                foreach (DungeonRoom room in Dungeon.Rooms)
                {
                    if (room.roomCells.Contains(cell))
                        return false;
                }
                return true;
            }).ToList();

            if (clippedPath.Count > 0)
            {
                startPoint = clippedPath.OrderBy(cell => cell.DistanceToSquared(roomA.Center)).First();
                endPoint = clippedPath.OrderBy(cell => cell.DistanceToSquared(roomB.Center)).First();
            }

            Corridoor mainCorridor = new Corridoor(startPoint, endPoint, width);
            mainCorridor.SetPath(clippedPath);
            return mainCorridor;
        }

        private static ConnectionPoints FindOptimalConnectionPoints(DungeonRoom roomA, DungeonRoom roomB)
        {
            IntVec3 startPoint = roomA.Center;
            IntVec3 endPoint = roomB.Center;
            return new ConnectionPoints(startPoint, endPoint);
        }

        public static CorridorPathBase GetRandomWildCorridorStyle(IntVec3 start, IntVec3 end, Map map)
        {
            var styles = new CorridorPathBase[]
            {
                new StraightCorridorPath(),
            };
            return styles.Where(x => x.FitnessTest(start, end, map)).RandomElement();
        }
    }
}
