using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{

    public class Corridoor
    {
        public IntVec3 Start;
        public IntVec3 End;
        public IntVec3 RoomAEntryPoint;
        public IntVec3 RoomBEntryPoint;
        public List<IntVec3> path;
        public int Width { get; set; } = 1;

        public CellRect CellRect => new CellRect(
            Mathf.Min(Start.x, End.x),
            Mathf.Min(Start.z, End.z),
            Mathf.Abs(End.x - Start.x) + 1,
            Mathf.Abs(End.z - Start.z) + 1
        );

        public Corridoor(IntVec3 start, IntVec3 end, int width = 1)
        {
            Start = start;
            End = end;
            Width = width;
            path = new List<IntVec3>();
            RoomAEntryPoint = Start;
            RoomBEntryPoint = End;
        }

        public IntVec3 GetNearestCorridorCellTo(IntVec3 cell)
        {
            IntVec3 nearest = Start;
            float nearestDist = (Start - cell).LengthHorizontalSquared;

            float endDist = (End - cell).LengthHorizontalSquared;
            if (endDist < nearestDist)
            {
                nearest = End;
                nearestDist = endDist;
            }

            foreach (var pathCell in path)
            {
                float pathDist = (pathCell - cell).LengthHorizontalSquared;
                if (pathDist < nearestDist)
                {
                    nearest = pathCell;
                    nearestDist = pathDist;
                }
            }
            return nearest;
        }

        public bool CellOnPath(IntVec3 c)
        {
            return Start == c || End == c || path.Contains(c);
        }

        public bool CellIsOnCorridoor(IntVec3 c)
        {
            return CellOnPath(c);
        }

        public void SetPath(List<IntVec3> pathCells)
        {
            path = pathCells;
        }

        // Get all corridor cells including start, end, and path
        public List<IntVec3> GetAllCorridorCells()
        {
            var allCells = new List<IntVec3> { Start, End };
            if (path != null)
                allCells.AddRange(path);
            return allCells.Distinct().ToList();
        }
    }
    ////a the empty cells between rooms
    //public class Corridoor
    //{
    //    public IntVec3 Start;
    //    public IntVec3 End;
    //    public IntVec3 RoomAEntryPoint;
    //    public IntVec3 RoomBEntryPoint;
    //    public List<IntVec3> path;

    //    public CellRect CellRect => new CellRect(
    //        Mathf.Min(Start.x, End.x),
    //        Mathf.Min(Start.z, End.z),
    //        Mathf.Abs(End.x - Start.x) + 1,
    //        Mathf.Abs(End.z - Start.z) + 1
    //    );

    //    public Corridoor(IntVec3 start, IntVec3 end)
    //    {
    //        Start = start;
    //        End = end;
    //        path = new List<IntVec3>();
    //        RoomAEntryPoint = Start;
    //        RoomBEntryPoint = End;
    //    }
    //    public IntVec3 GetNearestCorridorCellTo(IntVec3 cell)
    //    {
    //        IntVec3 nearest = Start;
    //        float nearestDist = (Start - cell).LengthHorizontalSquared;

    //        float endDist = (End - cell).LengthHorizontalSquared;
    //        if (endDist < nearestDist)
    //        {
    //            nearest = End;
    //            nearestDist = endDist;
    //        }

    //        foreach (var pathCell in path)
    //        {
    //            float pathDist = (pathCell - cell).LengthHorizontalSquared;
    //            if (pathDist < nearestDist)
    //            {
    //                nearest = pathCell;
    //                nearestDist = pathDist;
    //            }
    //        }

    //        return nearest;
    //    }
    //    public bool CellOnPath(IntVec3 c)
    //    {
    //        return Start == c || End == c || path.Contains(c);
    //    }

    //    public bool CellIsOnCorridoor(IntVec3 c)
    //    {
    //        return CellOnPath(c);
    //    }

    //    public void SetPath(List<IntVec3> pathCells)
    //    {
    //        path = pathCells;
    //    }
    //}
}
