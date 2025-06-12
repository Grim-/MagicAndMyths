using System.Collections.Generic;
using Verse;

namespace MagicAndMyths
{
    public class CorridorGenerator
    {
        public List<Corridoor> GenerateCorridors(Map map, DungeonRoom roomA, DungeonRoom roomB)
        {
            CorridorPathBase pathGenerator = new LShapedCorridorPath();

            ConnectionPoints connectionPoints = FindOptimalConnectionPoints(roomA, roomB);
            IntVec3 startPoint = connectionPoints.Start;
            IntVec3 endPoint = connectionPoints.End;

            List<IntVec3> corridorPath = pathGenerator.GeneratePathWithWidth(startPoint, endPoint, map);

            Corridoor mainCorridor = new Corridoor(startPoint, endPoint);
            mainCorridor.SetPath(corridorPath);

            return new List<Corridoor> { mainCorridor };
        }

        public CorridorPathBase GetRandomWildCorridorStyle()
        {
            var styles = new CorridorPathBase[]
            {
                new BranchingCorridorPath { branchCount = Rand.Range(1, 4), branchLength = Rand.Range(3f, 8f) },
                new OrganicCorridorPath { noise = Rand.Range(0.3f, 0.8f), smoothingPasses = Rand.Range(1, 4) },
                new ZigzagCorridorPath { segments = Rand.Range(3, 8), zigzagOffset = Rand.Range(2f, 5f) },
                new CurvedCorridorPath { curvature = Rand.Range(0.4f, 1.0f) },
                new DrunkWalkCorridorPath { drunkeness = Rand.Range(0.3f, 0.7f) }
            };

            return styles.RandomElement();
        }

        private ConnectionPoints FindOptimalConnectionPoints(DungeonRoom roomA, DungeonRoom roomB)
        {
            IntVec3 bestA = IntVec3.Invalid;
            IntVec3 bestB = IntVec3.Invalid;
            float bestDistance = float.MaxValue;

            var edgePointsA = GetRoomEdgePoints(roomA.roomCellRect);
            var edgePointsB = GetRoomEdgePoints(roomB.roomCellRect);

            foreach (var pointA in edgePointsA)
            {
                foreach (var pointB in edgePointsB)
                {
                    float distance = (pointA - pointB).LengthHorizontalSquared;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestA = pointA;
                        bestB = pointB;
                    }
                }
            }

            return new ConnectionPoints(bestA, bestB);
        }

        private List<IntVec3> GetRoomEdgePoints(CellRect room)
        {
            List<IntVec3> edgePoints = new List<IntVec3>();

            for (int x = room.minX; x <= room.maxX; x++)
            {
                edgePoints.Add(new IntVec3(x, 0, room.minZ));
                edgePoints.Add(new IntVec3(x, 0, room.maxZ));
            }

            for (int z = room.minZ + 1; z <= room.maxZ - 1; z++)
            {
                edgePoints.Add(new IntVec3(room.minX, 0, z));
                edgePoints.Add(new IntVec3(room.maxX, 0, z));
            }

            return edgePoints;
        }

        private struct ConnectionPoints
        {
            public IntVec3 Start;
            public IntVec3 End;

            public ConnectionPoints(IntVec3 start, IntVec3 end)
            {
                Start = start;
                End = end;
            }
        }
    }
}