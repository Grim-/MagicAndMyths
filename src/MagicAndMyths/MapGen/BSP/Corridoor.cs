using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    //a the empty cells between rooms
    public class Corridoor
    {
        public IntVec3 Start;
        public IntVec3 End;
        public IntVec3 RoomAEntryPoint;
        public IntVec3 RoomBEntryPoint;
        public List<IntVec3> path;

        public CellRect CellRect => new CellRect(
            Mathf.Min(Start.x, End.x),
            Mathf.Min(Start.z, End.z),
            Mathf.Abs(End.x - Start.x) + 1,
            Mathf.Abs(End.z - Start.z) + 1
        );

        public Corridoor(IntVec3 start, IntVec3 end)
        {
            Start = start;
            End = end;
            path = new List<IntVec3>();
            RoomAEntryPoint = Start;
            RoomBEntryPoint = End;
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
    }
}
