using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public static class CorridoorUtility
    {
        //generate corridors between rooms
        public static List<Corridoor> GenerateCorridors(BspUtility.BspNode roomA, BspUtility.BspNode roomB)
        {
            if (roomA.room == null || roomB.room == null)
                return new List<Corridoor>();

            IntVec3 center1 = roomA.room.CenterCell;
            IntVec3 center2 = roomB.room.CenterCell;

            IntVec3 corner = new IntVec3(center1.x, 0, center2.z);

            List<Corridoor> corridorSegments = new List<Corridoor>();

            Corridoor segment1 = new Corridoor(center1, new IntVec3(corner.x, 0, center1.z));
            Corridoor segment2 = new Corridoor(new IntVec3(corner.x, 0, center1.z), corner);
            Corridoor segment3 = new Corridoor(corner, new IntVec3(center2.x, 0, corner.z));
            Corridoor segment4 = new Corridoor(new IntVec3(center2.x, 0, corner.z), center2);

            FindRoomBoundaryPoints(roomA, roomB, segment1, segment4);

            corridorSegments.Add(segment1);
            corridorSegments.Add(segment2);
            corridorSegments.Add(segment3);
            corridorSegments.Add(segment4);

            return corridorSegments;
        }

        public static void AddRandomConnections(List<BspUtility.BspNode> nodes, int connectionCount)
        {
            if (nodes.Count <= 2)
                return;

            List<Tuple<BspUtility.BspNode, BspUtility.BspNode>> possibleConnections = new List<Tuple<BspUtility.BspNode, BspUtility.BspNode>>();

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    BspUtility.BspNode node1 = nodes[i];
                    BspUtility.BspNode node2 = nodes[j];

                    if (node1.connectedNodes.Contains(node2) || node2.connectedNodes.Contains(node1))
                        continue;


                    float distance = Vector3.Distance(
                        node1.room.CenterCell.ToVector3(),
                        node2.room.CenterCell.ToVector3());

                    float maxConnectDistance = Math.Max(node1.room.Width, node1.room.Height) * 2.5f;

                    if (distance > maxConnectDistance)
                        continue;

                    possibleConnections.Add(new Tuple<BspUtility.BspNode, BspUtility.BspNode>(node1, node2));
                }
            }

            possibleConnections.Shuffle();
            int connectionsToAdd = Math.Min(connectionCount, possibleConnections.Count);

            for (int i = 0; i < connectionsToAdd; i++)
            {
                var connection = possibleConnections[i];
                connection.Item1.connectedNodes.Add(connection.Item2);
                connection.Item2.connectedNodes.Add(connection.Item1);
            }
        }

        public static List<Corridoor> GenerateCorridorPoints(BspUtility.BspNode node1, BspUtility.BspNode node2)
        {
            return GenerateCorridors(node1, node2);
        }

        public static void ApplyCorridorsToGrid(List<Corridoor> corridors, Map map, BoolGrid grid)
        {
            foreach (var corridor in corridors)
            {
                foreach (IntVec3 cell in corridor.path)
                {
                    if (cell.InBounds(map))
                    {
                        grid[cell] = true;
                    }
                }
            }
        }
        private static void FindRoomBoundaryPoints(BspUtility.BspNode roomA, BspUtility.BspNode roomB,
                                                Corridoor firstSegment, Corridoor lastSegment)
        {
            // Find where the first corridor segment exits roomA
            if (roomA.roomWalls != null && roomA.roomWalls.EdgeCells.Any())
            {
                foreach (IntVec3 cell in firstSegment.path)
                {
                    if (IsOnRoomBoundary(cell, roomA.roomWalls.EdgeCells))
                    {
                        firstSegment.RoomAEntryPoint = cell;
                        break;
                    }
                }
            }

            // Find where the last corridor segment enters roomB
            if (roomB.roomWalls != null && roomB.roomWalls.EdgeCells.Any())
            {
                foreach (IntVec3 cell in lastSegment.path.AsEnumerable().Reverse())
                {
                    if (IsOnRoomBoundary(cell, roomB.roomWalls.EdgeCells))
                    {
                        lastSegment.RoomBEntryPoint = cell;
                        break;
                    }
                }
            }
        }

        private static bool IsOnRoomBoundary(IntVec3 cell, IEnumerable<IntVec3> roomEdgeCells)
        {
            if (roomEdgeCells.Contains(cell))
                return true;

            foreach (IntVec3 adj in GenAdjFast.AdjacentCellsCardinal(cell))
            {
                if (roomEdgeCells.Contains(adj))
                    return true;
            }

            return false;
        }
    }
}
