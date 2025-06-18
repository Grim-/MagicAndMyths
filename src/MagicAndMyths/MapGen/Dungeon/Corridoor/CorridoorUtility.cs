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

            //remove all cells that clip into the rooms for any reason.
            List<IntVec3> clippedPath = corridorPath.Where(cell =>
            {
                foreach (DungeonRoom room in Dungeon.Rooms)
                {
                    if (room.roomCells.Contains(cell))
                        return false;
                }
                return true;
            }).ToList();

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
                //new BranchingCorridorPath { branchCount = Rand.Range(1, 3), branchLength = Rand.Range(3f, 8f) },
                //new OrganicCorridorPath { noise = Rand.Range(0.5f, 0.8f), smoothingPasses = Rand.Range(5, 14) },
                //new ZigzagCorridorPath { segments = Rand.Range(1, 3), zigzagOffset = Rand.Range(2f, 5f) },
                //new CurvedCorridorPath { curvature = Rand.Range(0.6f, 1.0f) }
            };
            return styles.Where(x => x.FitnessTest(start, end, map)).RandomElement();
        }
    }
}
